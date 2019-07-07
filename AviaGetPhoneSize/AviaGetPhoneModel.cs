using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tesseract;

namespace AviaGetPhoneSize
{
    class AviaGetPhoneModel
    {
        static int Main(string[] args)
        {
            int ret = 0;
            //test_check_apple_device();
            //test();
            //test_ocr();
            extract_phone_image();
            return ret;
        }

        static void extract_phone_image()
        {
            //string fn = @"C:\Tools\avia\images\test.1\iphone_6\AP001-iphone6_gold\8301.1.bmp";
            foreach (string fn in System.IO.Directory.GetFiles(@"C:\Tools\avia\images\test.1\iphone_6", "*.bmp", System.IO.SearchOption.AllDirectories))
            {
                Mat m = CvInvoke.Imread(fn);
                string f = System.IO.Path.Combine("output", "iphone_6", System.IO.Path.GetFileName(fn));
                Image<Gray, Byte> img = m.ToImage<Gray, Byte>().Rotate(-90.0, new Gray(0), false);
                Rectangle roi = found_device_image(img.Resize(0.1, Inter.Cubic));
                Image<Gray, Byte> img0 = img.Copy(roi);
                img0.Save(f);
                Program.logIt($"{f}: {img0.Size}");
                //roi = found_apple_text_v4(img0);
                //img0.ROI = roi;
                //img0.Save(f);
                //test_ocr(img0, f);
                m = null;
                GC.Collect();
            }
        }
        static void test()
        {
            foreach(string fn in System.IO.Directory.GetFiles(@"C:\Tools\avia\images\test.1\iphone_6", "*.bmp", System.IO.SearchOption.AllDirectories))
            {
                Program.logIt($"check: {fn}");
                if (!check_apple_device(fn))
                {
                    Program.logIt($"Fail: {fn}");
                }
            }
            //check_apple_device(@"C:\Tools\avia\images\test.1\AP001-iphone6_gold\0123.1.bmp");
        }
        static void test_ocr(Image<Gray,Byte> img, string fn)
        {
            Image<Gray, Byte> img0 = img.SmoothGaussian(3);
            Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));
            img0 = img0.MorphologyEx(MorphOp.Erode, k, new Point(-1, -1), 5, BorderType.Default, new MCvScalar(0));
            CvInvoke.Threshold(img0, img0, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
            using (TesseractEngine TE = new TesseractEngine("tessdata", "eng", EngineMode.TesseractOnly))
            {
                //img0.Save("temp_1.jpg");
                var p = TE.Process(img0.ToBitmap());
                string s = p.GetText();
                Program.logIt($"{fn}: {s}");
                s = p.GetHOCRText(0);
            }
            
        }
        static void test_check_apple_device()
        {            
            string fn = @"C:\Tools\avia\images\test.1\iphone_6\AP002-iphone6_Gray\8738.1.bmp";
            //Mat m = CvInvoke.Imread(fn);
            //Image<Gray, Byte> img = m.ToImage<Gray, Byte>().Rotate(90.0, new Gray(0), false);
            //Rectangle roi = found_device_image(img.Resize(0.1, Inter.Cubic));
            //Image<Gray, Byte> img1 = img.Copy(roi);
            //img1.Save("temp_1.jpg");

            //roi = found_apple_text(img1.Resize(0.1, Inter.Cubic));
            //if(roi.IsEmpty)
            //{
            //    roi = found_apple_text_v2(img1);
            //}
            //if (!roi.IsEmpty)
            //{
            //    img1.ROI = roi;
            //    img1.Save("temp_1.jpg");
            //    do_ocr(img1, new Regex(@"iPhone", RegexOptions.IgnoreCase));
            //}
            check_apple_device(fn);
        }
        static Tuple<bool,string> do_ocr(Image<Gray,Byte> img, Regex re)
        {
            bool ret = false;
            string retS = string.Empty;
            //img.Save("temp_txt.jpg");
            Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));
            int[] iter = new int[] { 1,3,5,7,9};
            foreach(int i in iter)
            {
                Image<Gray, Byte> img1 = img.SmoothGaussian(3);
                img1 = img1.MorphologyEx(MorphOp.Erode, k, new Point(-1, -1), i, BorderType.Default, new MCvScalar(0));
                using (TesseractEngine TE = new TesseractEngine("tessdata", "eng", EngineMode.TesseractOnly))
                {
                    var p = TE.Process(img1.ToBitmap());
                    string s = p.GetText();
                    if (re.Match(s).Success)
                    {
                        // return true
                        ret = true;
                        retS = p.GetHOCRText(0);
                    }
                }
                if (ret)
                    break;
            }
            return new Tuple<bool, string>(ret, retS);
        }
        static bool check_apple_device(string filename)
        {
            bool ret = false;
            if (System.IO.File.Exists(filename))
            {
                try
                {
                    Mat m = CvInvoke.Imread(filename);
                    Image<Gray, Byte> img0 = m.ToImage<Gray, Byte>().Rotate(-90.0, new Gray(0), false);
                    Rectangle roi = found_device_image(img0.Resize(0.1, Inter.Cubic));
                    Image<Gray, Byte> img1 = img0.Copy(roi);
                    //img1.Save("temp_1.jpg");
                    roi = found_apple_text_v2(img1);
                    Image<Gray, Byte> img_txt = img1.Copy(roi);
                    Tuple<bool, string> res = do_ocr(img_txt, new Regex(@"iPh[bo]ne", RegexOptions.IgnoreCase));
                    ret = res.Item1;
                    if (!ret)
                    {
                        roi = found_apple_text(img1.Resize(0.1, Inter.Cubic));
                        if (!roi.IsEmpty)
                        {
                            img_txt = img1.Copy(roi);
                            res = do_ocr(img_txt, new Regex(@"iPhone", RegexOptions.IgnoreCase));
                            ret = res.Item1;
                        }
                    }
                    GC.Collect();
                }
                catch (Exception) { }
            }
            return ret;
        }
        static public Rectangle found_device_image(Image<Gray, Byte> src, double ratio = 10)
        {
            Mat m = new Mat();
            CvInvoke.GaussianBlur(src, m, new Size(3, 3), 0);
            Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
            Image<Gray, Byte> img = m.ToImage<Gray, Byte>();
            img = img.MorphologyEx(MorphOp.Open, k, new Point(-1, -1), 3, BorderType.Default, new MCvScalar(0));
            img = img.MorphologyEx(MorphOp.Gradient, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
            //img.Save("temp_1.jpg");
            double v;
            v = CvInvoke.Threshold(img, img, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
            //img.Save("temp_1.jpg");
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
                    if (roi.IsEmpty) roi = r;
                    else roi = Rectangle.Union(roi, r);
                }
            }
            if (!roi.IsEmpty)
            {
                v = ratio * roi.X;
                roi.X = (int)v;
                v = ratio * roi.Y;
                roi.Y = (int)v;
                v = ratio * roi.Width;
                roi.Width = (int)v;
                v = ratio * roi.Height;
                roi.Height = (int)v;
            }
            //Program.logIt($"{roi}");
            return roi;
        }
        static Rectangle found_apple_text(Image<Gray, Byte> src, double ratio = 10)
        {
            Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
            int[] iter = new int[] { 1, 3, 5, 7, 9 };
            Rectangle roi = Rectangle.Empty;
            foreach (int it in iter)
            {
                //img = img.MorphologyEx(MorphOp.Open, k, new Point(-1, -1), 3, BorderType.Default, new MCvScalar(0));
                Image<Gray,Byte> img = src.MorphologyEx(MorphOp.Erode, k, new Point(-1, -1), it, BorderType.Default, new MCvScalar(0));
                img = img.MorphologyEx(MorphOp.Gradient, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
                //img = img.MorphologyEx(MorphOp.Open, k, new Point(-1, -1), 3, BorderType.Default, new MCvScalar(0));
                double db = CvInvoke.Threshold(img, img, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
                //img.Save("temp_1.jpg");
                Point p = new Point(img.Width / 2, img.Height * 3 / 4);
                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(img, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
                    int count = contours.Size;
                    for (int i = 0; i < count; i++)
                    {
                        VectorOfPoint contour = contours[i];
                        double a = CvInvoke.ContourArea(contour);
                        if (a > 100 && a<1000)
                        {
                            Rectangle r = CvInvoke.BoundingRectangle(contour);
                            if (r.Contains(p))
                            {
                                roi = r;
                                //Program.logIt($"area={a}, r={roi}");
                            }
                        }
                    }
                }
                if (!roi.IsEmpty)
                    break;
            }
            if (!roi.IsEmpty)
            {
                Size sz = new Size(roi.Width / 10, roi.Height / 10);
                roi = Rectangle.Inflate(roi, sz.Width, sz.Height);
                double v = ratio * roi.X;
                roi.X = (int)v;
                v = ratio * roi.Y;
                roi.Y = (int)v;
                v = ratio * roi.Width;
                roi.Width = (int)v;
                v = ratio * roi.Height;
                roi.Height = (int)v;
            }
            //Program.logIt($"{roi}");
            return roi;
        }
        static Rectangle found_apple_text_v2(Image<Gray, Byte> src)
        {
            Rectangle r = new Rectangle(src.Width / 4, src.Height * 7 / 10, src.Width / 2, src.Height / 10);
            return r;
        }
        static Rectangle found_apple_text_v3(Image<Gray, Byte> src)
        {
            Rectangle r = new Rectangle(src.Width / 5, src.Height * 3 / 4, src.Width * 3/ 5, src.Height / 10);
            return r;
        }
        static Rectangle found_apple_text_v4(Image<Gray, Byte> src)
        {
            // height == 9880 is iphone X
            // height == 9620 is iphone 6
            // x <= width * 38%
            double x = 0.30 * src.Width;
            double w = 0.4 * src.Width;
            // y <= height * 72.212 %
            double y = 0.7 * src.Height;
            // height >= height * 3.825% ~ 3.853%
            double h = 0.1 * src.Height;
            if(src.Height<9800)
                h = 0.07 * src.Height;
            Rectangle r = new Rectangle((int)x, (int)y, (int)w, (int)h);
            return r;
        }
    }
}
