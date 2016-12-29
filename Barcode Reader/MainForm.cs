using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Touchless.Vision.Camera;

using System.Drawing.Imaging;
using System.IO;
using System.Globalization;
using System.Windows.Media.Imaging;
using System.Windows;

using ZXing;
using System.Timers;

namespace barcodeReader
{
    
    public partial class MainForm : Form
    {
        System.Timers.Timer timerStaticText;
        String textToShow = "";
        Boolean allowChangeText = true;

        private void setTimer()
        {
            // Create a timer with a 600 milli-second interval.
            timerStaticText = new System.Timers.Timer(3500);
            // Hook up the Elapsed event for the timer. 
            timerStaticText.Elapsed += OnTimedEventReset;
            timerStaticText.AutoReset = false;
            timerStaticText.Enabled = true;
        }

        private void OnTimedEventReset(Object source, ElapsedEventArgs e)
        {
            allowChangeText = true;
        }




        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            thrashOldCamera();
            if (timerStaticText != null)
            {
                timerStaticText.Stop();
                timerStaticText.Dispose();
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            thrashOldCamera();
        }

        private CameraFrameSource _frameSource;
        private static Bitmap _latestFrame;
        private Camera CurrentCamera
        {
            get
           {
              foreach (Camera cam in CameraService.AvailableCameras)
                return cam as Camera;
                return null;
           }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // Early return if we've selected the current camera
            if (CurrentCamera != null)
            {
                if (_frameSource != null && _frameSource.Camera == CurrentCamera)
                    return;

                thrashOldCamera();
                startCapturing();
            }
        }

        private void startCapturing()
        {
            try
            {
                Camera c = CurrentCamera;
                setFrameSource(new CameraFrameSource(c));
                _frameSource.Camera.CaptureWidth = 640;
                _frameSource.Camera.CaptureHeight = 480;
                _frameSource.Camera.Fps = 50;
                _frameSource.NewFrame += OnImageCaptured;

                pictureBoxDisplay.Paint += new PaintEventHandler(drawLatestImage);
                _frameSource.StartFrameCapture();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void drawLatestImage(object sender, PaintEventArgs e)
        {
            if (_latestFrame != null)
            {
                // Draw the latest image from the active camera
                e.Graphics.DrawImage(_latestFrame, 0, 0, _latestFrame.Width, _latestFrame.Height);
                
                System.Windows.Media.Imaging.BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                _latestFrame.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                System.Windows.Media.Imaging.WriteableBitmap writeableBitmap = new System.Windows.Media.Imaging.WriteableBitmap(bitmapSource);

                WriteableBitmap reverseBitmap;

                reverseBitmap = writeableBitmap;
                Bitmap bmp = new Bitmap(reverseBitmap.PixelWidth, reverseBitmap.PixelHeight);
                BitmapData data = bmp.LockBits(new Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                reverseBitmap.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
                bmp.UnlockBits(data);

                bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);


                // create a barcode reader instance
                IBarcodeReader reader = new ZXing.BarcodeReader();
                // detect and decode the barcode inside the bitmap
                var result = reader.Decode(bmp);
                // do something with the result
                if (allowChangeText)
                {
                    if (result != null)
                    {
                        this.lblValue.Text = result.BarcodeFormat.ToString() + "      " + result.Text;
                        textToShow = result.BarcodeFormat.ToString() + "      " + result.Text;
                        allowChangeText = false;
                        setTimer();
                    }
                    else
                    {
                        this.lblValue.Text = "CAN'T READ";
                    }
                }
            }
        }

        public void OnImageCaptured(Touchless.Vision.Contracts.IFrameSource frameSource, Touchless.Vision.Contracts.Frame frame, double fps)
        {
            _latestFrame = frame.Image;
            pictureBoxDisplay.Invalidate();
        }

        private void setFrameSource(CameraFrameSource cameraFrameSource)
        {
            if (_frameSource == cameraFrameSource)
                return;

            _frameSource = cameraFrameSource;
        }

        //

        private void thrashOldCamera()
        {
            // Trash the old camera
            if (_frameSource != null)
            {
                _frameSource.NewFrame -= OnImageCaptured;
                _frameSource.Camera.Dispose();
                setFrameSource(null);
                pictureBoxDisplay.Paint -= new PaintEventHandler(drawLatestImage);
            }
        }

        //

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_frameSource == null)
                return;

            Bitmap current = (Bitmap)_latestFrame.Clone();
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "*.png|*.png";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    current.Save(sfd.FileName);
                }
            }

            current.Dispose();
        }

        private void btnCapture_Click(object sender, EventArgs e)
        {
            if (_latestFrame != null)
            {
                System.Windows.Media.Imaging.BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                _latestFrame.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                System.Windows.Media.Imaging.WriteableBitmap writeableBitmap = new System.Windows.Media.Imaging.WriteableBitmap(bitmapSource);


                // create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoder = new PngBitmapEncoder();

                // create frame from the writable bitmap and add to encoder
                encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));

                string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

                string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                string path = Path.Combine(myPhotos, "Screenshot-Color-" + time + ".png");

                // write the new file to disk
                try
                {
                    // FileStream is IDisposable
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }

                    this.lblValue.Text =  "SAVED SCREENSHOT";
                }
                catch (IOException)
                {
                    this.lblValue.Text = "FAILED SCREENSHOT"; ;
                }

            }
        }
    }
}