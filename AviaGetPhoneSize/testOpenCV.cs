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
            //test();
            //test_2();
            //test_3();
            test_4();
            return 0;
        }
        static void test()
        {
            //string fn = @"C:\Tools\avia\images\Final270\iphone6 Gold\0123.1.bmp";
            string fn = @"C:\Tools\avia\Recog source\AP001-iphone6_gold\0123.1.bmp";
            //string fn = @"temp_1.jpg";
            Mat m = CvInvoke.Imread(fn);
            Image<Gray, Byte> img = m.ToImage<Gray, Byte>().GetSubRect(new Rectangle(new Point(5, 5), new Size(m.Width - 10, m.Height - 10))).Resize(0.1, Inter.Cubic);


            CvInvoke.GaussianBlur(img, img, new Size(3, 3), 0);
            double otsu = CvInvoke.Threshold(img, new Mat(), 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
            double sigma = 0.25;
            double lower = Math.Max(1, (1.0 - sigma) * otsu);
            double upper = Math.Min(255, (1.0 + sigma) * otsu);
            CvInvoke.Canny(img, img, lower, upper);
            img.Save("temp_1.jpg");

            /*
            CvInvoke.Laplacian(img, img, DepthType.Cv16S, 3, 1, 0);
            CvInvoke.ConvertScaleAbs(img, img, 1, 0);
            img.Save("temp_1.bmp");
            */
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
        static VectorOfPoint test_3()
        {
            Mat m = CvInvoke.Imread(@"C:\Tools\avia\images\Apple_logo_1.png");
            CvInvoke.GaussianBlur(m, m, new Size(5, 5), 0);
            Image<Gray, Byte> img = m.ToImage<Gray, Byte>();
            CvInvoke.Threshold(img, img, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
            VectorOfVectorOfPoint vvp1 = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(img, vvp1, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            int cnt = vvp1.Size;
            CvInvoke.DrawContours(m, vvp1, -1, new MCvScalar(0, 255, 0), 2);
            m.Save("temp_3.jpg");
            //VectorOfVectorOfPoint vvp0 = new VectorOfVectorOfPoint();
            VectorOfPoint all_vp = new VectorOfPoint();
            for (int i=0; i<cnt; i++)
            {
                VectorOfPoint vp = vvp1[i];
                all_vp.Push(vp);
            }
            //return vvp1[1];
            return all_vp;
        }
        static void test_4()
        {
            Mat m = CvInvoke.Imread("temp_1.jpg");
            Image<Gray, byte> img = m.ToImage<Gray, byte>();
            List<myContour> myc = new List<myContour>();
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                VectorOfPoint t = test_3();
                CvInvoke.FindContours(img, contours, null, RetrType.List, ChainApproxMethod.ChainApproxNone);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    VectorOfPoint contour = contours[i];
                    double a = CvInvoke.ContourArea(contour);
                    if (a > 100.0)
                    {
                        double d = CvInvoke.MatchShapes(t, contour, ContoursMatchType.I1);
                        if (d < 0.05)
                        {
                            Program.logIt($"match: {d}");
                        }
                    }
                }
            }

        }
    }
}
