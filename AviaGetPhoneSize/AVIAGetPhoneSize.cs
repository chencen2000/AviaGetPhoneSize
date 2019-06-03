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
            Rectangle r = get_rectangle(@"C:\Tools\avia\images\Final270\iphone6 Gold\0123.6.bmp");
            Console.WriteLine($"{r} and {toFloat(r)}");
            return 0;
        }
        static RectangleF toFloat(Rectangle r, float ratio= 0.0139339f)
        {
            RectangleF ret = new RectangleF(ratio * r.X, ratio * r.Y, ratio * r.Width, ratio * r.Height);
            return ret;
        }
        static Rectangle get_rectangle(string filename)
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
    }
}
