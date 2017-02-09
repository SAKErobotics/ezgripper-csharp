# ezgripper-csharp
C# library for controlling the EZGripper robotic grippers. Based on ROBOTIS DynamixelSDK.

## Installation
1. Follow the instructions to install DynamixelSDK.
  * Get it from https://github.com/ROBOTIS-GIT/DynamixelSDK/releases
  * Build the C library as described in https://github.com/ROBOTIS-GIT/DynamixelSDK/wiki/3.2.1.1-C-Windows
  * Build and try the C# sample solution DynamixelSDK-3.4.1\c#\protocol1.0\ping\win64\ping.sln (This assumes you have 64bit Windows. If you have 32bit Windows, use the win32 folder.)
    * In DynamixelSDK-3.4.1\c#\protocol1.0\ping\win64\ping\Ping.cs set BAUDRATE=57600 and servo id DXL_ID=1, set DEVICENAME accordingly.
2. Once the sample project works
  * Copy the *.cs files from this repo to DynamixelSDK-3.4.1\c#\protocol1.0\ping\win64\ping, add them to the project in Visual Studio,
  * Remove Ping.cs from the project.
  * In Sample.cs:
    * Set DEVICENAME accordingly.
    * Change servo_ids = { 1, 2, 3 } to match the ids of servos in your gripper.
      For a single-servo gripper it would be servo_ids = { 1 }.
  
