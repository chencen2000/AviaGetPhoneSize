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
    class myContour
    {
        public Point []points;
        public int index;
        public double area;
        public int next;
        public int previous;
        public int child;
        public int parent;
        public myContour(VectorOfPoint pnts, int index, double area, int next, int previous, int child, int parent)
        {
            points = pnts.ToArray();
            this.index = index;
            this.area = area;
            this.next = next;
            this.previous = previous;
            this.child = child;
            this.parent = parent;
        }
    }

    class testOpenCV
    {
        static int Main(string[] args)
        {
            //resize_image();
            //test();
            //test_2();
            //test_3();
            //test_4();
            is_apple_device();
            return 0;
        }
        static void test()
        {
            //string fn = @"C:\Tools\avia\images\Final270\iphone6 Gold\0123.1.bmp";
            //string fn = @"C:\Tools\avia\images\test.1\iphoneX SpaceGray_img\1873.1.jpg";
            string fn = @"C:\Tools\avia\images\test.1\iphone6 Plus Gold\1473.1.jpg";
            //string fn = @"temp_1.jpg";
            Mat m = CvInvoke.Imread(fn);
            Image<Gray, Byte> img = m.ToImage<Gray, Byte>(); //.GetSubRect(new Rectangle(new Point(5, 5), new Size(m.Width - 10, m.Height - 10))).Resize(0.1, Inter.Cubic);
            img = img.Rotate(90, new Gray(0), false);
            //img=img.Dilate(7);
            img = img.Erode(3);
            img = img.Dilate(5);
            //img = img.Erode(5);
            img.Save("temp_1.jpg");
            CvInvoke.GaussianBlur(img, img, new Size(3, 3), 0);
            double otsu = CvInvoke.Threshold(img, new Mat(), 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
            double sigma = 0.25;
            double lower = Math.Max(1, (1.0 - sigma) * otsu);
            double upper = Math.Min(255, (1.0 + sigma) * otsu);
            CvInvoke.Canny(img, img, lower, upper);
            img.Save("temp_1.jpg");

            //VectorOfPoint apple_logo = test_3();
            Tuple<VectorOfPoint, VectorOfPoint> apple_logo = get_apple_logo();
            //VectorOfPoint apple_iphone = test_4();
            Mat n = new Mat(m.Rows, m.Cols, DepthType.Cv8U, 1);
            VectorOfVectorOfPoint vvp = new VectorOfVectorOfPoint();
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(img, contours, null, RetrType.List, ChainApproxMethod.ChainApproxNone);
                int count = contours.Size;
                for(int i=0; i<count; i++)
                {
                    double a = CvInvoke.ContourArea(contours[i]);
                    if(a> 1000.0)
                    {
                        double d1 = CvInvoke.MatchShapes(apple_logo.Item1, contours[i], ContoursMatchType.I1);
                        double d2 = CvInvoke.MatchShapes(apple_logo.Item2, contours[i], ContoursMatchType.I1);
                        if (d1<0.05 || d2<0.05)
                        {
                            vvp.Push(contours[i]);
                        }
                    }
                    //if (a > 100.0)
                    //{
                    //    double d1 = CvInvoke.MatchShapes(apple_iphone, contours[i], ContoursMatchType.I1);
                    //    if (d1 < 0.1)
                    //    {
                    //        vvp.Push(contours[i]);
                    //    }
                    //}
                }
            }
            CvInvoke.DrawContours(n, vvp, -1, new MCvScalar(255));
            n.Save("temp_2.jpg");
            /*
            CvInvoke.GaussianBlur(img, img, new Size(3, 3), 0);
            Mat dx = new Mat();
            Mat dy = new Mat();
            CvInvoke.Sobel(img, dx, DepthType.Cv16S, 1, 0);
            CvInvoke.Sobel(img, dy, DepthType.Cv16S, 0, 1);
            CvInvoke.ConvertScaleAbs(dx, dx, 1, 0);
            CvInvoke.ConvertScaleAbs(dy, dy, 1, 0);
            CvInvoke.AddWeighted(dx, 0.5, dy, 0.5, 0, img);
            //dx.Save("temp_x.jpg");
            //dy.Save("temp_y.jpg");
            img.Save("temp_2.jpg");
            */
        }
        static void test_2()
        {
            Mat m = CvInvoke.Imread("temp_2.jpg");
            Image<Gray, byte> img = m.ToImage<Gray, byte>();
            List<myContour> myc = new List<myContour>();
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                Mat info = new Mat();
                CvInvoke.FindContours(img, contours, info, RetrType.Tree, ChainApproxMethod.ChainApproxNone);
                Matrix<Int32> infox = new Matrix<Int32>(info.Rows, info.Cols, info.NumberOfChannels);
                info.CopyTo(infox);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    VectorOfPoint contour = contours[i];
                    double a = CvInvoke.ContourArea(contour);
                    int next = infox.Data[0, i * 4+0];
                    int previous = infox.Data[0, i * 4+1];
                    int child = infox.Data[0, i * 4+2];
                    int parent = infox.Data[0, i * 4+3];
                    myContour c = new myContour(contour, i, a, next, previous, child, parent);
                    myc.Add(c);
                }
            }
            List<myContour> outer = new List<myContour>();
            foreach (myContour c in myc)
            {
                if(c.area>1000.0 && c.child!=-1 && c.parent == -1)
                {
                    outer.Add(c);
                }
            }
            //VectorOfVectorOfPoint vvp = new VectorOfVectorOfPoint();
            /*
            VectorOfPoint match_t = test_3();
            foreach (myContour c in outer)
            {
                //using (VectorOfVectorOfPoint vvp = new VectorOfVectorOfPoint())
                //{
                //    VectorOfPoint vp = new VectorOfPoint(c.points);
                //    vvp.Push(vp);
                //    CvInvoke.DrawContours(m, vvp, -1, new MCvScalar(0, 255, 0));
                //    m.Save("temp_3.jpg");
                //}
                //CvInvoke.DrawContours(m, vp, 0, new MCvScalar(0, 255, 0));
                double d = CvInvoke.MatchShapes(match_t, new VectorOfPoint(c.points), ContoursMatchType.I1);
            }
            */
            m.Save("temp_3.jpg");
        }
        static Dictionary<int,object> find_contour(Dictionary<int,object> data, int id)
        {
            Dictionary<int, object> ret = null;
            foreach(KeyValuePair<int,object> kvp in data)
            {
                if (kvp.Key == id)
                {
                    ret = data;
                    break;
                }
                else
                {
                    if (kvp.Value != null && kvp.Value.GetType() == typeof(Dictionary<int, object>))
                    {
                        ret = find_contour((Dictionary<int, object>)kvp.Value, id);
                    }
                }
            }
            return ret;
        }
        static Tuple<VectorOfPoint, VectorOfPoint> get_apple_logo()
        {
            Mat m = CvInvoke.Imread(@"images\Apple-Logo-black-880x660.png");
            Image<Gray, Byte> img = m.ToImage<Gray, Byte>();
            //img = img.Erode(5);
            //img = img.Dilate(2);
            CvInvoke.GaussianBlur(img, img, new Size(5, 5), 0);
            CvInvoke.Threshold(img, img, 0, 255, ThresholdType.BinaryInv | ThresholdType.Otsu);
            VectorOfVectorOfPoint vvp1 = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(img, vvp1, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            int cnt = vvp1.Size;
            //CvInvoke.DrawContours(m, vvp1, -1, new MCvScalar(0, 255, 0), 2);
            //m.Save("temp_3.jpg");
            //VectorOfVectorOfPoint vvp0 = new VectorOfVectorOfPoint();
            VectorOfPoint all_vp = new VectorOfPoint();
            VectorOfPoint main_vp = new VectorOfPoint();
            double ma = 0;
            int id = -1;
            for (int i=0; i<cnt; i++)
            {
                double a = CvInvoke.ContourArea(vvp1[i]);
                if (a > ma)
                {
                    ma = a;
                    id = i;
                }
                VectorOfPoint vp = vvp1[i];
                all_vp.Push(vp);
            }
            main_vp.Push(vvp1[id]);
            //return vvp1[1];
            return new Tuple<VectorOfPoint, VectorOfPoint>(all_vp, main_vp);
        }
        static VectorOfPoint test_4()
        {
            //Rectangle r = found_apple_logo(@"C:\Tools\avia\images\test.1\iphone6 Plus Gold\1473.1.jpg");
            //Rectangle r = found_apple_logo(@"C:\Tools\avia\images\test.1\iphone6 Gold\0123.1.jpg");

            string folder = @"C:\Tools\avia\images\test.1";
            foreach (string fn in System.IO.Directory.GetFiles(folder, "*.jpg", System.IO.SearchOption.AllDirectories))
            {
                Rectangle r = found_apple_logo(fn);
                if (r.IsEmpty)
                {
                    // fail to found apple logo
                    Program.logIt($"!!! Fail to found apple logo. {fn}");
                }
                else
                {
                    Program.logIt($"Found {r} in {fn}");
                }
            }


            return null;
        }
        static void resize_image()
        {
            string folder = @"C:\Tools\avia\images\test.1";
            foreach(string fn in System.IO.Directory.GetFiles(folder, "*.bmp", System.IO.SearchOption.AllDirectories))
            {
                Mat m = CvInvoke.Imread(fn);
                Mat n = new Mat();
                CvInvoke.Resize(m, n, new Size(0, 0), 0.1, 0.1);
                n.Save(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(fn), System.IO.Path.ChangeExtension(System.IO.Path.GetFileName(fn), ".jpg")));
            }
        }
        static Rectangle found_apple_logo(string filename)
        {
            Rectangle ret = Rectangle.Empty;
            Tuple<int, int, double>[] param = new Tuple<int, int, double>[]
                {
                                new Tuple<int, int, double>(0,0,0.02),
                                new Tuple<int, int, double>(3,5,0.05),
                                new Tuple<int, int, double>(7,7,0.05),
                };
            foreach (Tuple<int, int, double> p in param)
            {
                using (Mat m = CvInvoke.Imread(filename))
                {
                    double score = 1.0;
                    int most_match = -1;
                    Image<Gray, Byte> img = m.ToImage<Gray, Byte>();
                    img = img.Rotate(90, new Gray(0), false);
                    if (p.Item1 > 0)
                        img = img.Erode(p.Item1);
                    if (p.Item2 > 0)
                        img = img.Dilate(p.Item1);
                    CvInvoke.GaussianBlur(img, img, new Size(3, 3), 0);
                    double otsu = CvInvoke.Threshold(img, new Mat(), 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
                    double sigma = 0.25;
                    double lower = Math.Max(1, (1.0 - sigma) * otsu);
                    double upper = Math.Min(255, (1.0 + sigma) * otsu);
                    CvInvoke.Canny(img, img, lower, upper);
                    Tuple<VectorOfPoint, VectorOfPoint> apple_logo = get_apple_logo();
                    using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                    {
                        CvInvoke.FindContours(img, contours, null, RetrType.List, ChainApproxMethod.ChainApproxNone);
                        int count = contours.Size;
                        for (int i = 0; i < count; i++)
                        {
                            double a = CvInvoke.ContourArea(contours[i]);
                            if (a > 1000.0)
                            {
                                double d1 = CvInvoke.MatchShapes(apple_logo.Item1, contours[i], ContoursMatchType.I1);
                                double d2 = CvInvoke.MatchShapes(apple_logo.Item2, contours[i], ContoursMatchType.I1);
                                if (d1 < p.Item3 || d2 < p.Item3)
                                {
                                    if (d1 < score)
                                    {
                                        score = d1;
                                        most_match = i;
                                    }
                                    if (d2 < score)
                                    {
                                        score = d2;
                                        most_match = i;
                                    }
                                }
                            }
                        }
                        if(most_match>=0 && most_match < count)
                        {
                            ret = CvInvoke.BoundingRectangle(contours[most_match]);
                        }
                    }
                    if (!ret.IsEmpty)
                        break;
                }
            }
            return ret;
        }
        static void is_apple_device()
        {
            Rectangle ret = Rectangle.Empty;
            Tuple<int, int, double>[] param = new Tuple<int, int, double>[]
                {
                    new Tuple<int, int, double>(5,5,0.05),
                                //new Tuple<int, int, double>(0,0,0.02),
                                //new Tuple<int, int, double>(3,5,0.05),
                                //new Tuple<int, int, double>(7,7,0.05),
                };
            string filename = @"C:\Tools\avia\images\test.1\iphone6 Plus Gold\1473.1.jpg";
            foreach (Tuple<int, int, double> p in param)
            {
                using (Mat m = CvInvoke.Imread(filename))
                {
                    CvInvoke.Rotate(m, m, RotateFlags.Rotate90Clockwise);
                    double score = 1.0;
                    int most_match = -1;
                    Image<Gray, Byte> img = m.ToImage<Gray, Byte>();
                    //img = img.Rotate(90, new Gray(0), false);
                    if (p.Item1 > 0)
                        img = img.Erode(p.Item1);
                    if (p.Item2 > 0)
                        img = img.Dilate(p.Item1);
                    CvInvoke.GaussianBlur(img, img, new Size(3, 3), 0);
                    double otsu = CvInvoke.Threshold(img, new Mat(), 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
                    double sigma = 0.25;
                    double lower = Math.Max(1, (1.0 - sigma) * otsu);
                    double upper = Math.Min(255, (1.0 + sigma) * otsu);
                    CvInvoke.Canny(img, img, lower, upper);
                    img.Save("temp_1.jpg");
                    Tuple<VectorOfPoint, VectorOfPoint> apple_logo = get_apple_logo();
                    using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                    {
                        CvInvoke.FindContours(img, contours, null, RetrType.List, ChainApproxMethod.ChainApproxNone);
                        int count = contours.Size;
                        for (int i = 0; i < count; i++)
                        {
                            double a = CvInvoke.ContourArea(contours[i]);
                            if (a > 1000.0)
                            {
                                double d1 = CvInvoke.MatchShapes(apple_logo.Item1, contours[i], ContoursMatchType.I1);
                                double d2 = CvInvoke.MatchShapes(apple_logo.Item2, contours[i], ContoursMatchType.I1);
                                if (d1 < p.Item3 || d2 < p.Item3)
                                {
                                    if (d1 < score)
                                    {
                                        score = d1;
                                        most_match = i;
                                    }
                                    if (d2 < score)
                                    {
                                        score = d2;
                                        most_match = i;
                                    }
                                }
                                CvInvoke.DrawContours(m, contours, i, new MCvScalar(0, 255, 0));
                            }
                        }
                        if (most_match >= 0 && most_match < count)
                        {
                            ret = CvInvoke.BoundingRectangle(contours[most_match]);
                        }
                    }
                    m.Save("temp_2.jpg");
                    if (!ret.IsEmpty)
                        break;
                }
            }
            Program.logIt($"{ret}");
            //return ret;
        }
    }
}
