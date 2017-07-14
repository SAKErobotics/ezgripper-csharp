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
using dynamixel_sdk;

namespace SakeRobotics
{
    public abstract class DynamixelException : Exception
    {
        protected const int PROTOCOL_VERSION1 = 1;
        protected int error_code;
        public DynamixelException(int error_code)
        {
            this.error_code = error_code;
        }
        public abstract void print();
    }
    public class DynamixelTxRxException : DynamixelException
    {
        public DynamixelTxRxException(int error_code) : base(error_code)
        {
        }
        public override void print()
        {
            Console.WriteLine("DynamixelTxRxException: " + error_code);
            dynamixel.printTxRxResult(PROTOCOL_VERSION1, error_code);
        }
    }
    public class DynamixelRxPacketException : DynamixelException
    {
        public DynamixelRxPacketException(int error_code) : base(error_code)
        {
        }
        public override void print()
        {
            Console.WriteLine("DynamixelRxPacketException: " + error_code);
            dynamixel.printRxPacketError(PROTOCOL_VERSION1, (byte)error_code);
        }
    }

    class Sdk_wrapper
    {
        const int PROTOCOL_VERSION1 = 1;
        const int RETRY_COUNT = 3;

        int port_num;

        public Sdk_wrapper(int port_num)
        {
            this.port_num = port_num;
        }
        // Return true if no errors
        public bool checkErrors(bool throw_on_error)
        {
            int dxl_comm_result = dynamixel.getLastTxRxResult(port_num, PROTOCOL_VERSION1);
            if (dxl_comm_result != 0)
            {
                if (throw_on_error)
                    throw new DynamixelTxRxException(dxl_comm_result);

                dynamixel.printTxRxResult(PROTOCOL_VERSION1, dxl_comm_result);
                return false;
            }
            byte dxl_error = dynamixel.getLastRxPacketError(port_num, PROTOCOL_VERSION1);
            if (dxl_error != 0)
            {
                if (throw_on_error)
                    throw new DynamixelRxPacketException(dxl_error);

                dynamixel.printRxPacketError(PROTOCOL_VERSION1, dxl_error);
                return false;
            }
            return true;
        }
        public byte read1byte(byte servo_id, ushort address)
        {
            byte result = 0;
            for (int i = 1; i <= RETRY_COUNT; i++)
            {
                result = dynamixel.read1ByteTxRx(port_num, PROTOCOL_VERSION1, servo_id, address);
                if (i < RETRY_COUNT)
                {
                    if (checkErrors(false))
                        return result; // All is good, return

                    Console.WriteLine("read1byte, retry " + i);
                }
                else
                    checkErrors(true); // Last try - throw an exception if it fails
            }
            return result;
        }
        public ushort read2byte(byte servo_id, ushort address)
        {
            ushort result = 0;
            for (int i = 1; i <= RETRY_COUNT; i++)
            {
                result = dynamixel.read2ByteTxRx(port_num, PROTOCOL_VERSION1, servo_id, address);
                if (i < RETRY_COUNT)
                {
                    if (checkErrors(false))
                        return result; // All is good, return

                    Console.WriteLine("read2byte, retry " + i);
                }
                else
                    checkErrors(true); // Last try - throw an exception if it fails
            }
            return result;
        }
        public short read2byteSigned(byte servo_id, ushort address)
        {
            return unchecked((short)read2byte(servo_id, address));
        }
        public void write1byte(byte servo_id, ushort address, byte data)
        {
            for (int i = 1; i <= RETRY_COUNT; i++)
            {
                dynamixel.write1ByteTxRx(port_num, PROTOCOL_VERSION1, servo_id, address, data);
                if (i < RETRY_COUNT)
                {
                    if (checkErrors(false))
                        return; // All is good, return

                    Console.WriteLine("write1byte, retry " + i);
                }
                else
                    checkErrors(true); // Last try - throw an exception if it fails
            }
        }
        public void write2byte(byte servo_id, ushort address, ushort data)
        {
            for (int i = 1; i <= RETRY_COUNT; i++)
            {
                dynamixel.write2ByteTxRx(port_num, PROTOCOL_VERSION1, servo_id, address, data);
                if (i < RETRY_COUNT)
                {
                    if (checkErrors(false))
                        return; // All is good, return
                    Console.WriteLine("write2byte, retry " + i);
                }
                else
                    checkErrors(true); // Last try - throw an exception if it fails
            }
        }
    }
}
