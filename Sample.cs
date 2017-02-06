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
using dynamixel_sdk;
using SakeRobotics;

namespace Sample
{
    class Sample
    {
        const int BAUDRATE = 57600;
        const string DEVICENAME = "COM4";

        static void Main(string[] args)
        {
            int port_num = dynamixel.portHandler(DEVICENAME);
            dynamixel.packetHandler();

            if (dynamixel.openPort(port_num))
            {
                Console.WriteLine("Succeeded to open the port!");
            }
            else
            {
                Console.WriteLine("Failed to open the port!");
                Console.WriteLine("Press any key to terminate...");
                Console.ReadKey();
                return;
            }

            if (dynamixel.setBaudRate(port_num, BAUDRATE))
            {
                Console.WriteLine("Succeeded to change the baudrate!");
            }
            else
            {
                Console.WriteLine("Failed to change the baudrate!");
                Console.WriteLine("Press any key to terminate...");
                Console.ReadKey();
                return;
            }

            byte[] servo_ids = { 1, 2, 3 };
            EZGripper gripper = new EZGripper(port_num, servo_ids);

            try
            {
                Console.WriteLine("temperatures:");
                byte[] temperatures = gripper.get_temperature();
                for (int i = 0; i < temperatures.Length; i++)
                    Console.WriteLine(temperatures[i]);

                gripper.calibrate();
                gripper.goto_position(100, 100); // open
                Thread.Sleep(1000);
                gripper.goto_position(50, 100);

                Console.WriteLine("positions:");
                ushort[] positions = gripper.get_position();
                for (int i = 0; i < positions.Length; i++)
                    Console.WriteLine(positions[i]);

                Thread.Sleep(1000);
                gripper.goto_position(0, 30);

                Console.WriteLine("positions:");
                positions = gripper.get_position();
                for (int i = 0; i < positions.Length; i++)
                    Console.WriteLine(positions[i]);

                Thread.Sleep(1000);
                gripper.goto_position(100, 100);
                Thread.Sleep(1000);
                gripper.release();
            }
            catch (DynamixelException e)
            {
                e.print();
            }

            dynamixel.closePort(port_num);
        }
    }
}
