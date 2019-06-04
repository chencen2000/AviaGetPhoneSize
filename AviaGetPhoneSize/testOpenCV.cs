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
    class testOpenCV
    {
        static int Main(string[] args)
        {
            test_2();
            return 0;
        }
        static void test()
        {
            //string fn = @"C:\Tools\avia\images\Final270\iphone6 Gold\0123.1.bmp";
            string fn = @"C:\Tools\avia\images\Final270\iphone6s Plus Silver\1471.1.bmp";
            //string fn = @"temp_1.jpg";
            Mat m = CvInvoke.Imread(fn);
            Image<Gray, Byte> img = m.ToImage<Gray, Byte>().GetSubRect(new Rectangle(new Point(5, 5), new Size(m.Width - 10, m.Height - 10))).Resize(0.1, Inter.Cubic);


            CvInvoke.GaussianBlur(img, img, new Size(5, 5), 0);
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
            List<Tuple<int, VectorOfPoint, int, int, int, int>> data = new List<Tuple<int, VectorOfPoint, int, int, int, int>>();
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
                    Tuple<int, VectorOfPoint, int, int, int, int> r = new Tuple<int, VectorOfPoint, int, int, int, int>(i, contour, next, previous, child, parent);
                    if(parent!=-1 && parent >= i)
                    {

                    }
                    data.Add(r);
                }
            }
            List<Tuple<int, VectorOfPoint, int, int, int, int>> outer = new List<Tuple<int, VectorOfPoint, int, int, int, int>>();
            foreach (var v in data)
            {
                if(v.Item6==-1 && v.Item5 != -1)
                {
                }
            }
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
    }
}
