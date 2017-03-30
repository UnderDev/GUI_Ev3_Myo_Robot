
                                                 _______           ______  
                                                (  ____ \|\     /|/ ___  \ 
                                                | (    \/| )   ( |\/   \  \
                                                | (__    | |   | |   ___) /
                                                |  __)   ( (   ) )  (___ ( 
                                                | (       \ \_/ /       ) \
                                                | (____/\  \   /  /\___/  /
                                                (_______/   \_/   \______/ 

                      ______   _______  _______ _________ _______  _______           _______  _______ 
                     (  __  \ (  ____ \(  ____ \\__   __/(  ____ )(  ___  )|\     /|(  ____ \(  ____ )
                     | (  \  )| (    \/| (    \/   ) (   | (    )|| (   ) |( \   / )| (    \/| (    )|
                     | |   ) || (__    | (_____    | |   | (____)|| |   | | \ (_) / | (__    | (____)|
                     | |   | ||  __)   (_____  )   | |   |     __)| |   | |  \   /  |  __)   |     __)
                     | |   ) || (            ) |   | |   | (\ (   | |   | |   ) (   | (      | (\ (   
                     | (__/  )| (____/\/\____) |   | |   | ) \ \__| (___) |   | |   | (____/\| ) \ \__
                     (______/ (_______/\_______)   )_(   |/   \__/(_______)   \_/   (_______/|/   \__/                                                                                                                                              
 
### Introduction

The idea for this project was to build a [ Lego Mindstorms Ev3](https://www.lego.com/en-us/mindstorms) Robot, that I can Control Via the [MYO Armband](https://www.myo.com/) using different hand gestures.

To add a bit more funstionalty to the Robot, i mounted my phone to the front of the robot which was running Skype, i then send a video request to another Skpe application running on my Laptop. From this, i was able to drive the robot around with a first person view of where i was going without keeping the robot in sight.

The project will be built using UWP. 

### Required Hardware
  * Lego Ev3 Mindstorms
  * Myo Armband
  * Bluetooth Dongle
  
### Features
 * Automatically Connects to Myo Armband Via bluetooth
 * Automatically Connects to Lego Brick via Wifi
 * Webcam Streaming To the App (Optional & not fully implemented)
 
### Running the Project
 To Run the project please follow the steps Below:
 
 * Download the Source Code into Visual Studio 2015.
 * Download [Myo Connect.](https://www.myo.com/start)
 * Run Myo Connect and [Calibrate](https://support.getmyo.com/hc/en-us/articles/203829315-Creating-a-custom-calibration-profile-for-your-Myo-armband) Your Myo Armband.
 * Connect Your Lego MindStorms Brick to Same [Wi-fi Connection](https://uk.mathworks.com/help/supportpkg/legomindstormsev3io/ug/connect-to-an-ev3-brick-over-wifi.html?requestedDomain=www.mathworks.com) as the App.
 * Run the App on Visual Studio 2015 in Debug 86X on the local machine.

### Ev3 Port's To Gesture
These are the following Ports each Gesture Controls and there associated speeds:

Motor Ports: A, B, C, D
* Fist Gesture   - (Port D, Speed 100) and (Port C, Speed 100)
* Finger Spread  - (Port D, Speed -100) and (Port C , Speed -100)
* Wave In        - (Port D, Speed 100) and (Port C , Speed -55)
* Wave Out       - (Port D, Speed -55) and (Port C , Speed 100)
* Pinch          - (Port B, Speed -48)
* Pinch (Again)  - (Port B, Speed 35)
* Rest           - Stops all Motors in all ports

Sensor Ports: 1, 2, 3, 4

Only the IR sensor is used, and is connected to port one to grab the Sensors SIV value
* Port One - Raw Value 
 
### Gestures(Controls) Used:
Controls for the Robot were as Follows:
 * Move Robot Forward - Fist Gesture.
 * Move Robot Backwards – Finger Spread.
 * Move Robot Right – Wave Out.
 * Move Robot Left – Wave In.
 * Move Robot Bucket Up – Pinch.
 * Move Robot Bucket Down – Pinch Twice.
 * Stop Robot Moving - Rest.
  
### External Api's Used
[LegoEv3](https://github.com/BrianPeek/legoev3)  Wrapper to connect Uwp to the Lego Brick.

[MyoSharp](https://github.com/tayfuzun/MyoSharp) Wrapper to connecting UWP to The Myo Armband.
  

  
  
