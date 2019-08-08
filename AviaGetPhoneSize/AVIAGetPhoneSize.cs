using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.ML;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AviaGetPhoneSize
{
    class AVIAGetPhoneSize
    {
        static int Main(string[] args)
        {
            //Rectangle r = get_rectangle_by_canny(@"C:\Tools\avia\images\Final270\iphone6 Gold\0123.6.bmp");
            //Console.WriteLine($"Canny: {r} and {toFloat(r)}");
            //Rectangle r1 = get_rectangle_by_sobel(@"C:\Tools\avia\images\Final270\iphone6 Gold\0123.6.bmp");
            //Console.WriteLine($"Sobel: {r1} and {toFloat(r1)}");
            //test();
            //montion_detect();
            start(null);
            return 0;

        }
        public static void start(System.Threading.EventWaitHandle quitEvent)
        {
            //montion_detect(quitEvent);
            //montion_detect_v2(quitEvent);
            //montion_detect_v3(quitEvent);
            montion_detect_v4(quitEvent);
        }
        static void test()
        {
            string fn = @"temp_2.jpg";
            Mat m = CvInvoke.Imread(fn);
            Size sz = detect_size(m.ToImage<Gray, Byte>());
            Image<Bgr, Byte> img1 = new Image<Bgr, byte>("temp_1.jpg");
            Bgr rgb = sample_color(img1);
        }
        static RectangleF toFloat(Rectangle r, float ratio= 0.0139339f)
        {
            RectangleF ret = new RectangleF(ratio * r.X, ratio * r.Y, ratio * r.Width, ratio * r.Height);
            return ret;
        }
        static Rectangle get_rectangle_by_canny(string filename)
        {
            Rectangle ret = Rectangle.Empty;
            Mat m = CvInvoke.Imread(filename, Emgu.CV.CvEnum.ImreadModes.Grayscale);
            Image<Gray, Byte> img = m.ToImage<Gray, Byte>().GetSubRect(new Rectangle(new Point(5, 5), new Size(m.Width - 10, m.Height - 10)));
            CvInvoke.GaussianBlur(img, img, new Size(5, 5), 0);
            double otsu = CvInvoke.Threshold(img, new Mat(), 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
            double sigma = 0.25;
            double lower = Math.Max(1, (1.0 - sigma) * otsu);
            double upper = Math.Min(255, (1.0 + sigma) * otsu);
            CvInvoke.Canny(img, img, lower, upper);
            Rectangle roi = new Rectangle();
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(img, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    //double a1 = CvInvoke.ContourArea(contours[i], false);
                    //if (a1 > 1)
                    {
                        //Program.logIt($"area: {a1}");
                        Rectangle r = CvInvoke.BoundingRectangle(contours[i]);
                        if (roi.IsEmpty) roi = r;
                        else roi = Rectangle.Union(roi, r);
                    }
                }
            }
            ret = roi;
            return ret;
        }

        static Rectangle get_rectangle_by_sobel(string filename)
        {
            Rectangle ret = Rectangle.Empty;
            //string filename = @"C:\Tools\avia\images\Final270\iphone6 Gold\0123.6.bmp";
            Mat m = CvInvoke.Imread(filename, Emgu.CV.CvEnum.ImreadModes.Grayscale);
            Image<Gray, Byte> img = m.ToImage<Gray, Byte>().GetSubRect(new Rectangle(new Point(5, 5), new Size(m.Width - 10, m.Height - 10)));
            Mat b1 = new Mat();
            CvInvoke.GaussianBlur(img, b1, new Size(3, 3), 0, 0, BorderType.Default);
            Mat dx = new Mat();
            Mat dy = new Mat();
            CvInvoke.Sobel(b1, dx, DepthType.Cv16S, 1, 0);
            CvInvoke.ConvertScaleAbs(dx, dx, 1, 0);
            CvInvoke.Sobel(b1, dy, DepthType.Cv16S, 0, 1);
            CvInvoke.ConvertScaleAbs(dy, dy, 1, 0);
            dx.Save("temp_x.bmp");
            dy.Save("temp_y.bmp");
            Mat[] ms = new Mat[] { dx, dy };
            Rectangle[] rs = new Rectangle[] { Rectangle.Empty, Rectangle.Empty };
            for(int idx=0; idx< ms.Length; idx++)
            {
                double otsu = CvInvoke.Threshold(ms[idx], ms[idx], 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
                Rectangle roi = new Rectangle();
                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(ms[idx], contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
                    int count = contours.Size;
                    for (int i = 0; i < count; i++)
                    {
                        //double a1 = CvInvoke.ContourArea(contours[i], false);
                        //if (a1 > 1)
                        {
                            //Program.logIt($"area: {a1}");
                            Rectangle rt = CvInvoke.BoundingRectangle(contours[i]);
                            if (roi.IsEmpty) roi = rt;
                            else roi = Rectangle.Union(roi, rt);
                        }
                    }
                }
                rs[idx] = roi;
                //Program.logIt($"RectX: {roi}, size={toFloat(roi)}");
            }
            ret = new Rectangle(rs[0].X, rs[1].Y, rs[0].Width, rs[1].Height);
            //Program.logIt($"Rect: {ret}, size={toFloat(ret)}");
            return ret;
        }

        static Image<Bgr,Byte> get_frame(TcpClient c, NetworkStream s)
        {
            Image<Bgr, Byte> ret = null;
            byte[] cmd = System.Text.Encoding.UTF8.GetBytes("QueryFrame\n");
            byte[] data = new byte[1024];
            Regex r = new Regex(@"^ACK frame (.+)\s*$", RegexOptions.IgnoreCase);
            string frames = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "AVIA", "frames");
            if (c.Connected)
            {
                try
                {
                    s.Write(cmd, 0, cmd.Length);
                    int read = s.Read(data, 0, data.Length);
                    string str = System.Text.Encoding.UTF8.GetString(data, 0, read);
                    Match m = r.Match(str);
                    if (m.Success)
                    {
                        Mat m0 = CvInvoke.Imread(System.IO.Path.Combine(frames, m.Groups[1].Value));
                        CvInvoke.Rotate(m0, m0, RotateFlags.Rotate90CounterClockwise);
                        ret = m0.ToImage<Bgr, byte>();
                    }
                }
                catch (Exception) { }
            }
            return ret;
        }
        static void montion_detect_v4(System.Threading.EventWaitHandle quitEvent = null)
        {
            TcpClient client = new TcpClient();
            Dictionary<string, object> _cfg = Program.loadConfig(System.Environment.MachineName);
            try
            {
                string root = _cfg["root"]?.ToString();
                //Rectangle roi = new Rectangle(744, 266, 576, 1116);
                //Rectangle roi = new Rectangle(744, 266, 540, 1116);
                Rectangle roi = Program.config_load_rectangle(_cfg, "rectangle1");
                //System.Threading.Thread.Sleep(5000);
                //string frames = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "AVIA", "frames");
                //Regex r = new Regex(@"^ACK frame (.+)\s*$", RegexOptions.IgnoreCase);
                client.Connect(IPAddress.Loopback, 6280);
                NetworkStream ns = client.GetStream();
                ns.ReadTimeout = 3000;
                //byte[] cmd = System.Text.Encoding.UTF8.GetBytes("QueryFrame\n");
                //byte[] data = new byte[1024];
                //BackgroundSubtractorMOG2 bgs = new BackgroundSubtractorMOG2();
                Image<Bgr, Byte> bg_img = new Emgu.CV.Image<Bgr, Byte>(System.IO.Path.Combine(root, "Images", "BackGround.jpg")).Rotate(-90, new Bgr(0, 0, 0), false);
                bg_img.ROI = roi;
                int frame_num = 0;
                Image<Bgr, Byte>[] frames = new Image<Bgr, Byte>[4];
                while (true)
                {
                    System.Threading.Thread.Sleep(500);
                    // fetch a frame
                    Image<Bgr, Byte> cf = get_frame(client, ns);
                    if (cf != null)
                    {
                        CvInvoke.Rotate(cf, cf, RotateFlags.Rotate90CounterClockwise);
                        frames[frame_num % 4] = cf.Copy(roi);
                        frame_num++;
                        if (frame_num > 7)
                            frame_num = 4;
                    }
                    // check 4 frame are same
                    if (frame_num > 3)
                    {
                        if (all_same_frames(frames))
                        {
                            Image<Bgr, Byte> frame_roi = frames[0];
                            // still
                            Tuple<bool, bool, double> device_inplace = check_device_inplace_v2(frame_roi);
                            if (device_inplace.Item1)
                            {
                                if (device_inplace.Item2)
                                {
                                    // device inplace
                                    Program.logIt($"Device inplace, score={device_inplace.Item3}");

#if true
                                    int sizeid = check_size(device_inplace.Item3, _cfg);
                                    if (sizeid > 0)
                                    {
                                        Bgr rgb = sample_color(frame_roi);
                                        Tuple<bool, int, int> res = predict_color_and_size(rgb, new Size(515, 1032), _cfg);
                                        if (res.Item1)
                                        {
                                            Console.WriteLine($"device=ready");
                                            Console.WriteLine($"colorid={res.Item2}");
                                            Console.WriteLine($"sizeid={sizeid}");
                                        }
                                    }
                                    else
                                    {
                                        // error
                                        Program.logIt("Fail to get size");
                                    }
#else
                                    Image<Bgr, Byte> diff = frame_roi.AbsDiff(bg_img);
                                    Size sz = detect_size(diff.Convert<Gray, Byte>());
                                    if (sz.IsEmpty)
                                    {
                                        // error
                                    }
                                    else
                                    {
                                        Bgr rgb = sample_color(frame_roi);
                                        Program.logIt($"device: size={sz}, color={rgb}");
                                        Tuple<bool, int, int> res = predict_color_and_size(rgb, sz, _cfg);
                                        if (res.Item1)
                                        {
                                            Console.WriteLine($"device=ready");
                                            Console.WriteLine($"colorid={res.Item2}");
                                            Console.WriteLine($"sizeid={res.Item3}");
                                        }
                                    }
#endif
                                }
                                else
                                {
                                    // device not implace
                                    Program.logIt("Device not inplace");
                                    Console.WriteLine($"device=removed");
                                }
                            }
                            else
                            {
                                // error during check device in place.
                            }
                        }
                        else
                        {
                            // montion
                        }
                    }

                    // check terminating.
                    GC.Collect();
                    if (System.Console.KeyAvailable)
                    {
                        ConsoleKeyInfo ki = Console.ReadKey();
                        if (ki.Key == ConsoleKey.Escape)
                        {
                            Program.logIt("Monitor will terminated by ESC pressed.");
                            break;
                        }
                    }
                    if (quitEvent != null)
                    {
                        if (quitEvent.WaitOne(0))
                        {
                            Program.logIt("Monitor will terminated by event set.");
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.logIt(ex.Message);
                Program.logIt(ex.StackTrace);
            }
        }
        static void montion_detect_v3(System.Threading.EventWaitHandle quitEvent = null)
        {
            TcpClient client = new TcpClient();
            Dictionary<string, object> _cfg = Program.loadConfig(System.Environment.MachineName);
            try
            {
                string root = _cfg["root"]?.ToString();
                //Rectangle roi = new Rectangle(744, 266, 576, 1116);
                //Rectangle roi = new Rectangle(744, 266, 540, 1116);
                Rectangle roi = Program.config_load_rectangle(_cfg, "rectangle1");
                //System.Threading.Thread.Sleep(5000);
                string frames = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "AVIA", "frames");
                Regex r = new Regex(@"^ACK frame (.+)\s*$", RegexOptions.IgnoreCase);
                client.Connect(IPAddress.Loopback, 6280);
                NetworkStream ns = client.GetStream();
                byte[] cmd = System.Text.Encoding.UTF8.GetBytes("QueryFrame\n");
                byte[] data = new byte[1024];
                //BackgroundSubtractorMOG2 bgs = new BackgroundSubtractorMOG2();
                bool monition = false;
                Image<Bgr, Byte> bg_img = new Emgu.CV.Image<Bgr, Byte>(System.IO.Path.Combine(root, "Images", "BackGround.jpg")).Rotate(-90, new Bgr(0,0,0), false);
                bg_img.ROI = roi;
                while (true)
                {
                    System.Threading.Thread.Sleep(500);
                    ns.Write(cmd, 0, cmd.Length);
                    int read = ns.Read(data, 0, data.Length);
                    string str = System.Text.Encoding.UTF8.GetString(data, 0, read);
                    Match m = r.Match(str);
                    if (m.Success)
                    {
                        Mat cm = CvInvoke.Imread(System.IO.Path.Combine(frames, m.Groups[1].Value));
                        CvInvoke.Rotate(cm, cm, RotateFlags.Rotate90CounterClockwise);

                        if(bg_img==null)
                        {
                            bg_img = cm.ToImage<Bgr,Byte>();
                        }
                        else
                        {
                            Image<Bgr, Byte> frame = cm.ToImage<Bgr, Byte>();
                            Image<Bgr, Byte> frame_roi = frame.Copy(roi);
                            //frame.ROI = roi;
                            Image<Bgr, Byte> diff = frame_roi.AbsDiff(bg_img);
                            //Mat diff = new Mat();
                            //CvInvoke.AbsDiff(cm, bg_img, diff);

                            //Image<Gray, Byte> g = diff.ToImage<Gray, Byte>();
                            Gray ga = diff.Convert<Gray, byte>().GetAverage();
                            if (ga.MCvScalar.V0 < 11)
                            {
                                // same as bg image, fetch another frame again.
                            }
                            else
                            {
                                // no same as bg, check device is place.
                                Tuple<bool, bool> device_inplace = check_device_inplace(frame_roi);
                                if (device_inplace.Item1)
                                {
                                    if (device_inplace.Item2)
                                    {
                                        // device inplace
                                        Program.logIt("Device Arrival");
                                        Size sz = detect_size(diff.Convert<Gray, Byte>());
                                        if (sz.IsEmpty)
                                        {
                                            // error
                                        }
                                        else
                                        {
                                            Bgr rgb = sample_color(frame_roi);
                                            Program.logIt($"device: size={sz}, color={rgb}");
                                            Tuple<bool, int, int> res = predict_color_and_size(rgb, sz);
                                            if (res.Item1)
                                            {
                                                Console.WriteLine($"device=ready");
                                                Console.WriteLine($"colorid={res.Item2}");
                                                Console.WriteLine($"sizeid={res.Item3}");
                                            }
                                        }

                                    }
                                    else
                                    {
                                        // device not inplace
                                    }
                                }
                                else
                                {
                                    // error ocuurs during device inplace check.
                                }
                            }

                        }
                        /*
                        if (monition)
                        {
                            Program.logIt("motion stopped!");
                            Console.WriteLine("Montion stopped.");
                            monition = false;

                            
                           // CvInvoke.Rotate(bg_img, bg_img, RotateFlags.Rotate90CounterClockwise);
                            //Image<Bgr, byte> t_BgImg = bg_img.Copy();
                           // CvInvoke.Rotate(cm, cm, RotateFlags.Rotate90CounterClockwise);
                   
                            if (bg_img == null)
                            {
                                bg_img = cm.ToImage<Bgr, Byte>();
                            }
                            if (!handle_motion(cm.ToImage<Bgr, Byte>(), bg_img))
                            {
                                bg_img = cm.ToImage<Bgr, Byte>();
                            }
                        }
                        monition = true;
                        */
                        /*
                        Mat mask = new Mat();
                        bgs.Apply(cm, mask);
                        Image<Gray, Byte> g = mask.ToImage<Gray, Byte>();
                        Gray ga = g.GetAverage();
                        if (ga.MCvScalar.V0 > 11)
                        {
                            // montion 
                            if (!monition)
                            {
                                Program.logIt("motion detected!");
                                Console.WriteLine("Detected montion.");
                                monition = true;
                                System.Threading.Thread.Sleep(500);
                            }

                        }
                        else
                        {
                            // no montion
                            if (monition)
                            {
                                Program.logIt("motion stopped!");
                                Console.WriteLine("Montion stopped.");
                                monition = false;
                                CvInvoke.Rotate(cm, cm, RotateFlags.Rotate90CounterClockwise);
                                if (bg_img == null)
                                {
                                    bg_img = cm.ToImage<Bgr, Byte>();
                                }
                                if (!handle_motion(cm.ToImage<Bgr, Byte>(), bg_img))
                                {
                                    bg_img = cm.ToImage<Bgr, Byte>();
                                }
                            }
                        }
                        */
                        GC.Collect();
                        if (System.Console.KeyAvailable)
                        {
                            ConsoleKeyInfo ki = Console.ReadKey();
                            if (ki.Key == ConsoleKey.Escape)
                            {
                                Program.logIt("Monitor will terminated by ESC pressed.");
                                break;
                            }
                        }
                        if (quitEvent != null)
                        {
                            if (quitEvent.WaitOne(0))
                            {
                                Program.logIt("Monitor will terminated by event set.");
                                break;
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Program.logIt(ex.Message);
                Program.logIt(ex.StackTrace);
            }
        }
        static void montion_detect_v2(System.Threading.EventWaitHandle quitEvent = null)
        {
            TcpClient client = new TcpClient();
            try
            {
                string root = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "AVIA", "frames");
                Regex r = new Regex(@"^ACK frame (.+)\s*$", RegexOptions.IgnoreCase);
                client.Connect(IPAddress.Loopback, 6280);
                NetworkStream ns = client.GetStream();
                byte[] cmd = System.Text.Encoding.UTF8.GetBytes("QueryFrame\n");
                byte[] data = new byte[1024];
                BackgroundSubtractorMOG2 bgs = new BackgroundSubtractorMOG2();
                bool monition = false;
                Image<Bgr, Byte> bg_img = null;
                while (true)
                {
                    System.Threading.Thread.Sleep(500);
                    ns.Write(cmd, 0, cmd.Length);
                    int read = ns.Read(data, 0, data.Length);
                    string str = System.Text.Encoding.UTF8.GetString(data, 0, read);
                    Match m = r.Match(str);
                    if (m.Success)
                    {
                        Mat cm = CvInvoke.Imread(System.IO.Path.Combine(root, m.Groups[1].Value));
                        Mat mask = new Mat();
                        bgs.Apply(cm, mask);
                        Image<Gray, Byte> g = mask.ToImage<Gray, Byte>();
                        Gray ga = g.GetAverage();
                        if (ga.MCvScalar.V0 > 11)
                        {
                            // montion 
                            if (!monition)
                            {
                                Program.logIt("motion detected!");
                                Console.WriteLine("Detected montion.");
                                monition = true;
                                System.Threading.Thread.Sleep(500);
                            }

                        }
                        else
                        {
                            // no montion
                            if (monition)
                            {
                                Program.logIt("motion stopped!");
                                Console.WriteLine("Montion stopped.");
                                monition = false;
                                CvInvoke.Rotate(cm, cm, RotateFlags.Rotate90CounterClockwise);
                                if (bg_img == null)
                                {
                                    bg_img = cm.ToImage<Bgr, Byte>();
                                }
                                if (!handle_motion(cm.ToImage<Bgr, Byte>(), bg_img))
                                {
                                    bg_img = cm.ToImage<Bgr, Byte>();
                                }
                            }
                        }

                        GC.Collect();
                        if (System.Console.KeyAvailable)
                        {
                            ConsoleKeyInfo ki = Console.ReadKey();
                            if (ki.Key == ConsoleKey.Escape)
                            {
                                Program.logIt("Monitor will terminated by ESC pressed.");
                                break;
                            }
                        }
                        if (quitEvent != null)
                        {
                            if (quitEvent.WaitOne(0))
                            {
                                Program.logIt("Monitor will terminated by event set.");
                                break;
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Program.logIt(ex.Message);
                Program.logIt(ex.StackTrace);
            }
        }
        static void montion_detect(System.Threading.EventWaitHandle quitEvent=null)
        {
            VideoCapture vc = new VideoCapture(0);
            if (vc.IsOpened)
            {
                bool b = vc.SetCaptureProperty(CapProp.Mode, 0);
                b = vc.SetCaptureProperty(CapProp.FrameHeight, 1944);
                b = vc.SetCaptureProperty(CapProp.FrameWidth, 2592);
                BackgroundSubtractorMOG2 bgs = new BackgroundSubtractorMOG2();
                bool monition = false;
                Image<Bgr, Byte> bg_img = null;
                while (true)
                {
                    Mat cm = new Mat();
                    vc.Read(cm);
                    Mat mask = new Mat();
                    bgs.Apply(cm, mask);
                    Image<Gray, Byte> g = mask.ToImage<Gray, Byte>();
                    Gray ga = g.GetAverage();
                    if(ga.MCvScalar.V0 > 11)
                    {
                        // montion 
                        if (!monition)
                        {
                            Program.logIt("motion detected!");
                            Console.WriteLine("Detected montion.");
                            monition = true;
                            System.Threading.Thread.Sleep(500);
                        }

                    }
                    else
                    {
                        // no montion
                        if (monition)
                        {
                            Program.logIt("motion stopped!");
                            Console.WriteLine("Montion stopped.");
                            monition = false;
                            vc.Read(cm);
                            CvInvoke.Rotate(cm, cm, RotateFlags.Rotate90CounterClockwise);
                            if (bg_img == null)
                            {
                                bg_img = cm.ToImage<Bgr, Byte>();
                            }
                            if(!handle_motion(cm.ToImage<Bgr, Byte>(), bg_img))
                            {
                                //bg_img = cm.ToImage<Bgr, Byte>();
                            }
                        }
                    }

                    GC.Collect();
                    if (System.Console.KeyAvailable)
                    {
                        ConsoleKeyInfo ki = Console.ReadKey();
                        if (ki.Key == ConsoleKey.Escape)
                        {
                            Program.logIt("Monitor will terminated by ESC pressed.");
                            break;
                        }                            
                    }
                    if (quitEvent != null)
                    {
                        if(quitEvent.WaitOne(0))
                        {
                            Program.logIt("Monitor will terminated by event set.");
                            break;
                        }
                    }
                }
            }
        }
        static void montion_detect_1()
        {
            VideoCapture vc = new VideoCapture(0);
            if (vc.IsOpened)
            {
                double db = vc.GetCaptureProperty(CapProp.Mode);
                //bool b = vc.SetCaptureProperty(CapProp.Mode, 1);
                bool b = vc.SetCaptureProperty(CapProp.Mode, 0);
                b = vc.SetCaptureProperty(CapProp.FrameHeight, 1944);
                b = vc.SetCaptureProperty(CapProp.FrameWidth, 2592);
                if (vc.Grab())
                {
                    Mat m = new Mat();
                    if (vc.Retrieve(m))
                    {
                        m.Save("temp_1.jpg");
                    }
                }
                //VideoWriter v1 = new VideoWriter("test_1.mp4", (int)vc.GetCaptureProperty(CapProp.Fps), new Size((int)vc.GetCaptureProperty(CapProp.FrameWidth), (int)vc.GetCaptureProperty(CapProp.FrameHeight)), true);
                //VideoWriter v2 = new VideoWriter("test_2.mp4", (int)vc.GetCaptureProperty(CapProp.Fps), new Size((int)vc.GetCaptureProperty(CapProp.FrameWidth), (int)vc.GetCaptureProperty(CapProp.FrameHeight)), true);
                BackgroundSubtractorMOG2 bgs = new BackgroundSubtractorMOG2();
                bool monition = false;
                Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
                Image<Gray, Byte> bg_img = null;
                int index = 1;
                Console.WriteLine("Camera is ready. Press Esc to exit.");
                bool device_in_place = false;
                while (true)
                {
                    Mat cm = new Mat();
                    vc.Read(cm);
                    Mat mask = new Mat();
                    bgs.Apply(cm, mask);
                    //v1.Write(cm);
                    //v2.Write(mask);
                    //img = img.MorphologyEx(MorphOp.Erode, k, new Point(-1, -1), 3, BorderType.Default, new MCvScalar(0));
                    //CvInvoke.MorphologyEx(mask, mask, MorphOp.Erode, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
                    MCvScalar mean = new MCvScalar();
                    MCvScalar stdDev = new MCvScalar();
                    CvInvoke.MeanStdDev(mask, ref mean, ref stdDev);
                    if (mean.V0 > 17)
                    {
                        if (!monition)
                        {
                            Program.logIt("motion detected!");
                            Console.WriteLine("Detected montion.");
                            monition = true;
                        }
                    }
                    else
                    {
                        if (monition)
                        {
                            Program.logIt("motion stopped!");
                            Console.WriteLine("Montion stopped.");
                            monition = false;
#if true

                            if(bg_img == null)
                            {
                                bg_img = cm.ToImage<Gray, Byte>().Rotate(-90, new Gray(0), false);
                                //bg_img.Save("temp_bg.jpg");
                            }
                            else
                            {
                                device_in_place = handle_motion_V2(cm.ToImage<Bgr, Byte>().Rotate(-90, new Bgr(0, 0, 0), false), bg_img, index++);
                                if(!device_in_place)
                                    bg_img = cm.ToImage<Gray, Byte>().Rotate(-90, new Gray(0), false);
                                //Rectangle r = new Rectangle(196, 665, 269, 628);
                                //// check needed.
                                //{
                                //    Image<Gray, Byte> img = cm.ToImage<Gray, Byte>().Rotate(-90, new Gray(0), false);
                                //    img.Save($"temp_{index}.jpg");
                                //    img = img.AbsDiff(bg_img);
                                //    if (img.GetAverage().MCvScalar.V0 > 10)
                                //    {

                                //    }
                                //    img.Save($"temp_{index}_diff.jpg");
                                //}
                                //{
                                //    Image<Bgr, Byte> img = cm.ToImage<Bgr, Byte>().Rotate(-90, new Bgr(0, 0, 0), false);
                                //    Image<Bgr, Byte> img1 = img.Copy(r);
                                //    img1.Save($"temp_{index}_1.jpg");
                                //}
                                //index++;
                            }
#else
                            if (!device_in_place)
                            {
                                bg_img = cm.ToImage<Gray, Byte>().Rotate(-90, new Gray(0), false);
                            }
                            device_in_place = handle_motion(cm.ToImage<Bgr, Byte>().Rotate(-90, new Bgr(0, 0, 0), false), bg_img, index++);
#endif
                        }
                    }
                    GC.Collect();
                    if (System.Console.KeyAvailable)
                    {
                        ConsoleKeyInfo ki = Console.ReadKey();
                        if(ki.Key==ConsoleKey.Escape)
                            break;
                    }
                }
            }
        }
        public static Tuple<bool, bool, double> check_device_inplace_v2(Image<Bgr, Byte> diff, double threshold = 0.27)  // 0.3
        {
            bool ret = false;
            bool device_inplace = false;
            Program.logIt("check_device_inplace: ++");
            int[] all = diff.CountNonzero();
            double r = 0;
            if (all[0] > 0 && all[1] > 0 && all[2] > 0)
            {
                Image<Hsv, Byte> hsvimg = diff.Convert<Hsv, Byte>();
                Image<Gray, Byte> mask = hsvimg.InRange(new Hsv(45, 100, 50), new Hsv(75, 255, 255));
                //diff.Save("temp_2.jpg");
                //Image<Gray, Byte> mask = diff.InRange(new Bgr(30, 60, 30), new Bgr(95, 130, 70)); //img.InRange(new Bgr(30, 60, 30), new Bgr(95, 130, 70));
                int[] area = mask.CountNonzero();
                r = (double)area[0] / (mask.Width * mask.Height);
                if (r < threshold)
                {
                    device_inplace = true;
                }
                ret = true;
            }
            //mask.Save("temp_3.jpg");
            Program.logIt($"check_device_inplace: -- {ret}, inplace={device_inplace} score={r}");
            return new Tuple<bool, bool, double>(ret, device_inplace, r);
        }
        public static Tuple<bool,bool> check_device_inplace(Image<Bgr, Byte> diff, double threshold =0.27)  // 0.3
        {
            bool ret = false;
            bool device_inplace = false;
            Program.logIt("check_device_inplace: ++");
            int[] all = diff.CountNonzero();
            double r = 0;
            if (all[0] > 0 && all[1]>0 && all[2]>0)
            {
                Image<Hsv, Byte> hsvimg = diff.Convert<Hsv, Byte>();
                Image<Gray, Byte> mask = hsvimg.InRange(new Hsv(45, 100, 50), new Hsv(75, 255, 255));
                //diff.Save("temp_2.jpg");
                //Image<Gray, Byte> mask = diff.InRange(new Bgr(30, 60, 30), new Bgr(95, 130, 70)); //img.InRange(new Bgr(30, 60, 30), new Bgr(95, 130, 70));
                int[] area = mask.CountNonzero();
                r = (double)area[0] / (mask.Width * mask.Height);
                if (r < threshold)
                {
                    device_inplace = true;
                }
                ret = true;
            }
            //mask.Save("temp_3.jpg");
            Program.logIt($"check_device_inplace: -- {ret}, inplace={device_inplace} score={r}");
            return new Tuple<bool, bool>(ret, device_inplace);
        }
        static bool handle_motion(Image<Bgr, Byte> frane, Image<Bgr, Byte> bg)
        {
            bool ret = false;
            Rectangle roi = new Rectangle(744, 266, 576, 1116);

            Image<Bgr, Byte> img0 = bg.Copy();
            Image<Bgr, Byte> img1 = frane.Copy(roi);
                                 
            //img1.Save("temp_1.jpg");
            img0 = img1.AbsDiff(img0);
           
            
            Tuple<bool, bool> device_inplace = check_device_inplace(img1);
            if (device_inplace.Item1)
            {
                ret = device_inplace.Item2;
                if (ret)
                {
                    Program.logIt("Device Arrival");
                    Size sz = detect_size(img0.Mat.ToImage<Gray, Byte>());
                    if (sz.IsEmpty)
                    {
                        // error
                        frane.Save("temp_1.jpg");
                        bg.Save("temp_2.jpg");
                    }
                    else
                    {
                        Bgr rgb = sample_color(img1);
                        Program.logIt($"device: size={sz}, color={rgb}");
                        Tuple<bool, int, int> res = predict_color_and_size(rgb, sz);
                        if (res.Item1)
                        {
                            Console.WriteLine($"device=ready");
                            Console.WriteLine($"colorid={res.Item2}");
                            Console.WriteLine($"sizeid={res.Item3}");
                        }
                    }
                }
                else
                {
                    Program.logIt("Device Removal");
                    Console.WriteLine($"device=removed");
                }
            }
            return ret;
        }
        static Tuple<bool, int,int> predict_color_and_size(Bgr rgb, Size sz, Dictionary<string,object> args=null)
        {
            bool retb = false;
            int color_id = -1;
            int size_id = -1;
            try
            {
                string dir = System.IO.Path.GetDirectoryName(Program.getCurrentExeFilename());
                if (args != null)
                    dir = args["root"] as string;
                using (SVM model = new SVM())
                {
                    model.Load(System.IO.Path.Combine(dir,"traindata","iPhone_color.xml"));
                    Matrix<float> test = new Matrix<float>(1, 3);
                    test[0, 0] = (float)rgb.Red;
                    test[0, 1] = (float)rgb.Green;
                    test[0, 2] = (float)rgb.Blue;
                    color_id = (int)model.Predict(test);
                    Program.logIt($"prodict: colorID={color_id}");
                }
                using (SVM model = new SVM())
                {
                    model.Load(System.IO.Path.Combine(dir, "traindata", "iPhone_size.xml"));
                    //model.Load(@"traindata/iPhone_size.xml");
                    Matrix<float> test = new Matrix<float>(1, 2);
                    test[0, 0] = (float)sz.Width;
                    test[0, 1] = (float)sz.Height;
                    size_id = (int)model.Predict(test);
                    Program.logIt($"prodict: sizeID={size_id}");
                }
                retb = true;
            }
            catch (Exception) { }
            return new Tuple<bool, int, int>(retb, color_id, size_id);
        }
        static bool handle_motion_V2(Image<Bgr, Byte> frane, Image<Gray, Byte> bg, int idx)
        {
            bool device_in_place = false;
            //Rectangle r = new Rectangle(196, 665, 269, 628);
            Rectangle r = new Rectangle(334, 774, 452, 1016);
            Image<Bgr, Byte> img1 = frane.Copy(r);
            Image<Gray, Byte> imgg = frane.Mat.ToImage<Gray, Byte>().Copy(r);
            Image<Gray, Byte> imgbg = bg.Copy(r);
            imgg = imgg.AbsDiff(imgbg);
            Gray g = imgg.GetAverage();
            if (g.MCvScalar.V0 > 13)
            {
                Rectangle sz = detect_size_old(imgg);
                Bgr rgb = sample_color(img1);
                Program.logIt($"Device arrival. size: {sz.Size}, color: {rgb} ({g.MCvScalar.V0})");
                // report 
                Console.WriteLine($"Raw Data: size={sz.Size}, color={rgb}");
                if (sz.Size.Width > 430)
                {
                    // it is plus mode
                    Console.WriteLine($"sizeID=2");
                }
                else if (sz.Size.Width < 400)
                {
                    // it is plus mode
                    Console.WriteLine($"sizeID=1");
                }
                else
                {
                    // error. unknown size
                    Console.WriteLine($"sizeID=-1");
                }

                // for color
                string[] color_note = new string[] 
                {
                    "NA",
                    "Blue (iPhone XR)",
                    "Gray (iPhone 8 Plus)",
                    "Red (iPhone 8 Plus)",
                    "Silver (iPhone 8/iPhone 8 Plus)",
                };

                try
                {
                    Console.WriteLine($"r={rgb.Red}");
                    Console.WriteLine($"g={rgb.Green}");
                    Console.WriteLine($"b={rgb.Blue}");
                    using (SVM model = new SVM())
                    {
                        model.Load(@"traindata/iPhone_color.xml");
                        Matrix<float> test = new Matrix<float>(1, 3);
                        test[0, 0] = (float)rgb.Red;
                        test[0, 1] = (float)rgb.Green;
                        test[0, 2] = (float)rgb.Blue;
                        int l = (int)model.Predict(test);
                        Console.WriteLine($"colorID={l}");
                        if(l>=0 && l<color_note.Length)
                            Console.WriteLine($"colorNote={color_note[l]}");
                    }
                }
                catch (Exception) { }
                //Console.WriteLine("Enter device model and color:");
                //string info = System.Console.ReadLine();
                //img1.Save($"temp_{info}_2.jpg");
                //imgbg.Save($"temp_{info}_1.jpg");
                //imgg.Save($"temp_{info}_3.jpg");
                //Console.WriteLine($"{info}: size={sz.Size}, color={rgb}");
                //Program.logIt($"{info}: size={sz.Size}, color={rgb}");
                device_in_place = true;
            }
            else
            {
                Program.logIt($"Device removal. ({g.MCvScalar.V0})");
                device_in_place = false;
            }
            return device_in_place;
        }
        static bool handle_motion(Image<Bgr, Byte> frane, Image<Gray,Byte> bg, int idx)
        {
            bool device_in_place = false;
            //Rectangle r = new Rectangle(196, 665, 269, 628);
            Rectangle r = new Rectangle(334, 774, 452, 1016);
            Image<Bgr, Byte> img1 = frane.Copy(r);
            Image<Gray, Byte> imgg = frane.Mat.ToImage<Gray, Byte>().Copy(r);
            Image<Gray, Byte> imgbg = bg.Copy(r);
            imgg = imgg.AbsDiff(imgbg);
            Gray g = imgg.GetAverage();
            if (g.MCvScalar.V0 > 17)
            {
                Rectangle sz = detect_size_old(imgg);
                Bgr rgb = sample_color(img1);
                Program.logIt($"Device arrival. size: {sz.Size}, color: {rgb} ({g.MCvScalar.V0})");
                Console.WriteLine("Enter device model and color:");
                string info = System.Console.ReadLine();
                img1.Save($"temp_{info}_2.jpg");
                imgbg.Save($"temp_{info}_1.jpg");
                imgg.Save($"temp_{info}_3.jpg");
                Console.WriteLine($"{info}: size={sz.Size}, color={rgb}");
                Program.logIt($"{info}: size={sz.Size}, color={rgb}");
                device_in_place = true;
            }
            else
            {
                Program.logIt($"Device removal. ({g.MCvScalar.V0})");
                device_in_place = false;
            }
            return device_in_place;
        }
        public static Bgr sample_color(Image<Bgr, Byte> img, Rectangle rect = default(Rectangle))
        {
            //Rectangle r = new Rectangle(20, 810, 290, 30);
            //Rectangle r = new Rectangle(387, 106, 43, 267);
            Rectangle r = new Rectangle(375, 450, 30, 200);
            if (!rect.IsEmpty)
                r = rect;
            Image<Bgr, Byte> i = img.Copy(r);
            Bgr rgb = i.GetAverage();
            // debug
#if true
            Program.logIt($"b,g,r={rgb}");
            Image<Hsv, float> i1 = i.Convert<Hsv, float>();
            Hsv h = i1.GetAverage();
            Program.logIt($"hsv={h}");
#endif
            return rgb;
        }
        public static Size detect_size(Image<Gray, Byte> img)
        {
            Program.logIt("detect_size: ++");
            Mat m = new Mat();
            CvInvoke.Threshold(img, m, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
           
            Image<Gray, Byte> img1 = m.ToImage<Gray, Byte>();
            img1._Erode(2);
            Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
            img1._MorphologyEx(MorphOp.Gradient, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
            Rectangle roi = Rectangle.Empty;
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(img1, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    VectorOfPoint contour = contours[i];
                    double a = CvInvoke.ContourArea(contour);
                    Rectangle r = CvInvoke.BoundingRectangle(contour);
                    //if (a > 10.0)
                    {
                        //Program.logIt($"area: {a}, {r}");
                        if (roi.IsEmpty) roi = r;
                        else roi = Rectangle.Union(roi, r);
                    }
                }
            }
            Size sz = Size.Empty;
            if (!roi.IsEmpty && roi.Width<img.Width && roi.Height<img.Height)
            {
                sz = new Size(roi.X + roi.Width, roi.Y + roi.Height);
            }
            else
            {
                Program.logIt($"detect_size: Error! {roi}");
            }
            Program.logIt($"detect_size: -- {sz}");
            return sz;
        }
        static Rectangle detect_size_old(Image<Gray,Byte> img)
        {
            Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
            CvInvoke.GaussianBlur(img, img, new Size(3, 3), 0);
            Image<Gray, Byte> g = img.MorphologyEx(MorphOp.Gradient, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
            CvInvoke.Threshold(g, g, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
            Rectangle roi = Rectangle.Empty;
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(g, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    VectorOfPoint contour = contours[i];
                    double a = CvInvoke.ContourArea(contour);
                    Rectangle r = CvInvoke.BoundingRectangle(contour);
                    if (roi.IsEmpty) roi = r;
                    else roi = Rectangle.Union(roi, r);
                }
            }
            return roi;
        }
        static bool all_same_frames(Image<Bgr, Byte>[] frames, double th = 11)
        {
            bool ret = false;
            Image<Bgr, Byte> m0 = frames[0];
            if (m0 != null)
            {
                bool all_same = true;
                for (int i = 1; i < frames.Length && all_same; i++)
                {
                    if (frames[i] != null && frames[i].Width == m0.Width && frames[i].Height == m0.Height)
                    {
                        Image<Bgr, Byte> diff = m0.AbsDiff(frames[i]);
                        Gray ga = diff.Convert<Gray, byte>().GetAverage();
                        if (ga.MCvScalar.V0 > th)
                        {
                            Program.logIt($"image diff: score={ga.MCvScalar.V0}");
                            all_same = false;
                        }
                    }
                    else
                        all_same = false;
                }
                ret = all_same;
            }
            return ret;
        }
        static int check_size(double score, Dictionary<string,object> cfg)
        {
            int ret = 0;
#if true
            if (cfg.ContainsKey("size"))
            {
                Dictionary<string, object> size_cfg = (Dictionary<string, object>)cfg["size"];
                foreach (KeyValuePair<string, object> kvp in size_cfg)
                {
                    try
                    {
                        ArrayList al = (ArrayList)kvp.Value;
                        double d1 = System.Decimal.ToDouble((System.Decimal)al[0]);
                        double d2 = System.Decimal.ToDouble((System.Decimal)al[1]);
                        double r = Math.Abs(score - d1) / d1;
                        if (r < d2)
                        {
                            // ok
                            ret = Int32.Parse(kvp.Key);
                            break;
                        }
                    }
                    catch (Exception) { }
                }
            }

#else
            if (ret == 0)
            {
                double diff = Math.Abs(score - 0.13);
                double r = diff / 0.13;
                if (r < 0.1)
                    ret = 2;
            }
            if (ret == 0)
            {
                double diff = Math.Abs(score - 0.28);
                double r = diff / 0.28;
                if (r < 0.1)
                    ret = 3;
            }
            if (ret == 0)
            {
                double diff = Math.Abs(score - 0.23);
                double r = diff / 0.23;
                if (r < 0.1)
                    ret = 4;
            }
#endif
            return ret;
        }
    }
}
