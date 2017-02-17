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
        private Boolean _liftCheck = false;
        private Boolean _F_B_Check = false;
        private Boolean _isRobotConnected = false;


        private MyoSharp.Communication.IChannel _myoChannel;
        private MyoSharp.Communication.IChannel _myoChannel1;
        private IHub _myoHub;
        private IHub _myoHub1;


        private  Pose _currentPose;
        private double _currentRoll;
        private double _currentHeight;

        DispatcherTimer _orientationTimer;


        public MainPage()
        {
            this.InitializeComponent();

            setupTimers();
            _orientationTimer.Start();

           // init();
        }


        #region timers methods
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

            //Start and Stop the Robot Connection
            if (_currentPose == Pose.FingersSpread)
            {

                if (_isRobotConnected==false)
                    //BrickInit();

                if (_startRobot)
                    _startRobot = false;
                else
                    _startRobot = true;        
            }

            //If The Robot Is Connected and Started
            if (_startRobot && _isRobotConnected)
            {
                #region Roll
                //if (_currentRoll >= 0)
                //{   // Move Left
                //    //eMyo.SetValue(Canvas.LeftProperty, (double)eMyo.GetValue(Canvas.LeftProperty) - 10);
                //}
                //else if (_currentRoll <= 0)
                //{   // Move Right
                //    //eMyo.SetValue(Canvas.LeftProperty, (double)eMyo.GetValue(Canvas.LeftProperty) + 10);
                //}
                //else
                //{
                //    //Stop 
                //}
                #endregion

                if (_currentPose == Pose.DoubleTap)
                {
                    if (_liftCheck)
                    {
                        //LIFT UP
                        RobotLift();
                    }
                    else{
                        //DROP
                        RobotDrop();
                    }
                    //LIFT
                }


                if (_currentPose == Pose.Fist)
                {
                    //FOWARD
                    if (_F_B_Check)
                    {
                        RobotFoward();
                    }
                    else
                    {
                        //BACKWARDS
                        RobotBackward();
                    }
                    //LIFT
                }

                if (_currentPose == Pose.WaveIn)
                {
                    //Turn LEFT
                    RobotLeft();
                }


                if (_currentPose == Pose.WaveIn)
                {
                    //Turn RIGHT
                    RobotRight();
                }


                if (_currentHeight >= 0)
                {   // move to the down
                    //eMyo.SetValue(Canvas.TopProperty, (double)eMyo.GetValue(Canvas.TopProperty) - 10);
                }
                else
                {   // movet to the up
                    //eMyo.SetValue(Canvas.TopProperty, (double)eMyo.GetValue(Canvas.TopProperty) + 10);
                }

                //if ((double)eMyo.GetValue(Canvas.LeftProperty) <= 0 || (double)eMyo.GetValue(Canvas.LeftProperty) >= 0)
                //{
                //tbHeight.Text = "Width";
                //}
                //else if ((double)eMyo.GetValue(Canvas.TopProperty) <= 0 || (double)eMyo.GetValue(Canvas.TopProperty) >= 0)
                // {
                //tbHeight.Text = "height";
                //}
                // else
                //{
                //tbHeight.Text = "";
                //}
            }

        }
        #endregion

        #region Myo Setup Methods
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
                tblUpdates.Text = tblUpdates.Text + System.Environment.NewLine +"Myo disconnected";
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

        #region Gryoscope data
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
            var rollDegree = (z * 180.0) /Math.PI;

            tblXGyro.Text = "Gyro X: " + (pitchDegree).ToString("0.00");
            tblYGyro.Text = "Gyro Y: " + (yawDegree).ToString("0.00");
            tblZGyro.Text = "Gyro R: " + (rollDegree).ToString("0.00");
        }
        #endregion

        #region Accelerometer Orientation Data
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

            tblXValue.Text = "Pitch: " + (pitchDegree).ToString("0.00");
            tblYValue.Text = "Yaw: " + (yawDegree).ToString("0.00");
            tblZValue.Text = "Roll: " + (rollDegree).ToString("0.00");

            pitchLine.X2 = pitchLine.X1 + pitchDegree;
            yawLine.Y2 = yawLine.Y1 - yawDegree;
            rollLine.X2 = rollLine.X1 - rollDegree;
            rollLine.Y2 = rollLine.Y1 + rollDegree;
        }
        #endregion

        #region Pose related methods

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


        private async void Myo_PoseChanged(object sender, PoseEventArgs e)
        {
            Pose curr = e.Pose;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                tblUpdates.Text = curr.ToString();

                //Sets the _currentPose to the current pose
                _currentPose = curr;

                switch (curr)
                {
                    case Pose.Rest:
                        //eMyo.Fill = new SolidColorBrush(Colors.Blue);
                        break;
                    case Pose.Fist:
                        //eMyo.Fill = new SolidColorBrush(Colors.Red);
                        break;
                    case Pose.WaveIn:
                        break;
                    case Pose.WaveOut:
                        break;
                    case Pose.FingersSpread:
                        break;
                    case Pose.DoubleTap:
                        break;
                    case Pose.Unknown:
                        break;
                    default:
                        break;
                }
            });
        }
        #endregion




        private void BrickInit()
        {
            _brick = new Brick(new NetworkCommunication("192.168.0.36"));

            _brick.BrickChanged += _brick_BrickChanged;

           connectToBrick();//Connects to the brick
        }

        private async void connectToBrick()
        {
            await _brick.ConnectAsync();

            //Return True when connected
            _isRobotConnected = true;
        }


        #region Robot Commands Movments Etc
        private async void RobotFoward()
        {
            await _brick.DirectCommand.PlayToneAsync(100, 1000, 300);
            await _brick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.A, 60);
            TbCurrentPose.Text += "\n RobotFoward() Pose.Fist";
        }

        private async void RobotBackward()
        {
            await _brick.DirectCommand.PlayToneAsync(100, 1000, 300);
            await _brick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.A | OutputPort.B, -60);
            TbCurrentPose.Text += "\n RobotBackward()  Pose.Fist";
        }

        private async void RobotRight()
        {
            await _brick.DirectCommand.PlayToneAsync(100, 1000, 300);
            await _brick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.B, 60);
            await _brick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.A, 30);
            TbCurrentPose.Text += "\n RobotRight()  Pose.WaveOut";
        }

        private async void RobotLeft()
        {
            await _brick.DirectCommand.PlayToneAsync(100, 1000, 300);
            await _brick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.A, 60);
            await _brick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.B, 30);
            TbCurrentPose.Text += "\n RobotLeft() Pose.WaveIn";
        }

        private async void RobotLift()
        {
            await _brick.DirectCommand.PlayToneAsync(100, 1000, 300);
            await _brick.DirectCommand.TurnMotorAtPowerForTimeAsync(OutputPort.C, 60, 3000, true);
            TbCurrentPose.Text += "\n RobotLift() Pose.DoubleTap";
        }

        private async void RobotDrop()
        {
            await _brick.DirectCommand.PlayToneAsync(100, 1000, 300);
            await _brick.DirectCommand.TurnMotorAtPowerForTimeAsync(OutputPort.C, -60, 3000, true);
            TbCurrentPose.Text += "\n RobotDrop() Pose.DoubleTap";
        }
        #endregion 


        // Set Polarity of the motors (what way they spin)
        private async void MotorPolarity()
        {
            await _brick.DirectCommand.SetMotorPolarity(OutputPort.A | OutputPort.B, Polarity.Forward);
        }

        #region Sample Command Methods
        //Direct Command  
        private async void DirectComands()
        {
            await _brick.DirectCommand.PlayToneAsync(100, 1000, 300);

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
            _brick.BatchCommand.PlayTone(100, 1200, 300);
            await _brick.BatchCommand.SendCommandAsync();
        }

        //Sends Sounds/Images to the Block
        private async void SendData()
        {
            await _brick.SystemCommand.CopyFileAsync("test.rdf", "../prjs/myApp/Test1.rsf");
            await _brick.DirectCommand.PlaySoundAsync(100, "../prjs/myApp/Test1");
        }
        #endregion


        private void _brick_BrickChanged(object sender, BrickChangedEventArgs e)
        {
            //Print out Port (Ones) value to the Console.
            Debug.WriteLine("Port A Results: "+e.Ports[InputPort.A].SIValue);
        }


        }
}
