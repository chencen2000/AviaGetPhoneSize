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
using System.Xml;
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
            //extract_phone_image();
            //test_ml();
            test_1();
            //test_2();
            //test_3();
            //save_template_image();
            return ret;
        }

        static void extract_phone_image()
        {
            //string fn = @"C:\Tools\avia\images\test.1\iphone7 MatteBlack_img\0342.1.bmp";
            foreach (string fn in System.IO.Directory.GetFiles(@"C:\Tools\avia\images\final270.1", "*.1.bmp", System.IO.SearchOption.AllDirectories))
            {
                Mat m = CvInvoke.Imread(fn);
                string f = System.IO.Path.Combine("output", "temp", System.IO.Path.GetFileName(fn));
                Image<Gray, Byte> img = m.ToImage<Gray, Byte>().Rotate(90.0, new Gray(0), false);
                Rectangle roi = found_device_image_v2(img.Resize(0.1, Inter.Cubic));
                if (!roi.IsEmpty)
                {
                    Image<Gray, Byte> img0 = img.Copy(roi);
                    if (System.IO.File.Exists(f))
                        Program.logIt($"{f} already exists");
                    else
                        img0.Save(f);
                    //img0.Save(f);
                    //double norm = CvInvoke.Norm(img);
                    //MCvScalar mean = new MCvScalar();
                    //MCvScalar stdDev = new MCvScalar();
                    //CvInvoke.MeanStdDev(img, ref mean, ref stdDev);
                    //Program.logIt($"{fn}: norm={norm}, mean={mean.V0}, stdDev={stdDev.V0}");
#if false
                    roi = found_apple_text_iPhone_7(img0);
                    if (!roi.IsEmpty)
                    {
                        Image<Gray, Byte> img_txt = img0.GetSubRect(roi);
                        test_ocr(img_txt, f);
                        double norm = CvInvoke.Norm(img_txt);
                        Gray g1 = new Gray(0);
                        MCvScalar mean1 = new MCvScalar();
                        img_txt.AvgSdv(out g1, out mean1);
                        Program.logIt($"{fn}: norm={norm}, mean={g1}, stdDev={mean1.V0}");
                        img_txt._EqualizeHist();
                        img_txt._GammaCorrect(4.0d);
                        img_txt.Save(f);
                        test_ocr(img_txt, f);
                        norm = CvInvoke.Norm(img_txt);
                        g1 = new Gray(0);
                        mean1 = new MCvScalar();
                        img_txt.AvgSdv(out g1, out mean1);
                        Program.logIt($"{fn}: norm={norm}, mean={g1}, stdDev={mean1.V0}");
                    }
#endif
                }
                m = null;
                GC.Collect();
            }
        }
        static void test()
        {
            foreach(string fn in System.IO.Directory.GetFiles(@"C:\Tools\avia\images\test.1\iphone7 MatteBlack_img", "*.bmp", System.IO.SearchOption.AllDirectories))
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
        static public Rectangle found_device_image_v2(Image<Gray, Byte> src, double ratio = 10)
        {
            Rectangle ret = Rectangle.Empty;
            Mat m = new Mat();
            CvInvoke.GaussianBlur(src, m, new Size(3, 3), 0);
            CvInvoke.Threshold(m, m, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
            Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
            CvInvoke.MorphologyEx(m, m, MorphOp.Gradient, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
            //m.Save("temp_1.jpg");
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(m, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    VectorOfPoint contour = contours[i];
                    double a = CvInvoke.ContourArea(contour);
                    Rectangle r = CvInvoke.BoundingRectangle(contour);
                    if (ret.IsEmpty) ret = r;
                    else ret = Rectangle.Union(ret, r);
                }
            }
            if (!ret.IsEmpty)
            {
                double v = ratio * ret.X;
                ret.X = (int)v;
                v = ratio * ret.Y;
                ret.Y = (int)v;
                v = ratio * ret.Width;
                ret.Width = (int)v;
                v = ratio * ret.Height;
                ret.Height = (int)v;
            }
            //Program.logIt($"{ret}");
            return ret;
        }
        static public Rectangle found_device_image(Image<Gray, Byte> src, double ratio = 10)
        {
            Mat m = new Mat();
            CvInvoke.GaussianBlur(src, m, new Size(3, 3), 0);
            Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
            Image<Gray, Byte> img = m.ToImage<Gray, Byte>();
            img = img.MorphologyEx(MorphOp.Open, k, new Point(-1, -1), 9, BorderType.Default, new MCvScalar(0));
            img = img.MorphologyEx(MorphOp.Gradient, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
            img.Save("temp_1.jpg");
            double v;
            v = CvInvoke.Threshold(img, img, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
            img.Save("temp_1.jpg");
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
            Program.logIt($"{roi}");
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
            double x = 0.30 * src.Width;
            double w = 0.4 * src.Width;
            double y = 0.7 * src.Height;
            double h = 0.07 * src.Height;
            Rectangle r = new Rectangle((int)x, (int)y, (int)w, (int)h);
            return r;
        }
        static Rectangle found_apple_text_iPhone_7(Image<Gray, Byte> src)
        {
            double x = 0.30 * src.Width;
            double w = 0.4 * src.Width;
            double y = 0.72 * src.Height;
            double h = 0.07 * src.Height;
            Rectangle r = new Rectangle((int)x, (int)y, (int)w, (int)h);
            return r;
        }
        static void test_ml()
        {
            //foreach (string fn in System.IO.Directory.GetFiles(@"C:\Tools\avia\images\final270.1", "*.1.bmp", System.IO.SearchOption.AllDirectories))
            //{
            //    string m = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(fn));
            //    string f = System.IO.Path.Combine("output", "train_data", $"{m}.{System.IO.Path.GetFileName(fn)}");
            //    System.IO.File.Copy(fn, f);
            //}
            Dictionary<string, string> files = new Dictionary<string, string>();
            foreach (string fn in System.IO.Directory.GetFiles(@"output\train_data", "*.1.bmp", System.IO.SearchOption.AllDirectories))
            {
                string f = System.IO.Path.GetFileName(fn);
                string m = f.Split(' ')[0];
                files.Add(fn, m);
            }

        }
        static string get_path_by_node(XmlNode node)
        {
            Stack<string> sk = new Stack<string>();
            while (node != null)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    sk.Push(node.Name);
                    node = node.ParentNode;
                }
                if (node.NodeType == XmlNodeType.Document)
                    break;
            }
            StringBuilder sb = new StringBuilder();
            foreach(string s in sk)
            {
                sb.Append(s);
                sb.Append('/');
            }
            return sb.ToString();
        }
        static Tuple<Rectangle, bool, string>[] retrieve_area_by_filename(string filename)
        {
            List<Tuple<Rectangle, bool, string>> area = new List<Tuple<Rectangle, bool, string>>();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filename);
                XmlNodeList reg_list = doc.DocumentElement.SelectNodes("//region");
                foreach (XmlNode n in reg_list)
                {
                    Rectangle r = Rectangle.Empty;
                    Point p = Point.Empty;
                    bool is_mask = true;
                    string path = get_path_by_node(n);
                    if (n["is_mask"] != null)
                    {
                        if (Int32.Parse(n["is_mask"].InnerText) == 0)
                            is_mask = false;
                    }
                    //if (is_mask == 1)
                    {
                        if (n["center"] != null)
                        {
                            string s = n["center"].InnerText;
                            string[] ss = s.Split(',');
                            float x = float.Parse(ss[0]);
                            float y = float.Parse(ss[1]);
                            PointF pf = new PointF(x, y);
                            p = Point.Round(pf);
                        }
                        if (n["radius"] != null)
                        {
                            float f = float.Parse(n["radius"].InnerText);
                            int x = p.X - (int)f;
                            int y = p.Y - (int)f;
                            int w = (int)(2 * f);
                            int h = (int)(2 * f);
                            r = new Rectangle(x, y, w, h);
                        }
                        if (n["width"] != null && n["height"] != null)
                        {
                            int w = int.Parse(n["width"].InnerText);
                            int h = int.Parse(n["height"].InnerText);
                            int x = p.X - w / 2;
                            int y = p.Y - h / 2;
                            r = new Rectangle(x, y, w, h);
                        }
                        if (!r.IsEmpty)
                        {
                            //Program.logIt($"center={p}, rect={r}");
                            Tuple<Rectangle, bool, string> i = new Tuple<Rectangle, bool, string>(r, is_mask,path);
                            area.Add(i);
                        }
                    }
                }
            }
            catch (Exception) { }
            return area.ToArray();
        }
        static void save_template_image()
        {
            string folder = @"C:\Tools\avia\20190607-VZW-Model-Loose-270";
            //string[] models = System.IO.Directory.GetDirectories(folder);
            foreach(string mf in System.IO.Directory.GetDirectories(folder))
            {
                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                string model = System.IO.Path.GetFileName(mf);
                System.IO.Directory.CreateDirectory($@"output\template\{model}");
                string xmlfile = System.IO.Path.Combine(mf, "work_station_1", "layout.xml");
                string imgfile = System.IO.Path.Combine(mf, "work_station_1", "image.bmp");
                //StringBuilder sb = new StringBuilder();
                Image<Gray, Byte> img0 = new Image<Gray, byte>(imgfile);
                Tuple<Rectangle, bool, string>[] area = retrieve_area_by_filename(xmlfile);
                for (int i = 0; i < area.Length; i++)
                {
                    Image<Gray, Byte> m0 = img0.Copy(area[i].Item1);
                    string s = $"{i}: {area[i].Item1} {area[i].Item2} {area[i].Item3}";
                    Program.logIt(s);
                    //sb.AppendLine(s);
                    m0.Save($@"output\template\{model}\temp_{i}.jpg");
                    Dictionary<string, object> r = new Dictionary<string, object>();
                    r.Add("id", i);
                    //r.Add("rect", area[i].Item1);
                    r.Add("x", area[i].Item1.X);
                    r.Add("y", area[i].Item1.Y);
                    r.Add("width", area[i].Item1.Width);
                    r.Add("height", area[i].Item1.Height);
                    r.Add("xpath", area[i].Item3);
                    data.Add(r);
                }
                //System.IO.File.WriteAllText($@"output\template\{model}\info.txt", sb.ToString());
                try
                {
                    var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                    string s = jss.Serialize(data);
                    System.IO.File.WriteAllText($@"output\template\{model}\info.json", s);
                }
                catch (Exception) { }
            }
            /*
            StringBuilder sb = new StringBuilder();
            string imgfile = @"C:\Tools\avia\20190607-VZW-Model-Loose-270\Iphone7 plus Red\work_station_1\image.bmp";
            string xmlfile = @"C:\Tools\avia\20190607-VZW-Model-Loose-270\Iphone7 plus Red\work_station_1\layout.xml";
            Image<Gray, Byte> img0 = new Image<Gray, byte>(imgfile);
            Tuple<Rectangle, bool, string>[] area = retrieve_area_by_filename(xmlfile);
            for (int i = 0; i < area.Length; i++)
            {
                Image<Gray, Byte> m0 = img0.Copy(area[i].Item1);
                string s = $"{i}: {area[i].Item1} {area[i].Item2} {area[i].Item3}";
                Program.logIt(s);
                sb.AppendLine(s);
                m0.Save($@"output\template\Iphone7 plus Red\temp_{i}.jpg");
            }
            System.IO.File.WriteAllText($@"output\template\Iphone7 plus Red\info.txt", sb.ToString());
            */
        }
        static void test_3()
        {
            string imgfile = @"C:\Tools\avia\20190607-VZW-Model-Loose-270\Iphone7 plus Red\work_station_1\image.bmp";
            string xmlfile = @"C:\Tools\avia\20190607-VZW-Model-Loose-270\Iphone7 plus Red\work_station_1\layout.xml";
            Image<Gray, Byte> img0 = new Image<Gray, byte>(imgfile);
            Tuple<Rectangle, bool, string>[] area = retrieve_area_by_filename(xmlfile);
            string fn = @"C:\Tools\avia\images\final_270\iphone7 Plus Red\0656.1.bmp";
            Image<Gray, Byte> img1 = new Image<Gray, byte>(fn);
            for (int i=0; i<area.Length; i++)
            {
                if (area[i].Item2)
                {
                    try
                    {
                        Image<Gray, Byte> m0 = img0.Copy(area[i].Item1);
                        //Rectangle r = area[i].Item1;
                        //Rectangle.Inflate(r, 10, 10);
                        //Image<Gray, Byte> m1 = img1.Copy(Rectangle.Inflate(r, 100, 100));
                        Image<Gray, Byte> m1 = img1.Copy(area[i].Item1);
                        Program.logIt($"{i}: {area[i].Item1} {area[i].Item2} {area[i].Item3} ");
                        m0.Save($@"output\temp\temp_{i}_0.jpg");
                        m1.Save($@"output\temp\temp_{i}_1.jpg");
                        Image<Gray, float> res = m1.MatchTemplate(m0, TemplateMatchingType.CcoeffNormed);
                        double[] minValues, maxValues;
                        Point[] minLocations, maxLocations;
                        res.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
                        Program.logIt($"{i}: simi={maxValues[0]}, {maxLocations[0]}");
                    }
                    catch (Exception) { }
                }
            }
        }
        static void test_2()
        {
            string imgfile = @"C:\Tools\avia\A07- NEW2.4.3.3\Allmodels\AP001\work_station_1\image.bmp";
            string xmlfile = @"C:\Tools\avia\A07- NEW2.4.3.3\Allmodels\AP001\work_station_1\layout.xml";
            Tuple<Rectangle, bool, string>[] area = retrieve_area_by_filename(xmlfile);
            Rectangle rm = Rectangle.Empty;
            foreach (Tuple<Rectangle, bool,string> a in area)
            {
                if (a.Item1.Width * a.Item1.Height > rm.Height * rm.Width)
                    rm = a.Item1;
            }
            Image<Gray, Byte> img0 = new Image<Gray, byte>(imgfile).Copy(rm).Rotate(180, new Gray(0), false);

            //string fn = @"C:\Tools\avia\images\final_270\iphone6s Gray\1580.1.bmp";
            foreach (string fn in System.IO.Directory.GetFiles(@"C:\Tools\avia\images\final_270\iphone6 Gold"))
            {
                Image<Gray, Byte> img1 = new Image<Gray, byte>(fn);
                //Image<Gray, Byte> img = m1.ToImage<Gray, Byte>().Copy(rm);
                Image<Gray, float> res = img1.MatchTemplate(img0, TemplateMatchingType.CcoeffNormed);
                double[] minValues, maxValues;
                Point[] minLocations, maxLocations;
                res.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
                Program.logIt($"simi={maxValues[0]}, {maxLocations[0]}");
            }


        }
        static Tuple<Rectangle, string, string>[] get_roi_area(string model, string color="")
        {
            List<Tuple<Rectangle, string, string>> ret = new List<Tuple<Rectangle, string, string>>();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(@"test\iphone_layout.xml");
                string xpath = "";
                if (string.IsNullOrEmpty(color))
                    xpath = $@"model[@name='{model}' and not(@color)]";
                else
                    xpath = $@"model[@name='{model}' and @color='{color}']";
                XmlNode node = doc.DocumentElement.SelectSingleNode(xpath);
                if (node != null)
                {
                    XmlNodeList nodes = node.SelectNodes("rectangle");
                    foreach (XmlNode n in nodes)
                    {
                        string id = n.Attributes?["id"]?.Value;
                        string type = n.Attributes?["type"]?.Value;
                        int x, y, w, h;
                        if (Int32.TryParse(n["x"]?.InnerText, out x) &&
                            Int32.TryParse(n["y"]?.InnerText, out y) &&
                            Int32.TryParse(n["width"]?.InnerText, out w) &&
                            Int32.TryParse(n["height"]?.InnerText, out h))
                        {
                            Rectangle r = new Rectangle(x, y, w, h);
                            ret.Add(new Tuple<Rectangle, string, string>(r, id, type));
                        }
                    }
                }
            }
            catch (Exception) { }
            return ret.ToArray();
        }
        static void test_1()
        {
#if !false
            string fn1 = @"C:\Tools\avia\images\final_270\iphone6 Gold\0123.1.bmp";
            is_iPhone_6_gold(fn1);
#else
            foreach (string fn in System.IO.Directory.GetFiles(@"C:\Tools\avia\images\final_270\iphone6 Gray"))
            {
                if (!is_iPhone_6_gold(fn))
                {
                    Program.logIt($"{fn} is not iPhone 6");
                }
            }
#endif
        }
        static bool is_iPhone_8Plus_spacegray(string filename, double threshold=0.60)
        {
            bool ret = false;
            double score = 0.0;
            string root = @"images\template\iphone8 plus spacegray";
            Program.logIt($"is_iPhone_8Plus_spacegray: ++ {filename}");
            //foreach (string fn in System.IO.Directory.GetFiles(@"C:\Tools\avia\images\final_270\iphone6 Gold"))
            {
                string test_img = filename;
                Image<Gray, Byte> img = new Image<Gray, byte>(test_img);
                List<Dictionary<string, object>> data = null;
                try
                {
                    string s = System.IO.File.ReadAllText(System.IO.Path.Combine(root, @"info.json"));
                    var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                    data = jss.Deserialize<List<Dictionary<string, object>>>(s);
                }
                catch (Exception) { }

                var area_data = data.Where(x => x.ContainsKey("type") && x["type"].ToString() == "match");
                List<double> scores = new List<double>();
                foreach (var a in area_data)
                {
                    Rectangle r = new Rectangle((int)a["x"], (int)a["y"], (int)a["width"], (int)a["height"]);
                    Mat m = CvInvoke.Imread(System.IO.Path.Combine(root, $"temp_{a["id"]}.jpg"), ImreadModes.Grayscale);
                    //a.Add("image", m);
                    Image<Gray, Byte> img_t = img.Copy(r);
                    Image<Gray, float> mm = img_t.MatchTemplate(m.ToImage<Gray,Byte>(), TemplateMatchingType.CcoeffNormed);
                    double[] minValues, maxValues;
                    Point[] minLocations, maxLocations;
                    mm.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
                    Program.logIt($"id: {a["name"]}, match: {maxValues[0]}");
                    scores.Add(maxValues[0]);
                }
                score = scores.Average();
                if (score > threshold)
                    ret = true;
            }
            Program.logIt($"is_iPhone_8Plus_spacegray: -- {ret} score={score}");
            return ret;

        }
        static bool is_iPhone_8PlusRed(string filename)
        {
            bool ret = false;
            Program.logIt($"is_iPhone_8PlusRed: ++ {filename}");
            //foreach (string fn in System.IO.Directory.GetFiles(@"C:\Tools\avia\images\final_270\iphone6 Gold"))
            {
                string test_img = filename;
                Image<Gray, Byte> img = new Image<Gray, byte>(test_img);
                List<Dictionary<string, object>> data = null;
                try
                {
                    string s = System.IO.File.ReadAllText(@"images\template\Iphone8 plus Red\info.json");
                    var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                    data = jss.Deserialize<List<Dictionary<string, object>>>(s);
                }
                catch (Exception) { }

                var area_data = data.Where(x => x.ContainsKey("type") && x["type"].ToString() == "match");
                foreach(var a in area_data)
                {
                    a.Add("rectangle", new Rectangle((int)a["x"], (int)a["y"], (int)a["width"], (int)a["height"]));
                    Mat m = CvInvoke.Imread($@"images\template\Iphone8 plus Red\temp_{a["id"]}.jpg");
                    a.Add("image", m);
                }
            }
            Program.logIt($"is_iPhone_8PlusRed: -- {ret}");
            return ret;
        }
        static bool is_iPhone_7PlusRed(string filename)
        {
            bool ret = false;
            Program.logIt($"is_iPhone_7PlusRed: ++ {filename}");
            //foreach (string fn in System.IO.Directory.GetFiles(@"C:\Tools\avia\images\final_270\iphone6 Gold"))
            {
                string test_img = filename;
                Image<Gray, Byte> img = new Image<Gray, byte>(test_img);
                List<Dictionary<string, object>> data = null;
                try
                {
                    string s = System.IO.File.ReadAllText(@"images\template\Iphone7 plus Red\info.json");
                    var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                    data = jss.Deserialize<List<Dictionary<string, object>>>(s);
                }
                catch (Exception) { }

                var area = data.Where(x => x.ContainsKey("name") && x["name"].ToString() == "model").First();
                Rectangle r = new Rectangle((int)area["x"], (int)area["y"], (int)area["width"], (int)area["height"]);
                Image<Gray, Byte> img1 = img.Copy(r).Rotate(90, new Gray(0), false);
                Regex re = new Regex(area["regex"].ToString(), RegexOptions.Singleline);
                img1.Save("temp_1.jpg");

                CvInvoke.GaussianBlur(img1, img1, new Size(3, 3), 0);
                img1 = img1.InRange(new Gray(85), new Gray(95));
                img1.Save("temp_2.jpg");
                Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
                img1 = img1.MorphologyEx(MorphOp.Open, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
                img1.Save("temp_2.jpg");
                //img1 = img1.MorphologyEx(MorphOp.Erode, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
                //img1.Save("temp_2.jpg");
                using (TesseractEngine TE = new TesseractEngine("tessdata", "eng", EngineMode.TesseractOnly))
                {
                    //Bitmap b = new Bitmap(@"temp_text_3.jpg");
                    var p = TE.Process(img1.ToBitmap());
                    string s = p.GetText();
                    if (re.Match(s).Success)
                    {
                        //Program.logIt($"{System.IO.Path.GetFileName(test_img)}: is iPhone 6");
                        ret = true;
                    }
                    else
                    {
                        s = p.GetHOCRText(0);
                        Program.logIt($"{System.IO.Path.GetFileName(test_img)}:");
                        Program.logIt(s);
                        Program.logIt(System.Environment.NewLine);
                    }
                }
            }
            Program.logIt($"is_iPhone_7PlusRed: -- {ret}");
            return ret;
        }
        static bool is_iPhone_7Plus(string filename)
        {
            bool ret = false;
            Program.logIt($"is_iPhone_7Plus: ++ {filename}");
            //foreach (string fn in System.IO.Directory.GetFiles(@"C:\Tools\avia\images\final_270\iphone6 Gold"))
            {
                string test_img = filename;
                Image<Gray, Byte> img = new Image<Gray, byte>(test_img);
                List<Dictionary<string, object>> data = null;
                try
                {
                    string s = System.IO.File.ReadAllText(@"images\template\iphone7 plus gold\info.json");
                    var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                    data = jss.Deserialize<List<Dictionary<string, object>>>(s);
                }
                catch (Exception) { }

                var area = data.Where(x => x.ContainsKey("name") && x["name"].ToString() == "model").First();
                Rectangle r = new Rectangle((int)area["x"], (int)area["y"], (int)area["width"], (int)area["height"]);
                Image<Gray, Byte> img1 = img.Copy(r).Rotate(90, new Gray(0), false);
                Regex re = new Regex(area["regex"].ToString(), RegexOptions.Singleline);
                //img1.Save("temp_1.jpg");

                CvInvoke.GaussianBlur(img1, img1, new Size(3, 3), 0);
                double v = CvInvoke.Threshold(img1, img1, 0, 255, ThresholdType.BinaryInv | ThresholdType.Otsu);
                //img1.Save("temp_1.jpg");
                Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
                img1 = img1.MorphologyEx(MorphOp.Open, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
                //img1.Save("temp_2.jpg");
                //img1 = img1.MorphologyEx(MorphOp.Erode, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
                //img1.Save("temp_2.jpg");
                using (TesseractEngine TE = new TesseractEngine("tessdata", "eng", EngineMode.TesseractOnly))
                {
                    //Bitmap b = new Bitmap(@"temp_text_3.jpg");
                    var p = TE.Process(img1.ToBitmap());
                    string s = p.GetText();
                    if (re.Match(s).Success)
                    {
                        //Program.logIt($"{System.IO.Path.GetFileName(test_img)}: is iPhone 6");
                        ret = true;
                    }
                    else
                    {
                        s = p.GetHOCRText(0);
                        Program.logIt($"{System.IO.Path.GetFileName(test_img)}:");
                        Program.logIt(s);
                        Program.logIt(System.Environment.NewLine);
                    }
                }
            }
            Program.logIt($"is_iPhone_7Plus: -- {ret}");
            return ret;
        }
        static bool is_iPhone_6SPlus(string filename)
        {
            bool ret = false;
            Program.logIt($"is_iPhone_6SPlus: ++ {filename}");
            //foreach (string fn in System.IO.Directory.GetFiles(@"C:\Tools\avia\images\final_270\iphone6 Gold"))
            {
                string test_img = filename;
                Image<Gray, Byte> img = new Image<Gray, byte>(test_img);
                List<Dictionary<string, object>> data = null;
                try
                {
                    string s = System.IO.File.ReadAllText(@"images\template\iphone6s plus rosegold\info.json");
                    var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                    data = jss.Deserialize<List<Dictionary<string, object>>>(s);
                }
                catch (Exception) { }

                var area = data.Where(x => x.ContainsKey("name") && x["name"].ToString() == "model").First();
                Rectangle r = new Rectangle((int)area["x"], (int)area["y"], (int)area["width"], (int)area["height"]);
                Image<Gray, Byte> img1 = img.Copy(r).Rotate(90, new Gray(0), false);
                Regex re = new Regex(area["regex"].ToString(), RegexOptions.Singleline);
                //img1.Save("temp_1.jpg");

                CvInvoke.GaussianBlur(img1, img1, new Size(3, 3), 0);
                double v = CvInvoke.Threshold(img1, img1, 0, 255, ThresholdType.BinaryInv | ThresholdType.Otsu);
                //img1.Save("temp_1.jpg");
                Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
                img1 = img1.MorphologyEx(MorphOp.Erode, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
                //img1.Save("temp_2.jpg");
                using (TesseractEngine TE = new TesseractEngine("tessdata", "eng", EngineMode.TesseractOnly))
                {
                    //Bitmap b = new Bitmap(@"temp_text_3.jpg");
                    var p = TE.Process(img1.ToBitmap());
                    string s = p.GetText();
                    if (re.Match(s).Success)
                    {
                        //Program.logIt($"{System.IO.Path.GetFileName(test_img)}: is iPhone 6");
                        ret = true;
                    }
                    else
                    {
                        s = p.GetHOCRText(0);
                        Program.logIt($"{System.IO.Path.GetFileName(test_img)}:");
                        Program.logIt(s);
                        Program.logIt(System.Environment.NewLine);
                    }
                }
            }
            Program.logIt($"is_iPhone_6SPlus: -- {ret}");
            return ret;
        }
        static bool is_iPhone_6Plus(string filename)
        {
            bool ret = false;
            Program.logIt($"is_iPhone_6Plus: ++ {filename}");
            //foreach (string fn in System.IO.Directory.GetFiles(@"C:\Tools\avia\images\final_270\iphone6 Gold"))
            {
                string test_img = filename;
                Image<Gray, Byte> img = new Image<Gray, byte>(test_img);
                List<Dictionary<string, object>> data = null;
                try
                {
                    string s = System.IO.File.ReadAllText(@"images\template\iphone6 plus gold\info.json");
                    var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                    data = jss.Deserialize<List<Dictionary<string, object>>>(s);
                }
                catch (Exception) { }

                var area = data.Where(x => x.ContainsKey("name") && x["name"].ToString() == "model").First();
                Rectangle r = new Rectangle((int)area["x"], (int)area["y"], (int)area["width"], (int)area["height"]);
                Image<Gray, Byte> img1 = img.Copy(r).Rotate(90, new Gray(0), false);
                Regex re = new Regex(area["regex"].ToString(), RegexOptions.Singleline);
                //img1.Save("temp_1.jpg");

                CvInvoke.GaussianBlur(img1, img1, new Size(3, 3), 0);
                double v = CvInvoke.Threshold(img1, img1, 0, 255, ThresholdType.BinaryInv | ThresholdType.Otsu);
                //img1.Save("temp_1.jpg");
                Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
                img1 = img1.MorphologyEx(MorphOp.Erode, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
                //img1.Save("temp_2.jpg");
                using (TesseractEngine TE = new TesseractEngine("tessdata", "eng", EngineMode.TesseractOnly))
                {
                    //Bitmap b = new Bitmap(@"temp_text_3.jpg");
                    var p = TE.Process(img1.ToBitmap());
                    string s = p.GetText();
                    if (re.Match(s).Success)
                    {
                        //Program.logIt($"{System.IO.Path.GetFileName(test_img)}: is iPhone 6");
                        ret = true;
                    }
                    else
                    {
                        s = p.GetHOCRText(0);
                        Program.logIt($"{System.IO.Path.GetFileName(test_img)}:");
                        Program.logIt(s);
                        Program.logIt(System.Environment.NewLine);
                    }
                }
            }
            Program.logIt($"is_iPhone_6Plus: -- {ret}");
            return ret;
        }
        static bool is_iPhone_6S(string filename)
        {
            bool ret = false;
            Program.logIt($"is_iPhone_6S: ++ {filename}");
            //foreach (string fn in System.IO.Directory.GetFiles(@"C:\Tools\avia\images\final_270\iphone6 Gold"))
            {
                string test_img = filename;
                Image<Gray, Byte> img = new Image<Gray, byte>(test_img);
                List<Dictionary<string, object>> data = null;
                try
                {
                    string s = System.IO.File.ReadAllText(@"images\template\iphone6s gray\info.json");
                    var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                    data = jss.Deserialize<List<Dictionary<string, object>>>(s);
                }
                catch (Exception) { }

                var area = data.Where(x => x.ContainsKey("name") && x["name"].ToString() == "model").First();
                Rectangle r = new Rectangle((int)area["x"], (int)area["y"], (int)area["width"], (int)area["height"]);
                Image<Gray, Byte> img1 = img.Copy(r).Rotate(90, new Gray(0), false);
                Regex re = new Regex(area["regex"].ToString(), RegexOptions.Singleline);
                //img1.Save("temp_1.jpg");

                CvInvoke.GaussianBlur(img1, img1, new Size(3, 3), 0);
                double v = CvInvoke.Threshold(img1, img1, 0, 255, ThresholdType.BinaryInv | ThresholdType.Otsu);
                //img1.Save("temp_1.jpg");
                Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
                img1 = img1.MorphologyEx(MorphOp.Open, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
                //img1.Save("temp_2.jpg");
                using (TesseractEngine TE = new TesseractEngine("tessdata", "eng", EngineMode.TesseractOnly))
                {
                    //Bitmap b = new Bitmap(@"temp_text_3.jpg");
                    var p = TE.Process(img1.ToBitmap());
                    string s = p.GetText();
                    if (re.Match(s).Success)
                    {
                        //Program.logIt($"{System.IO.Path.GetFileName(test_img)}: is iPhone 6");
                        ret = true;
                    }
                    else
                    {
                        s = p.GetHOCRText(0);
                        Program.logIt($"{System.IO.Path.GetFileName(test_img)}:");
                        Program.logIt(s);
                        Program.logIt(System.Environment.NewLine);
                    }
                }
            }
            Program.logIt($"is_iPhone_6S: -- {ret}");
            return ret;
        }
        static bool is_iPhone_6(string filename)
        {
            bool ret = false;
            Program.logIt($"is_iPhone_6: ++ {filename}");
            //Regex re = new Regex(@"\b[A]?[-]?[1lI][S5]49\b|\bA1586\b|\bA1589\b", RegexOptions.Singleline);
            //string test_img = @"C:\Tools\avia\images\final_270\iphone6 Gray\2556.1.bmp";
            //foreach (string fn in System.IO.Directory.GetFiles(@"C:\Tools\avia\images\final_270\iphone6 Gold"))
            {
                string test_img = filename;
                Image<Gray, Byte> img = new Image<Gray, byte>(test_img);
                List<Dictionary<string, object>> data = null;
                try
                {
                    string s = System.IO.File.ReadAllText(@"images\template\iphone6 gold\info.json");
                    var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                    data = jss.Deserialize<List<Dictionary<string, object>>>(s);
                }
                catch (Exception) { }

                var area = data.Where(x => x.ContainsKey("name") && x["name"].ToString() == "model").First();
                Rectangle r = new Rectangle((int)area["x"], (int)area["y"], (int)area["width"], (int)area["height"]);
                Image<Gray, Byte> img1 = img.Copy(r).Rotate(90, new Gray(0), false);
                Regex re = new Regex(area["regex"].ToString(), RegexOptions.Singleline);
                //img1.Save("temp_1.jpg");

                CvInvoke.GaussianBlur(img1, img1, new Size(3, 3), 0);
                double v = CvInvoke.Threshold(img1, img1, 0, 255, ThresholdType.BinaryInv | ThresholdType.Otsu);
                //img1.Save("temp_1.jpg");
                Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
                img1 = img1.MorphologyEx(MorphOp.Open, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
                //img1 = img1.MorphologyEx(MorphOp.Erode, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
                //img1.Save("temp_2.jpg");
                using (TesseractEngine TE = new TesseractEngine("tessdata", "eng", EngineMode.TesseractOnly))
                {
                    //Bitmap b = new Bitmap(@"temp_text_3.jpg");
                    var p = TE.Process(img1.ToBitmap());
                    string s = p.GetText();
                    if (re.Match(s).Success)
                    {
                        //Program.logIt($"{System.IO.Path.GetFileName(test_img)}: is iPhone 6");
                        ret = true;
                    }
                    else
                    {
                        s = p.GetHOCRText(0);
                        Program.logIt($"{System.IO.Path.GetFileName(test_img)}:");
                        Program.logIt(s);
                        Program.logIt(System.Environment.NewLine);
                    }
                }
            }
            Program.logIt($"is_iPhone_6: -- {ret}");
            return ret;
        }
        static bool is_iPhone_6_gold(string filename, double threshold = 0.60)
        {
            bool ret = false;
            double score = 0.0;
            string root = @"images\template\iphone6 gold";
            Program.logIt($"is_iPhone_6_gold: ++ {filename}, threshold={threshold}");
            {
                string test_img = filename;
                Image<Gray, Byte> img = new Image<Gray, byte>(test_img);
                List<Dictionary<string, object>> data = null;
                try
                {
                    string s = System.IO.File.ReadAllText(System.IO.Path.Combine(root,@"info.json"));
                    var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                    data = jss.Deserialize<List<Dictionary<string, object>>>(s);
                }
                catch (Exception) { }
                var area_data = data.Where(x => x.ContainsKey("type") && x["type"].ToString() == "match");
                List<double> scores = new List<double>();
                foreach (var a in area_data)
                {
                    Rectangle r = new Rectangle((int)a["x"], (int)a["y"], (int)a["width"], (int)a["height"]);
                    Mat m = CvInvoke.Imread(System.IO.Path.Combine(root, $"temp_{a["id"]}.jpg"), ImreadModes.Grayscale);
                    //a.Add("image", m);
                    Image<Gray, Byte> img_t = img.Copy(r);
                    Image<Gray, float> mm = img_t.MatchTemplate(m.ToImage<Gray, Byte>(), TemplateMatchingType.CcoeffNormed);
                    double[] minValues, maxValues;
                    Point[] minLocations, maxLocations;
                    mm.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
                    Program.logIt($"id: {a["name"]}, match: {maxValues[0]}");
                    scores.Add(maxValues[0]);
                }
                score = scores.Average();
                if (score > threshold)
                    ret = true;
            }
            Program.logIt($"is_iPhone_6_gold: -- {ret} score={score}");
            return ret;
        }
    }
}
