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
        //Rectangle ROI = new Rectangle(783, 582, 528, 1068);
        //Rectangle ROI = new Rectangle(864, 463, 642, 1150);
        //Rectangle ROI = new Rectangle(492, 213, 580, 1100);
        Rectangle ROI = new Rectangle(108, 148, 750, 1249);
        double rotate_angle = -90.0;
        Hsv hsv_low = new Hsv(40, 0, 50);
        Hsv hsv_high = new Hsv(80, 255, 255);
        Mat mCurrentFrame = new Mat();
        LedController led = null;
        public Form3()
        {
            InitializeComponent();
            led = new LedController("COM7");
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            Task.Run(() => 
            {
                led.open();
                led.set_led_to_detect_size();
            });
            if (System.IO.File.Exists(@"D:\projects\avia\images\background.jpg"))
            {
                mBackGround = new Bitmap(@"D:\projects\avia\images\background.jpg");
            }
            // prepare context menu of pictureBoxOrg
            MenuItem led_menu = new MenuItem("LED Control");
            led_menu.MenuItems.Add("On/Off LED", (s, ea) => { led.turn_onoff(); });
            led_menu.MenuItems.Add("Change LED", (s, ea) => { led.switch_led(); });
            led_menu.MenuItems.Add("LED Light", (s, ea) => { led.level_up(); });
            led_menu.MenuItems.Add("LED dark", (s, ea) => { led.level_down(); });
            {
                ContextMenu cm = new ContextMenu();
                cm.MenuItems.Add("Save", (s,ea)=> 
                {
                    if (pictureBoxOrg.Image != null)
                    {
                        Image<Bgr, Byte> img = mCurrentFrame.ToImage<Bgr, Byte>().Copy();
                        save_image(img.Bitmap);
                    }
                });
                cm.MenuItems.Add("Set As BackGround", (s, ea) => 
                {
                    mBackGround = new Bitmap(pictureBoxOrg.Image);
                });
                cm.MenuItems.Add(led_menu.CloneMenu());
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
                        //Image<Bgr, Byte> img = mCurrentFrame.ToImage<Bgr, Byte>().Copy();
                        process_image();
                    }
                });
                cm.MenuItems.Add("Save", (s, ea) =>
                {
                    if (pictureBoxOrg.Image != null)
                    {
                        //Image<Bgr, Byte> img = mCurrentFrame.ToImage<Bgr, Byte>().Copy();
                        save_image((Bitmap)pictureBoxProcessed.Image);
                    }
                });
                cm.MenuItems.Add(led_menu.CloneMenu());
                pictureBoxProcessed.ContextMenu = cm;
            }
            theVideoCapturer = new VideoCapture(0, VideoCapture.API.DShow);
            bool b = theVideoCapturer.SetCaptureProperty(CapProp.FrameHeight, 1080);
            b = theVideoCapturer.SetCaptureProperty(CapProp.FrameWidth, 1920);
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
                    lock(mCurrentFrame)
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
                            pictureBoxProcessed.Image = i.Rotate(rotate_angle, new Bgr(0, 0, 0), false).Copy(ROI).Bitmap;
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

        private void save_image(Bitmap img)
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
            Image<Bgr, Byte> image = new Image<Bgr, byte>(img).Rotate(rotate_angle, new Bgr(0, 0, 0), false).Copy(ROI);
            pictureBoxProcessed.Image = image.Bitmap;
        }
        private Rectangle get_tray_size(Image<Gray,Byte> src)
        {
            Rectangle ret = Rectangle.Empty;
            Image<Gray, Byte> img = src.Copy(new Rectangle(src.Width / 2, src.Height / 2, src.Width / 2, src.Height / 2));

            img._Erode(1);
            Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
            img._MorphologyEx(MorphOp.Gradient, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));

            Size ret_sz = Size.Empty;
            Rectangle roi = Rectangle.Empty;
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(img, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    VectorOfPoint contour = contours[i];
                    double a = CvInvoke.ContourArea(contour);
                    Rectangle r = CvInvoke.BoundingRectangle(contour);
                    if (a > 250.0)
                    {
                        //Program.logIt($"area: {a}, {r}");
                        if (roi.IsEmpty) roi = r;
                        else roi = Rectangle.Union(roi, r);
                    }
                }
                ret_sz = new Size(roi.Width + img.Width, roi.Height + img.Height);
                //Program.logIt($"size: {ret_sz}");
                ret = new Rectangle(new Point(0, 0), ret_sz);
            }
            return ret;
        }
        private void process_image()
        {
            if (mBackGround == null)
            {
                MessageBox.Show("please set back ground image first.");
                return;
            }

            toolStripStatusLabel1.Text = "Precess image...";
            Task.Run(() =>
            {
                Image<Bgr, Byte> bg = new Image<Bgr, byte>(mBackGround).Rotate(rotate_angle, new Bgr(0, 0, 0), false).Copy(ROI);
                Image<Hsv, byte> hsv_bg = bg.Convert<Hsv, byte>();
                Image<Gray, Byte> mask_bg = hsv_bg.InRange(hsv_low, hsv_high);
                //Rectangle tray_rect = get_tray_size(mask_bg);
                Rectangle tray_rect = testOpenCV.get_size_m3(mask_bg.Copy(new Rectangle(0, 0, mask_bg.Width * 2 / 3, mask_bg.Height * 4 / 5)).Not());

                //Image<Bgr, Byte> img = mCurrentFrame.ToImage<Bgr, Byte>();
                Image<Bgr, Byte> img1;
                lock(mCurrentFrame)
                    img1 = mCurrentFrame.ToImage<Bgr, Byte>().Rotate(rotate_angle, new Bgr(0, 0, 0), false).Copy(ROI);
                Image<Hsv, byte> hsv1 = img1.Convert<Hsv, byte>();
                Image<Gray, Byte> mask1 = hsv1.InRange(hsv_low, hsv_high);

                Image<Gray, Byte> diff = mask1.AbsDiff(mask_bg);

                //diff.ROI = new Rectangle(diff.Width / 2, diff.Height / 2, diff.Width / 2, diff.Height / 2);
                //diff._Erode(1);
                //Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
                //diff._MorphologyEx(MorphOp.Gradient, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));

                Size ret_sz = Size.Empty;
                Rectangle phone_rect = testOpenCV.get_size_m3(diff);
                ret_sz = phone_rect.Size;
                /*
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
                        if (a > 100.0)
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
                */
                if (!ret_sz.IsEmpty)
                {
                    img1.Copy(new Rectangle(new Point(0, 0), ret_sz)).Save("temp_1.jpg");
                    img1.Copy(new Rectangle(tray_rect.Width, 0, ret_sz.Width - tray_rect.Width, ret_sz.Height)).Save("temp_2.jpg");
                    int case_type = testOpenCV.check_deviceimagetype(img1.Copy(new Rectangle(tray_rect.Width, 0, ret_sz.Width - tray_rect.Width, ret_sz.Height)));
                    if (case_type == 1)
                    {
                        led.turn_on_cold_led();
                        System.Threading.Thread.Sleep(1000);
                        double ratio = 1.0;
                        int retry = 5;
                        while (ratio > 0.5 && retry>0)
                        {
                            // get image
                            Image<Bgr, Byte> i0;
                            lock(mCurrentFrame)
                                i0 = mCurrentFrame.ToImage<Bgr, Byte>().Copy();
                            img1 = i0.Rotate(rotate_angle, new Bgr(0, 0, 0), false).Copy(ROI);
                            //
                            ratio = test(img1.Copy(new Rectangle(tray_rect.Width, 0, ret_sz.Width - tray_rect.Width, ret_sz.Height)));
                            //
                            if (ratio > 0.5)
                            {
                                led.level_down();
                                Tuple<bool,int,int> led_value= led.read_value();
                                if (led_value.Item1 && (led_value.Item2 == 45 || led_value.Item3 == 45))
                                    retry--;
                                System.Threading.Thread.Sleep(1000);
                            }
                            else
                            {
                                // check color
                            }
                        }
                        // final
                        lock (mCurrentFrame)
                        {
                            mCurrentFrame.ToImage<Bgr, Byte>().Rotate(rotate_angle, new Bgr(0, 0, 0), false).Copy(ROI).Save("temp_1.jpg");
                        }
                    }
                }
            });
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(led!=null)
            {
                led.close();
            }
        }

        
        double test(Image<Bgr,Byte> src)
        {
            double ret = 0;
            // cut the corner
            SizeF sz = new SizeF(0, 0);
            Rectangle rect = new Rectangle(new Point(0, 0), src.Size);
            sz.Width = -0.3f * rect.Width;
            rect.Inflate(Size.Round(sz));
            rect.Height = 500;
            Image<Bgr, Byte> img = src.Copy(rect);
            img.Save("temp_3.jpg");

            Image<Hsv, float> img_hsv = img.Convert<Hsv, float>();
            Image<Gray, Byte> mask = img_hsv.InRange(new Hsv(0, 0, 235), new Hsv(255, 255, 255));
            ret = 1.0 * CvInvoke.CountNonZero(mask) / (mask.Width * mask.Height);
            return ret;
        }
    }
}

