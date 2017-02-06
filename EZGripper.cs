/*******************************************************************************
* Software License Agreement (BSD License)
*
* Copyright (c) 2017, SAKE Robotics
* All rights reserved.
* 
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
*    * Redistributions of source code must retain the above copyright
*      notice, this list of conditions and the following disclaimer.
*    * Redistributions in binary form must reproduce the above copyright
*      notice, this list of conditions and the following disclaimer in the
*      documentation and/or other materials provided with the distribution.
*    * Neither the name of the copyright holder nor the names of its
*      contributors may be used to endorse or promote products derived from
*      this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
* AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
* IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
* ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
* LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
* CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
* SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
* INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
* CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
* ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
* POSSIBILITY OF SUCH DAMAGE.
*******************************************************************************/

using System;
using System.Threading;

namespace SakeRobotics
{
    class EZGripper
    {
        const ushort GRIP_MAX = 2500; // maximum open position for grippers
        const ushort TORQUE_MAX = 800;
        const ushort TORQUE_HOLD = 100;

        Sdk_wrapper sdk;
        ushort[] zero_positions;
        byte[] servo_ids;

        public EZGripper(int port, byte[] servo_ids)
        {
            this.sdk = new Sdk_wrapper(port);
            this.servo_ids = servo_ids;
            this.zero_positions = new ushort[servo_ids.Length];
            for (int i = 0; i < zero_positions.Length; i++)
                zero_positions[i] = 0;
        }

        // Scale from 0..100 to 0..to_max
        static ushort scale(ushort n, ushort to_max)
        {
            ushort result = (ushort)(((int)n) * to_max / 100);
            if (result > to_max) { result = to_max; }
            return result;
        }
        // Scale from 0..to_max to 0..100
        static ushort down_scale(ushort n, ushort to_max)
        {
            ushort result = (ushort)(((int)n) * 100 / to_max);
            if (result > 100) { result = 100; }
            return result;
        }

        void wait_for_stop()
        {
            DateTime wait_start = DateTime.Now;
            int last_position = 1000000;
            while (true)
            {
                int current_position = sdk.read2byte(servo_ids[0], 36);
                if (current_position == last_position)
                    break;
                last_position = current_position;
                Thread.Sleep(100);
                if ((DateTime.Now - wait_start).TotalSeconds > 5)
                    break;
            }
        }

        public void calibrate()
        {
            Console.WriteLine("calibrating");

            for (int i = 0; i < servo_ids.Length; i++)
            {
                sdk.write2byte(servo_ids[i], 6, 0x1fff);       // 1) "Multi-Turn" - ON
                sdk.write2byte(servo_ids[i], 8, 0x1fff);       // 
                sdk.write2byte(servo_ids[i], 34, 500);         // 2) "Torque Limit" to 500 (or so)
                sdk.write1byte(servo_ids[i], 24, 0);           // 3) "Torque Enable" to OFF
                sdk.write1byte(servo_ids[i], 70, 1);           // 4) Set "Goal Torque Mode" to ON
                sdk.write2byte(servo_ids[i], 71, 1024 + 100);  // 5) Set "Goal Torque" Direction to CW and Value 100
            }

            Thread.Sleep(4000);                                                     // 6) give it time to stop

            for (int i = 0; i < servo_ids.Length; i++)
            {
                sdk.write2byte(servo_ids[i], 71, 1024 + 10);   // 7) Set "Goal Torque" Direction to CW and Value 10 - reduce load on servo
                sdk.write2byte(servo_ids[i], 20, 0);           // 8) set "Multi turn offset" to 0
                zero_positions[i] = sdk.read2byte(servo_ids[i], 36); // 9) read current position of servo
            }

            Console.WriteLine("calibration done");
        }

        /// <summary>
        /// Moves fingers to the specified position with the specified torque.
        /// </summary>
        /// <param name="position">0..100 - where 0 is closed and 100 is open</param>
        /// <param name="closing_torque">0..100 - where 100 is maximum torque</param>
        public void goto_position(ushort position, ushort closing_torque)
        {
            Console.WriteLine(String.Format("goto_position {0}, torque {1}",position,closing_torque));
            ushort servo_position = scale(position, GRIP_MAX);
            ushort torque = scale(closing_torque, TORQUE_MAX);

            for (int i = 0; i < servo_ids.Length; i++)
            {
                sdk.write2byte(servo_ids[i], 34, torque);
                sdk.write2byte(servo_ids[i], 71, (ushort)(1024 + torque));

                if (position == 0)
                { // close with torque
                    sdk.write1byte(servo_ids[i], 70, 1);
                }
                else
                { // go to position
                    sdk.write1byte(servo_ids[i], 70, 0);
                    sdk.write2byte(servo_ids[i], 30,
                        (ushort)(zero_positions[i] + servo_position));
                }
            }

            wait_for_stop();

            // Sets torque to keep gripper in position, but does not apply torque if there is no load.
            // This does not provide continuous grasping torque.
            ushort hold_torque = Math.Min(torque, TORQUE_HOLD);
            for (int i = 0; i < servo_ids.Length; i++)
            {
                sdk.write2byte(servo_ids[i], 34, hold_torque);
                sdk.write2byte(servo_ids[i], 71, (ushort)(1024 + hold_torque));
            }
            Console.WriteLine("goto_position done");
        }

        public ushort[] get_position()
        {
            ushort[] positions = new ushort[servo_ids.Length];
            for (int i = 0; i < servo_ids.Length; i++)
            {
                positions[i] = sdk.read2byte(servo_ids[i], 36);
                if (positions[i] > zero_positions[i])
                    positions[i] -= zero_positions[i];
                else
                    positions[i] = 0;

                positions[i] = down_scale(positions[i], GRIP_MAX);
            }
            return positions;
        }

        public byte[] get_temperature()
        {
            byte[] temperatures = new byte[servo_ids.Length];
            for (int i = 0; i < servo_ids.Length; i++)
            {
                temperatures[i] = sdk.read1byte(servo_ids[i], 43);
            }
            return temperatures;
        }

        public void release()
        {
            for (int i = 0; i < servo_ids.Length; i++)
                sdk.write1byte(servo_ids[i], 70, 0);
        }
    }
}
