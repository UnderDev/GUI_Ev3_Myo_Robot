using System;
using Lego.Ev3.Core;
using Lego.Ev3.WinRT;

using MyoSharp.Communication;
using MyoSharp.Device;
using MyoSharp.Exceptions;
using MyoSharp.Poses;

using System.Diagnostics;
using Windows.ApplicationModel.Core;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using System.Linq;

using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.Xaml.Navigation;


namespace GUI_Ev3_Myo_Robot
{
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


        //Camera Stuff
        // Prevent the screen from sleeping while the camera is running
        private readonly DisplayRequest _displayRequest = new DisplayRequest();

        // For listening to media property changes
        private readonly SystemMediaTransportControls _systemMediaControls = SystemMediaTransportControls.GetForCurrentView();

        // MediaCapture and its state variables
        private MediaCapture _mediaCapture;
        private bool _isInitialized = false;


        public MainPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
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
        }

        private void _myoHub_MyoDisconnected(object sender, MyoEventArgs e)
        {
            UpdateUi("Myo disconnected");

            _MyoHub.MyoConnected -= _myoHub_MyoConnected;
            _MyoHub.MyoDisconnected -= _myoHub_MyoDisconnected;
        }

        private void _myoHub_MyoConnected(object sender, MyoEventArgs e)
        {
            _MyMyo = e.Myo;
            e.Myo.Vibrate(VibrationType.Long);

            // Add the pose changed event here
            e.Myo.PoseChanged += Myo_PoseChanged;

            // Unlock the Myo so that it doesn't keep locking between our poses
            e.Myo.Unlock(UnlockType.Hold);

            UpdateUi("Myo Connected: " + e.Myo.Handle);

            //Find The Bricks Ip Address
            FindHostIP();
        }
        #endregion

        #region Auto Connect Ev3 to Wifi - Needs Fixing as performs this every 10 seconds

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
                UpdateUi("Finding Ev3 IP Address.. Please Wait");
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
        private async void MessageReceived(DatagramSocket socket, DatagramSocketMessageReceivedEventArgs eventArguments)
        {
            //IF the Robot is not Connected, or maby Retry get connection to the robot 
            //Msg recived every 10 sec
            try
            {
                IOutputStream outputStream = await socket.GetOutputStreamAsync(
                    eventArguments.RemoteAddress,
                    eventArguments.RemotePort);

                UpdateUi("Found Ev3! Trying To Conecting!");

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
            Pose curPose = e.Pose;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                BitmapImage bitmapImage = null;

                //Path where all the images are stored
                string imgPath = "ms-appx://GUI_Ev3_Myo_Robot/Assets/";

                //Displays the Current Pose To the Screen
                tblUpdates.Text = curPose.ToString();

                //Sets the _currentPose to the current pose
                _CurrentPose = curPose;

                //Initilize the Robot
                //if (curr == Pose.FingersSpread)
                    //BrickInit();

                if (_IsRobotRunning)
                {
                    switch (curPose)
                    {
                        case Pose.Rest:
                            RobotStop();
                            bitmapImage = new BitmapImage(new Uri(imgPath + "Rest.png"));
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
                            bitmapImage = new BitmapImage(new Uri(imgPath + "Unknown.png"));
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
            //if (_IsRobotRunning == false && _IsRobotConnected == true)
            if (_IsRobotRunning == false && _CountStartUp == 0)
            {
                _brick = new Brick(new NetworkCommunication(RemoteAddress.CanonicalName));

                _brick.BrickChanged += _brick_BrickChanged;

                connectToBrick(RemoteAddress.CanonicalName);//Connects to the brick

                _CountStartUp++;
            }
        }

        //Connect to the Lego Brick
        private async void connectToBrick(String hostIp)
        {
            await _brick.ConnectAsync();
            UpdateUi("Connected To Ev3 At Address: "+ hostIp);
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
                tblUpdates.Text = message;
            }
            );
        }
        #endregion




        #region Constructor, lifecycle and navigation
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await InitializeCameraAsync();
        }
        #endregion Constructor, lifecycle and navigation

        #region MediaCapture methods

        /// <summary>
        /// Initializes the MediaCapture, registers events, gets camera device information for mirroring and rotating, and starts preview
        /// </summary>
        /// <returns></returns>
        private async Task InitializeCameraAsync()
        {
            Debug.WriteLine("InitializeCameraAsync");

            if (_mediaCapture == null)
            {
                UpdateUi("Finding Camera");
                // Attempt to get the back camera if one is available, but use any camera device if not
                var cameraDevice = await FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel.Top);

                if (cameraDevice == null)
                {
                    UpdateUi("No camera device found!");
                    return;
                }

                // Create MediaCapture and its settings
                _mediaCapture = new MediaCapture();

                var settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameraDevice.Id };

                // Initialize MediaCapture
                try
                {
                    await _mediaCapture.InitializeAsync(settings);
                    _isInitialized = true;
                }
                catch (UnauthorizedAccessException)
                {
                    UpdateUi("The app was denied access to the camera");
                }

                // If initialization succeeded, start the preview
                if (_isInitialized)
                {
                    await StartPreviewAsync();
                }
            }
        }

        /// <summary>
        /// Starts the preview and adjusts it for for rotation and mirroring after making a request to keep the screen on and unlocks the UI
        /// </summary>
        /// <returns></returns>
        private async Task StartPreviewAsync()
        {
            UpdateUi("Starting Camera Preview");

            // Prevent the device from sleeping while the preview is running
            _displayRequest.RequestActive();

            // Set the preview source in the UI and mirror it if necessary
            PreviewControl.Source = _mediaCapture;

            // Start the preview
            await _mediaCapture.StartPreviewAsync();
        }

        #endregion MediaCapture methods

        #region Helper functions

        /// <summary>
        /// Queries the available video capture devices to try and find one mounted on the desired panel
        /// </summary>
        /// <param name="desiredPanel">The panel on the device that the desired camera is mounted on</param>
        /// <returns>A DeviceInformation instance with a reference to the camera mounted on the desired panel if available,
        ///          any other camera if not, or null if no camera is available.</returns>
        private static async Task<DeviceInformation> FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel desiredPanel)
        {
            // Get available devices for capturing pictures
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            // Get the desired camera by panel
            DeviceInformation desiredDevice = allVideoDevices.FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desiredPanel);

            // If there is no device mounted on the desired panel, return the first device found
            return desiredDevice ?? allVideoDevices.FirstOrDefault();
        }

        /// <summary>
        /// Saves a SoftwareBitmap to the specified StorageFile
        /// </summary>
        /// <param name="bitmap">SoftwareBitmap to save</param>
        /// <param name="file">Target StorageFile to save to</param>
        /// <returns></returns>
        private static async Task SaveSoftwareBitmapAsync(SoftwareBitmap bitmap, StorageFile file)
        {
            using (var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, outputStream);

                // Grab the data from the SoftwareBitmap
                encoder.SetSoftwareBitmap(bitmap);
                await encoder.FlushAsync();
            }
        }

        #endregion Helper functions 



    }
}