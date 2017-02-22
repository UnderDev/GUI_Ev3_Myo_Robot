using Lego.Ev3.Core;
using Lego.Ev3.WinRT;
using MyoSharp.Communication;
using MyoSharp.Device;
using MyoSharp.Exceptions;
using MyoSharp.Poses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Channels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GUI_Ev3_Myo_Robot
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    ///  

    public sealed partial class MainPage : Page
    {
        private Brick _brick;

        private Boolean _startRobot = false;
        private Boolean _isRobotConnected = false;
        private float _DirectionValue;

        private MyoSharp.Communication.IChannel _myoChannel;
        private MyoSharp.Communication.IChannel _myoChannel1;
        private IHub _myoHub;
        private IHub _myoHub1;


        private Pose _currentPose;
        private double _currentRoll;
        private double _currentHeight;

        DispatcherTimer _orientationTimer;


        public MainPage()
        {
            this.InitializeComponent();

            //setupTimers();
            //_orientationTimer.Start();
        }

        #region Myo Setup Methods - Used
        //Button Event to Connect to the myo 
        private void btnMyo_Click(object sender, RoutedEventArgs e)
        { // communication, device, exceptions, poses

            // create the channel
            _myoChannel = Channel.Create(ChannelDriver.Create(ChannelBridge.Create(),
                                    MyoErrorHandlerDriver.Create(MyoErrorHandlerBridge.Create())));

            // Create the hub with the channel
            _myoHub = MyoSharp.Device.Hub.Create(_myoChannel);

            // Create the event handlers for connect and disconnect
            _myoHub.MyoConnected += _myoHub_MyoConnected;
            _myoHub.MyoDisconnected += _myoHub_MyoDisconnected;

            // Start listening 
            _myoChannel.StartListening();

            // Create the channel
            _myoChannel1 = Channel.Create(ChannelDriver.Create(ChannelBridge.Create(),
                                    MyoErrorHandlerDriver.Create(MyoErrorHandlerBridge.Create())));

            // create the hub with the channel
            _myoHub1 = MyoSharp.Device.Hub.Create(_myoChannel1);

            // create the event handlers for connect and disconnect
            _myoHub1.MyoConnected += _myoHub_MyoConnected;
            _myoHub1.MyoDisconnected += _myoHub_MyoDisconnected;

            // Start listening 
            _myoChannel1.StartListening();
        }

        private async void _myoHub_MyoDisconnected(object sender, MyoEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                tblUpdates.Text = tblUpdates.Text + System.Environment.NewLine + "Myo disconnected";
            });
            _myoHub.MyoConnected -= _myoHub_MyoConnected;
            _myoHub.MyoDisconnected -= _myoHub_MyoDisconnected;
            _orientationTimer.Stop();
        }


        private async void _myoHub_MyoConnected(object sender, MyoEventArgs e)
        {
            //Alert the Myo that its connected
            e.Myo.Vibrate(VibrationType.Long);

            //Update the text box that the myo is connected
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                tblUpdates.Text = "Myo Connected: " + e.Myo.Handle;
            });

            // add the pose changed event here
            e.Myo.PoseChanged += Myo_PoseChanged;
            e.Myo.OrientationDataAcquired += Myo_OrientationDataAcquired;
            e.Myo.GyroscopeDataAcquired += Myo_GyroscopeDataAcquired;

            // Unlock the Myo so that it doesn't keep locking between our poses
            e.Myo.Unlock(UnlockType.Hold);

            try
            {
                var sequence = PoseSequence.Create(e.Myo, Pose.FingersSpread, Pose.WaveIn);
                sequence.PoseSequenceCompleted += Sequence_PoseSequenceCompleted;

            }
            catch (Exception myoErr)
            {
                string strMsg = myoErr.Message;
            }

        }
        #endregion


        #region timers methods - Not Used
        private void setupTimers()
        {
            if (_orientationTimer == null)
            {
                _orientationTimer = new DispatcherTimer();
                _orientationTimer.Interval = TimeSpan.FromMilliseconds(100);
                _orientationTimer.Tick += _orientationTimer_Tick;
            }
        }

        private void _orientationTimer_Tick(object sender, object e)
        {

            ////Start and Stop the Robot Connection
            //if (_currentPose == Pose.FingersSpread)
            //{
            //    TbCurrentPose.Text += "\n Magic Happens In Scots Fingers";
            //    if (_isRobotConnected == false)
            //        //BrickInit();

            //        if (_startRobot)
            //            _startRobot = false;
            //        else
            //            _startRobot = true;
            //}

            //If The Robot Is Connected and Started
            //if (_startRobot && _isRobotConnected)
            //if (_startRobot)
            //{
            //    #region Roll
            //    //if (_currentRoll >= 0)
            //    //{   // Move Left
            //    //    //eMyo.SetValue(Canvas.LeftProperty, (double)eMyo.GetValue(Canvas.LeftProperty) - 10);
            //    //}
            //    //else if (_currentRoll <= 0)
            //    //{   // Move Right
            //    //    //eMyo.SetValue(Canvas.LeftProperty, (double)eMyo.GetValue(Canvas.LeftProperty) + 10);
            //    //}
            //    //else
            //    //{
            //    //    //Stop 
            //    //}
            //    #endregion

            //    if (_currentPose == Pose.DoubleTap)
            //    {
            //        if (_liftCheck)
            //        {
            //            //LIFT UP
            //            RobotLift();
            //        }
            //        else
            //        {
            //            //DROP
            //            RobotDrop();
            //        }
            //        //LIFT
            //    }


            //    if (_currentPose == Pose.Fist)
            //    {
            //        //FOWARD
            //        if (_F_B_Check)
            //        {
            //            RobotFoward();
            //            _F_B_Check = false;
            //        }
            //        else
            //        {
            //            //BACKWARDS
            //            RobotBackward();
            //            _F_B_Check = true;
            //        }
            //    }

            //    if (_currentPose == Pose.WaveIn)
            //    {
            //        //Turn LEFT
            //        RobotLeft();
            //    }


            //    if (_currentPose == Pose.WaveOut)
            //    {
            //        //Turn RIGHT
            //        RobotRight();
            //    }

            //    #region Pitch
            //    //if (_currentHeight >= 0)
            //    //{   // move to the down
            //    //    //eMyo.SetValue(Canvas.TopProperty, (double)eMyo.GetValue(Canvas.TopProperty) - 10);
            //    //}
            //    //else
            //    //{   // movet to the up
            //    //    //eMyo.SetValue(Canvas.TopProperty, (double)eMyo.GetValue(Canvas.TopProperty) + 10);
            //    //}

            //    //if ((double)eMyo.GetValue(Canvas.LeftProperty) <= 0 || (double)eMyo.GetValue(Canvas.LeftProperty) >= 0)
            //    //{
            //    //tbHeight.Text = "Width";
            //    //}
            //    //else if ((double)eMyo.GetValue(Canvas.TopProperty) <= 0 || (double)eMyo.GetValue(Canvas.TopProperty) >= 0)
            //    // {
            //    //tbHeight.Text = "height";
            //    //}
            //    // else
            //    //{
            //    //tbHeight.Text = "";
            //    //}
            //    #endregion
            //}

        }
        #endregion

        #region Gryoscope data - Not Used
        private async void Myo_GyroscopeDataAcquired(object sender, GyroscopeDataEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                showGryoscopeData(e.Gyroscope.X, e.Gyroscope.Y, e.Gyroscope.Z);
            });

        }

        private void showGryoscopeData(float x, float y, float z)
        {
            var pitchDegree = (x * 180.0) / Math.PI;
            var yawDegree = (y * 180.0) / Math.PI;
            var rollDegree = (z * 180.0) / Math.PI;

           // tblXGyro.Text = "Gyro X: " + (pitchDegree).ToString("0.00");
           // tblYGyro.Text = "Gyro Y: " + (yawDegree).ToString("0.00");
           // tblZGyro.Text = "Gyro R: " + (rollDegree).ToString("0.00");
        }
        #endregion

        #region Accelerometer Orientation Data - Not Used
        private async void Myo_OrientationDataAcquired(object sender, OrientationDataEventArgs e)
        {
            _currentRoll = e.Roll;
            _currentHeight = e.Pitch;

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                showOrientationData(e.Pitch, e.Yaw, e.Roll);
            });
        }

        private void showOrientationData(double pitch, double yaw, double roll)
        {

            var pitchDegree = (pitch * 180.0) / Math.PI;
            var yawDegree = (yaw * 180.0) / Math.PI;
            var rollDegree = (roll * 180.0) / Math.PI;

            /*tblXValue.Text = "Pitch: " + (pitchDegree).ToString("0.00");
            tblYValue.Text = "Yaw: " + (yawDegree).ToString("0.00");
            tblZValue.Text = "Roll: " + (rollDegree).ToString("0.00");

            pitchLine.X2 = pitchLine.X1 + pitchDegree;
            yawLine.Y2 = yawLine.Y1 - yawDegree;
            rollLine.X2 = rollLine.X1 - rollDegree;
            rollLine.Y2 = rollLine.Y1 + rollDegree;*/
        }
        #endregion

        #region Pose related methods - Not Used

        private async void Sequence_PoseSequenceCompleted(object sender, PoseSequenceEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                tblUpdates.Text = "Pose Sequence completed";
            });
        }

        private async void Pose_Triggered(object sender, PoseEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                tblUpdates.Text = "Pose Held: " + e.Pose.ToString();
            });

        }
        #endregion

        //Gets the current Pose From the user and fires off the Command associated
        private async void Myo_PoseChanged(object sender, PoseEventArgs e)
        {
            Pose curr = e.Pose;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                //Displays the Current Pose To the Screen
                tblUpdates.Text = curr.ToString();

                //Sets the _currentPose to the current pose
                _currentPose = curr;

                if(curr == Pose.FingersSpread)
                StartRobotCommands();

                if (_startRobot && _isRobotConnected)
                {
                    switch (curr)
                    {
                        case Pose.Rest:
                            RobotStop();
                            break;
                        case Pose.Fist:
                            RobotFoward();
                            break;
                        case Pose.WaveIn:
                            RobotLeft();
                            break;
                        case Pose.WaveOut:
                            RobotRight();
                            break;
                        case Pose.FingersSpread:
                            RobotBackward();
                            break;
                        case Pose.DoubleTap:
                            break;
                        case Pose.Unknown:
                            break;
                        default:
                            break;
                    }
                }
            });
        }

        private int count = 0;
        private void StartRobotCommands()
        {
            TbCurrentPose.Text += "\n Magic Fingers";
            //if (_isRobotConnected == false)
            //Initialize the Brick Once
            if (count == 0)
            BrickInit();

            count++;
            _startRobot = true;
        }


        private void BrickInit()
        {
            _brick = new Brick(new NetworkCommunication("192.168.0.13"));

            _brick.BrickChanged += _brick_BrickChanged;

            connectToBrick();//Connects to the brick
        }

        private async void connectToBrick()
        {
            await _brick.ConnectAsync();

            await _brick.DirectCommand.PlayToneAsync(10, 2000, 3000);
            //Return True when connected
            _isRobotConnected = true;
        }


        #region Robot Commands Movments Etc Used

        //Move Robot Fowards - Fist
        private async void RobotFoward()
        {
            await _brick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.D | OutputPort.C, 100);

            //Keep track of where the Robot is pointing eg left right center

            //_brick.BatchCommand.TurnMotorAtPower(OutputPort.D | OutputPort.C, 100);
            //_brick.BatchCommand.TurnMotorAtSpeedForTime(OutputPort.B, 10, 1500, true);

            //Send the Batch Commands listed Above
            //await _brick.BatchCommand.SendCommandAsync();

            TbCurrentPose.Text += "\n RobotFoward() Pose.Fist";
        }

        //Stop All The Motors - Rest
        private async void RobotStop()
        {
            await _brick.DirectCommand.StopMotorAsync(OutputPort.All, true);
            TbCurrentPose.Text += "\n RobotFoward() Pose.Fist";
        }

        //Move Robot Backwards - FingerSpread
        private async void RobotBackward()
        {
            await _brick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.D | OutputPort.C, -100);
            TbCurrentPose.Text += "\n RobotBackward()  Pose.Fist";
        }

        //Move Robot Right - Wave Out
        private async void RobotRight()
        {
             _brick.BatchCommand.TurnMotorAtPower(OutputPort.D | OutputPort.C, 100);
             _brick.BatchCommand.TurnMotorAtSpeedForTime(OutputPort.B, 10 , 1500 , true);

            //Send the Batch Commands listed Above
            await _brick.BatchCommand.SendCommandAsync();

            TbCurrentPose.Text += "\n RobotRight()  Pose.WaveOut";
        }

        //Move Robot Right - Wave In
        private async void RobotLeft()
        {
            _brick.BatchCommand.TurnMotorAtPower(OutputPort.D | OutputPort.C, 100);
            _brick.BatchCommand.TurnMotorAtSpeedForTime(OutputPort.B, -10, 1500, true);

            //Send the Batch Commands listed Above
            await _brick.BatchCommand.SendCommandAsync();

            TbCurrentPose.Text += "\n RobotLeft()  Pose.WaveIn";
        }
        #endregion


        //Event Fired when the brick changes
        private void _brick_BrickChanged(object sender, BrickChangedEventArgs e)
        {
            Debug.WriteLine("Port A Results: " + e.Ports[InputPort.Four].PercentValue);

            //Maby get the value from the Port 4(eyes) and stop it from crashing
            _DirectionValue =  e.Ports[InputPort.Four].PercentValue;
        }

        //Button Event to call shutBrickDown();
        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            shutBrickDown();
        }

        //Method Shuts Down The Brick And Stops All Motors
        private async void shutBrickDown()
        {
            await _brick.DirectCommand.StopMotorAsync(OutputPort.All,true);
            await _brick.DirectCommand.PlayToneAsync(10,2000,300);

            _brick.Disconnect();
            _isRobotConnected = false;
        }


        #region Sample Command Methods
        // Set Polarity of the motors (what way they spin)
        private async void MotorPolarity()
        {
            await _brick.DirectCommand.SetMotorPolarity(OutputPort.A | OutputPort.B, Polarity.Forward);
        }

        //Direct Command  
        private async void DirectComands()
        {
            await _brick.DirectCommand.TurnMotorAtPowerForTimeAsync(OutputPort.A, 60, 3000, true);
            await _brick.DirectCommand.TurnMotorAtPowerForTimeAsync(OutputPort.A | OutputPort.B, 60, 3000, true);
            // Motor in Port A At 60% Power Constant
            //await _brick.DirectCommand.TurnMotorAtPowerForTimeAsync(OutputPort.A, 60);

            // Two Motors at 60% Power
            //await _brick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.A | OutputPort.B, 60);
        }

        // Batch Command
        private async void BatchComands()
        {
            _brick.BatchCommand.TurnMotorAtPower(OutputPort.A, 60);
            await _brick.BatchCommand.SendCommandAsync();
        }

        //Sends Sounds/Images to the Block
        private async void SendData()
        {
            await _brick.SystemCommand.CopyFileAsync("test.rdf", "../prjs/myApp/Test1.rsf");
            await _brick.DirectCommand.PlaySoundAsync(100, "../prjs/myApp/Test1");
        }


        #endregion
    }
}
