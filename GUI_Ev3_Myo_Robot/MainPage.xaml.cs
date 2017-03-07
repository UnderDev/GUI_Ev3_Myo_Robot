using Lego.Ev3.Core;
using Lego.Ev3.WinRT;
using MyoSharp.Communication;
using MyoSharp.Device;
using MyoSharp.Exceptions;
using MyoSharp.Poses;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.Core;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GUI_Ev3_Myo_Robot
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    ///  

    public sealed partial class MainPage : Page
    {
        //Ev3 Vars
        private Brick _brick;

        //Booleans to check if Connected/Running
        private Boolean _IsRobotRunning = false;

        //Port etc To listen on for Ev3 Broadcast
        private const string _EV3_PORT = "3015";
        private const uint _EV3_INBOUND_BUFFER_SIZE = 67;

        //Used only for starting program
        private int _CountStartUp = 0;

        //Myo Vars
        private IChannel _MyoChannel;
        private IHub _MyoHub;
        private Pose _CurrentPose;
        private IMyo _MyMyo;

        public MainPage()
        {
            this.InitializeComponent();
        }

        #region Myo Setup Methods
        private void btnMyo_Click(object sender, RoutedEventArgs e)
        { // communication, device, exceptions, poses

            // Create the channel
            _MyoChannel = Channel.Create(ChannelDriver.Create(ChannelBridge.Create(),
                                    MyoErrorHandlerDriver.Create(MyoErrorHandlerBridge.Create())));

            // Create the hub with the channel
            _MyoHub = MyoSharp.Device.Hub.Create(_MyoChannel);

            // Create the event handlers for connect and disconnect
            _MyoHub.MyoConnected += _myoHub_MyoConnected;
            _MyoHub.MyoDisconnected += _myoHub_MyoDisconnected;

            // Start listening 
            _MyoChannel.StartListening();

            //// Create the channel
            //_myoChannel1 = Channel.Create(ChannelDriver.Create(ChannelBridge.Create(),
            //                        MyoErrorHandlerDriver.Create(MyoErrorHandlerBridge.Create())));

            //// Create the hub with the channel
            //_myoHub1 = MyoSharp.Device.Hub.Create(_myoChannel1);
            //// Create the event handlers for connect and disconnect
            //_myoHub1.MyoConnected += _myoHub_MyoConnected;
            //_myoHub1.MyoDisconnected += _myoHub_MyoDisconnected;

            //// Start listening 
            //_myoChannel1.StartListening();
        }

        private async void _myoHub_MyoDisconnected(object sender, MyoEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                tblUpdates.Text = tblUpdates.Text + System.Environment.NewLine +
                                    "Myo disconnected";
            });
            _MyoHub.MyoConnected -= _myoHub_MyoConnected;
            _MyoHub.MyoDisconnected -= _myoHub_MyoDisconnected;
        }

        private async void _myoHub_MyoConnected(object sender, MyoEventArgs e)
        {
            _MyMyo = e.Myo;
            e.Myo.Vibrate(VibrationType.Long);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                tblUpdates.Text = "Myo Connected: " + e.Myo.Handle;
            });
            // add the pose changed event here
            e.Myo.PoseChanged += Myo_PoseChanged;

            // unlock the Myo so that it doesn't keep locking between our poses
            e.Myo.Unlock(UnlockType.Hold);

            //TbCurrentPose.Text = "Connecting to Robot Please Wait....";

            FindHostIP();
        }
        #endregion

        #region Auto Connect Ev3 to Wifi - Needs Fixing

        private async void FindHostIP()
        {
            //Open up a socket
            DatagramSocket listener = new DatagramSocket();

            //Add MessageReceived Revived Event
            listener.MessageReceived += MessageReceived;

            //Important for async access
            CoreApplication.Properties.Add("listener", listener);

            // Start listen operation.
            try
            {
                listener.Control.InboundBufferSizeInBytes = _EV3_INBOUND_BUFFER_SIZE;
                // Don't limit traffic to an address or an adapter.
                await listener.BindServiceNameAsync(_EV3_PORT);
            }
            catch (Exception)
            {
                //Oops Something Went Wong
            }
        }

        //Event Fires Off when a message is recived on that Socket
        async void MessageReceived(DatagramSocket socket, DatagramSocketMessageReceivedEventArgs eventArguments)
        {
            //IF the Robot is not Connected, or maby Retry get connection to the robot 
            //Msg recived every 10 sec
            try
            {
                IOutputStream outputStream = await socket.GetOutputStreamAsync(
                    eventArguments.RemoteAddress,
                    eventArguments.RemotePort);

                BrickInit(eventArguments.RemoteAddress);
            }
            catch (Exception)
            {
                //Oops Something Went Wong
            }
        }

        #endregion

        #region Myo Event Methods
        //Gets the current Pose From the user and fires off the Command associated
        private async void Myo_PoseChanged(object sender, PoseEventArgs e)
        {
            Pose curr = e.Pose;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                BitmapImage bitmapImage = null;
                string imgPath = "ms-appx://GUI_Ev3_Myo_Robot/Assets/";

                //Displays the Current Pose To the Screen
                tblUpdates.Text = curr.ToString();

                //Sets the _currentPose to the current pose
                _CurrentPose = curr;

                //Initilize the Robot
                if (curr == Pose.FingersSpread)
                    BrickInit();

                if (_IsRobotRunning)
                {
                    switch (curr)
                    {
                        case Pose.Rest:
                            RobotStop();
                            break;

                        case Pose.Fist:
                            RobotFoward();
                            bitmapImage = new BitmapImage(new Uri(imgPath + "Fist.png"));
                            break;

                        case Pose.WaveIn:
                            RobotLeft();
                            bitmapImage = new BitmapImage(new Uri(imgPath + "WaveIn.png"));
                            break;

                        case Pose.WaveOut:
                            RobotRight();
                            bitmapImage = new BitmapImage(new Uri(imgPath + "WaveOut.png"));
                            break;

                        case Pose.FingersSpread:
                            RobotBackward();
                            bitmapImage = new BitmapImage(new Uri(imgPath + "FingerSpread.png"));
                            break;

                        case Pose.DoubleTap:
                            RobotLift();
                            bitmapImage = new BitmapImage(new Uri(imgPath + "Pinch.png"));
                            break;

                        case Pose.Unknown:
                            break;
                        default:
                            break;
                    }
                    ImgCurPose.Source = bitmapImage;
                }
            });
        }
        #endregion

        #region Brick Setup
        private void BrickInit(HostName RemoteAddress)
        {
            //TbCurrentPose.Text += "\n Magic Fingers";

            //if (_IsRobotRunning == false && _IsRobotConnected == true)
            if (_IsRobotRunning == false && _CountStartUp == 0)
            {
                _brick = new Brick(new NetworkCommunication(RemoteAddress.CanonicalName));

                _brick.BrickChanged += _brick_BrickChanged;

                connectToBrick();//Connects to the brick

                _CountStartUp++;
            }
        }

        //Connect to the Lego Brick
        private async void connectToBrick()
        {
            await _brick.ConnectAsync();

            // await _brick.DirectCommand.PlayToneAsync(5, 2000, 3000);

            //Return True when connected
            _IsRobotRunning = true;

            //Set The Bricks Motor Polarity
            //MotorPolarity();
        }
        #endregion

        #region Robot Movment Methods

        //Move Robot Fowards - Fist
        private async void RobotFoward()
        {
            await _brick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.D | OutputPort.C, 100);
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
            await _brick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.D, 100);
            await _brick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.C, 70);

            TbCurrentPose.Text += "\n RobotRight()  Pose.WaveOut";
        }

        //Move Robot Right - Wave In
        private async void RobotLeft()
        {
            await _brick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.D, 70);
            await _brick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.C, 100);

            TbCurrentPose.Text += "\n RobotLeft()  Pose.WaveIn";
        }

        //Move Robot Lift - Wave In
        private async void RobotLift()
        {
            await _brick.DirectCommand.TurnMotorAtPowerForTimeAsync(OutputPort.B, -30, 1300, true);

            TbCurrentPose.Text += "\n RobotLift()  Pose.Pinch";
        }

        #endregion

        #region Robot Event Methods
        //Event Fired when the brick changes
        private void _brick_BrickChanged(object sender, BrickChangedEventArgs e)
        {
            Debug.WriteLine("Port One Results: " + e.Ports[InputPort.One].RawValue);
            float portOneValue = e.Ports[InputPort.One].SIValue;

            //Update the Ui With the Value
            UpdateUi(portOneValue.ToString());
            CheckForCollision(portOneValue);
        }

        //Check Port One's Sensor SIValue for the distance between it any Object infront of it
        //If there is a collision, alert the user via the Myo Armband 
        private void CheckForCollision(float portOneValue)
        {
            if (portOneValue <= 40)
                _MyMyo.Vibrate(VibrationType.Short);
            else if (portOneValue <= 25)
                _MyMyo.Vibrate(VibrationType.Medium);
            else if (portOneValue <= 10)
                _MyMyo.Vibrate(VibrationType.Long);
        }

        //Button Event to call shutBrickDown();
        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            DisconnectFromBrick();
        }

        //Method Shuts Down The Brick And Stops All Motors
        private async void DisconnectFromBrick()
        {
            await _brick.DirectCommand.StopMotorAsync(OutputPort.All, true);
            //await _brick.DirectCommand.PlayToneAsync(5, 2000, 3000);

            _brick.Disconnect();
            _IsRobotRunning = false;
            _CountStartUp = 0;
        }

        //Set Up motor Polarity
        private async void MotorPolarity()
        {
            await _brick.DirectCommand.SetMotorPolarity(OutputPort.C | OutputPort.D, Polarity.Forward);
        }

        #endregion

        #region Update Ui Asyncronosly
        //Allows the update Of the UI from different Threads Asyncronosly
        private async void UpdateUi(string message)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                tbRawValue.Text = message;
            }
            );
        }
        #endregion




        #region Sample Command Methods - ** Not Part of the program **

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