using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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
            return 0;

        }
        static void test()
        {
            string folder = @"C:\Tools\avia\images\Final270";
            foreach(string fn in System.IO.Directory.GetFiles(folder, "*.6.bmp", System.IO.SearchOption.AllDirectories))
            {
                GC.Collect();
                Console.WriteLine(fn);
                DateTime _start = DateTime.Now;
                Rectangle r = get_rectangle_by_canny(fn);
                Console.WriteLine($"Detect size: {toFloat(r)}");
                //Console.WriteLine($"Canny: {r} and {toFloat(r)}, took: {DateTime.Now-_start}");
                //_start = DateTime.Now;
                //Rectangle r1 = get_rectangle_by_sobel(fn);
                //Console.WriteLine($"Sobel: {r1} and {toFloat(r1)}, took: {DateTime.Now - _start}");
            }
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
    }
}
