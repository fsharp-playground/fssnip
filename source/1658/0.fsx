/*Tripwire is a special part of motion detection that can be used to monitor and alert on specific changes. More specifically: tripwire means the detection of intrusion.

This code snippet presents how to create a C# software by using prewritten computer vision components (http://www.camera-sdk.com/) allowing you to get notified when your USB webcam triggers an intrusion. For instance, by using this application, you can use your camera to alarm when a people enters into the shop/office, or even to count how many people entered, etc.

After the necessary using lines and objects you need to implement the Main method and the necessary functions for connecting to a USB webcamera. The startBt_Click method is used to start the tripwire functionality. Thereafter you can see how to handle the enter and exit events. 

Nothing could be more simple! :) */

using System;
using System.Drawing;
using System.Windows.Forms;
using Ozeki.Media.MediaHandlers;
using Ozeki.Media.MediaHandlers.Video;
 
namespace Tripwire_WF
{
    public partial class MainForm : Form
    {
        private WebCamera _camera;
        private DrawingImageProvider _provider;
        private MediaConnector _connector;
 
        private Tripwire tripwire;
 
        private Point _p1, _p2;
 
        public MainForm()
        {
            InitializeComponent();
 
            tripwire = new Tripwire();
 
            _provider = new DrawingImageProvider();
            _connector = new MediaConnector();
        }
 
        private void connectBt_Click(object sender, EventArgs e)
        {
            _camera = WebCamera.GetDefaultDevice();
            if (_camera == null) return;
 
            videoViewerWF1.SetImageProvider(_provider);
 
            _connector.Connect(_camera, tripwire);
            _connector.Connect(tripwire, _provider);
 
            _camera.Start();
 
            videoViewerWF1.Start();
        }
 
        private void startBt_Click(object sender, EventArgs e)
        {
            tripwire.Line.LineWidth = 3;
            tripwire.LineColor = Color.Red;
 
            tripwire.SetPoints(new Point(300, 100), new Point(150, 300));
            tripwire.HighlightMotion = HighlightMotion.Highlight;
 
            tripwire.MotionColor = Color.Blue;
            tripwire.TripwireMotionEnteredToLine += TripwireTripwireMotionEnteredToLine;
            tripwire.TripwireMotionLeaveFromLine += TripwireTripwireMotionLeaveFromLine;
 
            tripwire.Start();
        }
 
        private void stopBt_Click(object sender, EventArgs e)
        {
            tripwire.Stop();
        }
 
        void InvokeThread(Action action)
        {
            Invoke(action);
        }
 
        void TripwireTripwireMotionLeaveFromLine(object sender, TripwireMotionCrossedArgs e)
        {
            InvokeThread(() => { crossedText.Text = @"EXIT!!!"; });
        }
 
        void TripwireTripwireMotionEnteredToLine(object sender, TripwireMotionCrossedArgs e)
        {
            InvokeThread(() => { crossedText.Text = @"ENTER!!!"; });
        }
 
        private void videoViewerWF1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            _p1 = e.Location;
            videoViewerWF1.MouseMove += videoViewerWF1_MouseMove;
        }
 
        private void videoViewerWF1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            _p2 = e.Location;
            tripwire.SetPoints(_p1, _p2);
            videoViewerWF1.MouseMove -= videoViewerWF1_MouseMove;
        }
 
        private void videoViewerWF1_MouseMove(object sender, MouseEventArgs e)
        {
            _p2 = e.Location;
            tripwire.SetPoints(_p1, _p2);
        }
    }
}