using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AviaGetPhoneSize
{
    public partial class Form3 : Form
    {
        VideoCapture theVideoCapturer;
        Bitmap mBackGround = null;
        Rectangle ROI = new Rectangle(783, 582, 528, 1068);
        Hsv hsv_low = new Hsv(75, 0, 50);
        Hsv hsv_high = new Hsv(95, 255, 255);
        Mat mCurrentFrame = new Mat();
        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(@"C:\Tools\avia\images\newled\background.jpg"))
            {
                mBackGround = new Bitmap(@"C:\Tools\avia\images\newled\background.jpg");
            }
            // prepare context menu of pictureBoxOrg
            {
                ContextMenu cm = new ContextMenu();
                cm.MenuItems.Add("Save", (s,ea)=> 
                {
                    if (pictureBoxOrg.Image != null)
                    {
                        save_image(pictureBoxOrg.Image);
                    }
                });
                cm.MenuItems.Add("Set As BackGround", (s, ea) => 
                {
                    mBackGround = new Bitmap(pictureBoxOrg.Image);
                });
                pictureBoxOrg.ContextMenu = cm;
            }
            // prepare context menu of pictureBoxProcessed
            {
                ContextMenu cm = new ContextMenu();
                //cm.MenuItems.Add("Preview", (s, ea) =>
                //{
                //    if (pictureBoxOrg.Image != null)
                //    {
                //        preview_image((Bitmap)pictureBoxOrg.Image);
                //    }
                //});
                cm.MenuItems.Add("process", (s, ea) =>
                {
                    if (pictureBoxOrg.Image != null)
                    {
                        Image<Bgr, Byte> img = mCurrentFrame.ToImage<Bgr, Byte>().Copy();
                        process_image(img.Bitmap);
                    }
                });
                pictureBoxProcessed.ContextMenu = cm;
            }
            theVideoCapturer = new VideoCapture(0);
            bool b = theVideoCapturer.SetCaptureProperty(CapProp.FrameHeight, 1944);
            b = theVideoCapturer.SetCaptureProperty(CapProp.FrameWidth, 2592);
            if(theVideoCapturer.IsOpened)
            {
                timer1.Interval = 250;
                timer1.Enabled = true;
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            // grab frame every 250ms
            try
            {
                if (theVideoCapturer != null && theVideoCapturer.IsOpened)
                {
                    //Mat m=new Mat();
                    theVideoCapturer.Read(mCurrentFrame);
                    //pictureBoxOrg.Image = m.ToImage<Bgr, Byte>().Bitmap;
                    display_picture();
                }
                GC.Collect();
            }
            catch (Exception) { }
        }

        private void display_picture()
        {
            try
            {
                if (tabControl1.SelectedTab == tabPage1)
                {
                    if (!mCurrentFrame.IsEmpty)
                    {
                        try
                        {
                            Image<Bgr, Byte> i = mCurrentFrame.ToImage<Bgr, byte>().Copy();
                            pictureBoxOrg.Image = i.Bitmap;
                        }
                        catch (Exception) { }
                    }
                }
                else if (tabControl1.SelectedTab == tabPage2)
                {
                    if (!mCurrentFrame.IsEmpty)
                    {
                        try
                        {
                            Image<Bgr, Byte> i = mCurrentFrame.ToImage<Bgr, byte>().Copy();
                            pictureBoxProcessed.Image = i.Rotate(-90.0, new Bgr(0, 0, 0), false).Copy(ROI).Bitmap;
                        }
                        catch (Exception) { }
                    }
                }
                else { }
            }
            catch (Exception) { }
        }
        #region Tab 2
        private void TabPage2_Enter(object sender, EventArgs e)
        {
            if (pictureBoxOrg.Image != null)
            {
                preview_image((Bitmap)pictureBoxOrg.Image);
            }
        }
        #endregion

        private void save_image(Image img)
        {
            using (SaveFileDialog saveFileDialog1 = new SaveFileDialog())
            {
                saveFileDialog1.Filter = "jpeg files (*.jpg)|*.jpg|All files (*.*)|*.*";
                saveFileDialog1.FilterIndex = 2;
                saveFileDialog1.RestoreDirectory = true;
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    img.Save(saveFileDialog1.FileName);
                }
            }
        }
        private void preview_image(Bitmap img)
        {
            Image<Bgr, Byte> image = new Image<Bgr, byte>(img).Rotate(-90.0, new Bgr(0, 0, 0), false).Copy(ROI);
            pictureBoxProcessed.Image = image.Bitmap;
        }
        private void process_image(Bitmap img)
        {
            if (mBackGround == null)
            {
                MessageBox.Show("please set back ground image first.");
                return;
            }

            toolStripStatusLabel1.Text = "Precess image...";
            Task.Run(() =>
            {
                Image<Bgr, Byte> bg = new Image<Bgr, byte>(mBackGround).Rotate(-90.0, new Bgr(0, 0, 0), false).Copy(ROI);
                Image<Hsv, byte> hsv_bg = bg.Convert<Hsv, byte>();
                Image<Gray, Byte> mask_bg = hsv_bg.InRange(hsv_low, hsv_high);

                Image<Bgr, Byte> img1 = new Image<Bgr, byte>(img).Rotate(-90.0, new Bgr(0, 0, 0), false).Copy(ROI);
                Image<Hsv, byte> hsv1 = img1.Convert<Hsv, byte>();
                Image<Gray, Byte> mask1 = hsv1.InRange(hsv_low, hsv_high);

                Image<Gray, Byte> diff = mask1.AbsDiff(mask_bg);

                diff.ROI = new Rectangle(diff.Width / 2, diff.Height / 2, diff.Width / 2, diff.Height / 2);
                diff._Erode(1);
                Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
                diff._MorphologyEx(MorphOp.Gradient, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));

                Size ret_sz = Size.Empty;
                Rectangle roi = Rectangle.Empty;
                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(diff, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                    int count = contours.Size;
                    for (int i = 0; i < count; i++)
                    {
                        VectorOfPoint contour = contours[i];
                        double a = CvInvoke.ContourArea(contour);
                        Rectangle r = CvInvoke.BoundingRectangle(contour);
                        if (a > 10.0)
                        {
                            //Program.logIt($"area: {a}, {r}");
                            if (roi.IsEmpty) roi = r;
                            else roi = Rectangle.Union(roi, r);
                        }
                    }
                    ret_sz = new Size(roi.Width + diff.Width, roi.Height + diff.Height);
                    Program.logIt($"size: {ret_sz}");
                    this.Invoke(new Action(() =>
                    {
                        toolStripStatusLabel1.Text = $"Device: Size={ret_sz}";
                    }));
                }
            });
        }
    }
}

