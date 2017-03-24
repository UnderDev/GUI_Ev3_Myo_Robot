```
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
 ```
 
### Introduction

The idea for this project was to build a [ Lego Mindstorms Ev3](https://www.lego.com/en-us/mindstorms) Robot, that I can Control Via the [MYO Armband](https://www.myo.com/) using different hand gestures.  

The project will be built using UWP. 

Having the App running on a raspberry pi, makes the project more portable, not needing to carry around a laptop everywhere you go just to control/connect the Robot.

### Required Hardware
  * Lego Ev3 Mindstorms
  * Myo Armband
  * Bluetooth Dongle

### Features
 * Automatically Connects to Myo Armband Via bluetooth
 * Automatically Connects to Lego Brick via Wifi
 * Webcam Streaming To the App

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
  

  
  
