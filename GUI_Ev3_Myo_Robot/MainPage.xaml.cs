using Lego.Ev3.Core;
using Lego.Ev3.WinRT;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        Brick _brick;

        public MainPage()
        {
            this.InitializeComponent();
            init();
        }


        private async void init()
        {
            _brick = new Brick(new NetworkCommunication("192.168.0.36"));

            _brick.BrickChanged += _brick_BrickChanged;

           connectToBrick();//Connects to the brick

        }

        private async void connectToBrick()
        {
            await _brick.ConnectAsync();
            DirectComands();
        }

        // Set Polarity of the motors (what way they spin)
        private async void MotorPolarity()
        {
            await _brick.DirectCommand.SetMotorPolarity(OutputPort.A | OutputPort.B, Polarity.Forward);
        }

        //Direct Command
        private async void DirectComands()
        {
            await _brick.DirectCommand.PlayToneAsync(100, 1000, 300);

            await _brick.DirectCommand.TurnMotorAtPowerForTimeAsync(OutputPort.A, 60, 3000, true);

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



        private void _brick_BrickChanged(object sender, BrickChangedEventArgs e)
        {

            //Print out Port (Ones) value to the Console.
            Debug.WriteLine("Port A Results: "+e.Ports[InputPort.A].SIValue);
        }
    }
}
