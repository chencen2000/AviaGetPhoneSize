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
using Tesseract;
using System.IO.Ports;
using System.Text.RegularExpressions;
using Emgu.CV.ML;
using System.Net.Sockets;
using System.Net;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Collections;
using System.Xml.Serialization;
using System.IO;

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
    public class color_item
    {
        public string description;
        public int id;
        public int red;
        public int green;
        public int blue;
    }
    public class colors
    {
        [XmlElement("color")]
        public color_item[] color_items;
    }
    class testOpenCV
    {
        [STAThread]
        static int Main(string[] args)
        {
            //resize_image();
            //test();
            //test_1();
            //train_iphone_color_data();
            //train_iphone_size_data_v2();
            //test_ML();
            //test_3();
            //test_4();
            //is_apple_device();
            //prepare_image();
            //found_text();
            //Tuple<Mat, Mat> r = prepare_image(@"C:\Tools\avia\images\test.1\iphone6 Gold\0123.1.bmp");
            //r.Item1.Save("temp_1.jpg");
            //r.Item2.Save("temp_2.jpg");
            //test_ocr();
            //test_5();
            //test_ss();
            //test_6();
            //test_7();
            //test_8();
            test_9();
            //toXml();
            //test_form();
            return 0;
        }
        static void test()
        {
            Rectangle r0 = new Rectangle(744, 266, 576, 1116);
            string fn0 = @"C:\Tools\avia\images\temp\background.jpg";
            string fn1 = @"C:\Tools\avia\images\temp\iphone_8p_black.jpg";
            string[] tf = new string[]
            {
                //@"C:\Tools\avia\images\temp\iphone_8p_black.jpg",
                //@"C:\Tools\avia\images\temp\iphone_8p_gray.jpg",
                //@"C:\Tools\avia\images\temp\iphone_8p_red.jpg",
                //@"C:\Tools\avia\images\temp\iphone_xr_blue.jpg",
                @"C:\Tools\avia\images\temp\iphone_6_rosegold.jpg",
            };
            foreach (string fn in tf)
            {
                string tn = System.IO.Path.GetFileNameWithoutExtension(fn);
                Program.logIt($"preocess: {tn}");
                Mat m0 = CvInvoke.Imread(fn0);
                Mat m1 = CvInvoke.Imread(fn);

                CvInvoke.Rotate(m0, m0, RotateFlags.Rotate90CounterClockwise);
                CvInvoke.Rotate(m1, m1, RotateFlags.Rotate90CounterClockwise);

                Image<Bgr, Byte> img0 = m0.ToImage<Bgr, Byte>().Copy(r0);
                Image<Bgr, Byte> img1 = m1.ToImage<Bgr, Byte>().Copy(r0);

                img0 = img1.AbsDiff(img0);
                img1.Save($"temp_{tn}_1.jpg");
                img0.Save($"temp_{tn}_2.jpg");
                Image<Gray, Byte> img = img0.Mat.ToImage<Gray, Byte>();
                Gray gv = img.GetAverage();
                CvInvoke.Threshold(img, img, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
                img._Erode(2);
                Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
                img._MorphologyEx(MorphOp.Gradient, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
                img.Save($"temp_{tn}_3.jpg");

                Rectangle roi = Rectangle.Empty;
                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(img, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
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
                Size sz = new Size(roi.X + roi.Width, roi.Y + roi.Height);

                Rectangle rc = new Rectangle(20, 810, 290, 30);
                Image<Bgr, Byte> imgc = img1.Copy(rc);
                Bgr bgr = imgc.GetAverage();

                Program.logIt($"{tn}: size={sz} r={bgr.Red} g={bgr.Green} b={bgr.Blue}");
            }
        }
        static void test_1()
        {
            string fn0 = @"C:\Tools\avia\images\temp\position_finish_bg.jpg";
            string fn1 = @"C:\Tools\avia\images\temp\position_finish_w_phone.jpg";
            Rectangle r0 = new Rectangle(744, 266, 576, 1116);

            Mat m0 = CvInvoke.Imread(fn0);
            Mat m1 = CvInvoke.Imread(fn1);

            CvInvoke.Rotate(m0, m0, RotateFlags.Rotate90CounterClockwise);
            CvInvoke.Rotate(m1, m1, RotateFlags.Rotate90CounterClockwise);

            Image<Bgr, Byte> img0 = m0.ToImage<Bgr, Byte>().Copy(r0);
            Image<Bgr, Byte> img1 = m1.ToImage<Bgr, Byte>().Copy(r0);

            img0.Save("temp_1.jpg");
            img1.Save("temp_2.jpg");

            Image<Gray, Byte> mask = img0.InRange(new Bgr(38, 58, 39), new Bgr(90, 120, 70));
            //mask.Save("temp_2.jpg");
            int[] area = mask.CountNonzero();
            Program.logIt($"{(double)area[0] / (mask.Width * mask.Height):P}");

            mask = img1.InRange(new Bgr(38, 58, 39), new Bgr(90, 120, 70));
            area = mask.CountNonzero();
            Program.logIt($"{(double)area[0] / (mask.Width * mask.Height):P}");
        }
        static void test_2()
        {
            string fn = @"../../test/iphone_color.txt";
            Regex re = new Regex(@"^.+color=\[([\d\.]*),([\d\.]*),([\d\.]*)\], label=(\d+).*$");
            List<Tuple<double, double, double, int>> datas = new List<Tuple<double, double, double, int>>();
            foreach (string l in System.IO.File.ReadAllLines(fn))
            {
                if (!string.IsNullOrEmpty(l))
                {
                    Match m = re.Match(l);
                    if (m.Success)
                    {
                        double b = Double.Parse(m.Groups[1].Value);
                        double g = Double.Parse(m.Groups[2].Value);
                        double r = Double.Parse(m.Groups[3].Value);
                        int label = Int32.Parse(m.Groups[4].Value);
                        Tuple<double, double, double, int> d = new Tuple<double, double, double, int>(r, g, b, label);
                        datas.Add(d);
                    }
                }
            }
            foreach (var v in datas)
            {
                Program.logIt($"R={v.Item1}, G={v.Item2}, B={v.Item3}, label={v.Item4}");
            }

            int trainSampleCount = datas.Count;
            Matrix<float> trainData = new Matrix<float>(trainSampleCount, 3);
            Matrix<int> trainClasses = new Matrix<int>(trainSampleCount, 1);
            for (int i = 0; i < datas.Count; i++)
            {
                trainData[i, 0] = (float)datas[i].Item1;
                trainData[i, 1] = (float)datas[i].Item2;
                trainData[i, 2] = (float)datas[i].Item3;
                trainClasses[i, 0] = datas[i].Item4;
            }
            TrainData td = new TrainData(trainData, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, trainClasses);
            NormalBayesClassifier classifier = new NormalBayesClassifier();
            bool trained = classifier.Train(td);

            if (trained)
            {
                classifier.Save("iPhone_color.xml");

                Matrix<float> test = new Matrix<float>(1, 3);
                for (int i = 0; i < datas.Count; i++)
                {
                    test[0, 0] = (float)datas[i].Item1;
                    test[0, 1] = (float)datas[i].Item2;
                    test[0, 2] = (float)datas[i].Item3;
                    int l = (int)classifier.Predict(test);
                    if (l != datas[i].Item4)
                    {
                        Program.logIt($"predict: {l} vs {datas[i].Item4}");
                    }
                }

                // test
                test[0, 0] = 8.96270396270396f;
                test[0, 1] = 6.96270396270396f;
                test[0, 2] = 6.96270396270396f;
                int p = (int)classifier.Predict(test);
                Program.logIt($"predict: {p} ");
            }
        }
        static void train_iphone_color_data()
        {
            //string fn = @"../../../test/M4_testing_data.txt";
            string fn = @"C:\Tools\avia\images\avia_m0_pc\train_data.txt";
            Regex re = new Regex(@"^.+color=\[([\d\.]*),([\d\.]*),([\d\.]*)\], clabel=(\d+).*$");
            List<Tuple<double, double, double, int>> datas = new List<Tuple<double, double, double, int>>();
            foreach (string l in System.IO.File.ReadAllLines(fn))
            {
                if (!string.IsNullOrEmpty(l))
                {
                    if (l.StartsWith("#"))
                    {
                        // comment line
                    }
                    else
                    {
                        Match m = re.Match(l);
                        if (m.Success)
                        {
                            double b = Double.Parse(m.Groups[1].Value);
                            double g = Double.Parse(m.Groups[2].Value);
                            double r = Double.Parse(m.Groups[3].Value);
                            int label = Int32.Parse(m.Groups[4].Value);
                            Tuple<double, double, double, int> d = new Tuple<double, double, double, int>(r, g, b, label);
                            datas.Add(d);
                        }
                    }
                }
            }
            foreach (var v in datas)
            {
                Program.logIt($"R={v.Item1}, G={v.Item2}, B={v.Item3}, label={v.Item4}");
            }

            int trainSampleCount = datas.Count;
            Matrix<float> trainData = new Matrix<float>(trainSampleCount, 3);
            Matrix<int> trainClasses = new Matrix<int>(trainSampleCount, 1);
            for (int i = 0; i < datas.Count; i++)
            {
                trainData[i, 0] = (float)datas[i].Item1;
                trainData[i, 1] = (float)datas[i].Item2;
                trainData[i, 2] = (float)datas[i].Item3;
                trainClasses[i, 0] = datas[i].Item4;
            }
            TrainData td = new TrainData(trainData, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, trainClasses);
            using (SVM model = new SVM())
            {
                if (System.IO.File.Exists("iPhone_color.xml"))
                {
                    model.Load("iPhone_color.xml");

                    Matrix<float> test = new Matrix<float>(1, 3);
                    for (int i = 0; i < datas.Count; i++)
                    {
                        test[0, 0] = (float)datas[i].Item1;
                        test[0, 1] = (float)datas[i].Item2;
                        test[0, 2] = (float)datas[i].Item3;
                        int l = (int)model.Predict(test);
                        if (l != datas[i].Item4)
                        {
                            Program.logIt($"predict: {l} vs {datas[i].Item4}");
                        }
                    }

                    // test
                    test[0, 0] = 8.96270396270396f;
                    test[0, 1] = 6.96270396270396f;
                    test[0, 2] = 6.96270396270396f;
                    int p = (int)model.Predict(test);
                    Program.logIt($"predict: {p} ");
                }
                else
                {
                    model.TermCriteria = new MCvTermCriteria(100, 0.00001);
                    model.C = 1;
                    model.Type = SVM.SvmType.CSvc;
                    model.SetKernel(SVM.SvmKernelType.Linear);
                    //bool trained = model.TrainAuto(trainData, trainClasses, null, null, p.MCvSVMParams, 5);
                    bool trained = model.Train(td);

                    model.Save("iPhone_color.xml");

                    Matrix<float> test = new Matrix<float>(1, 3);
                    for (int i = 0; i < datas.Count; i++)
                    {
                        test[0, 0] = (float)datas[i].Item1;
                        test[0, 1] = (float)datas[i].Item2;
                        test[0, 2] = (float)datas[i].Item3;
                        int l = (int)model.Predict(test);
                        if (l != datas[i].Item4)
                        {
                            Program.logIt($"predict: {l} vs {datas[i].Item4}");
                        }
                    }

                    // test
                    test[0, 0] = 8.96270396270396f;
                    test[0, 1] = 6.96270396270396f;
                    test[0, 2] = 6.96270396270396f;
                    int p = (int)model.Predict(test);
                    Program.logIt($"predict: {p} ");
                }
            }
            /*
            NormalBayesClassifier classifier = new NormalBayesClassifier();
            bool trained = classifier.Train(td);

            if (trained)
            {
                classifier.Save("iPhone_color.xml");

                Matrix<float> test = new Matrix<float>(1, 3);
                for (int i = 0; i < datas.Count; i++)
                {
                    test[0, 0] = (float)datas[i].Item1;
                    test[0, 1] = (float)datas[i].Item2;
                    test[0, 2] = (float)datas[i].Item3;
                    int l = (int)classifier.Predict(test);
                    if (l != datas[i].Item4)
                    {
                        Program.logIt($"predict: {l} vs {datas[i].Item4}");
                    }
                }

                // test
                test[0, 0] = 8.96270396270396f;
                test[0, 1] = 6.96270396270396f;
                test[0, 2] = 6.96270396270396f;
                int p = (int)classifier.Predict(test);
                Program.logIt($"predict: {p} ");
            }
            */
        }
        static void train_iphone_size_data_v2()
        {
            //string fn = @"../../../test/M4_testing_data.txt";
            string fn = @"C:\Tools\avia\images\avia_m0_pc\train_data.txt";
            Regex re = new Regex(@"^.+size=([\d\.]*),.+slabel=(\d+).*$");
            List<Tuple<double, int>> datas = new List<Tuple<double, int>>();
            foreach (string l in System.IO.File.ReadAllLines(fn))
            {
                if (!string.IsNullOrEmpty(l))
                {
                    if (l.StartsWith("#"))
                    {
                        // comment line
                    }
                    else
                    {
                        Match m = re.Match(l);
                        if (m.Success)
                        {
                            double r = Double.Parse(m.Groups[1].Value);
                            //double h = Double.Parse(m.Groups[2].Value);
                            int label = Int32.Parse(m.Groups[2].Value);
                            Tuple<double, int> d = new Tuple<double, int>(r, label);
                            datas.Add(d);
                        }
                    }
                }
            }
            foreach (var v in datas)
            {
                Program.logIt($"r={v.Item1}, label={v.Item2}");
            }

            int trainSampleCount = datas.Count;
            Matrix<float> trainData = new Matrix<float>(trainSampleCount, 1);
            Matrix<int> trainClasses = new Matrix<int>(trainSampleCount, 1);
            for (int i = 0; i < datas.Count; i++)
            {
                trainData[i, 0] = (float)datas[i].Item1;
                //trainData[i, 1] = (float)datas[i].Item2;
                trainClasses[i, 0] = datas[i].Item2;
            }
            TrainData td = new TrainData(trainData, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, trainClasses);
            using (SVM model = new SVM())
            {
                if (System.IO.File.Exists("iPhone_size.xml"))
                {
                    model.Load("iPhone_size.xml");

                    Matrix<float> test = new Matrix<float>(1, 1);
                    for (int i = 0; i < datas.Count; i++)
                    {
                        test[0, 0] = (float)datas[i].Item1;
                        //test[0, 1] = (float)datas[i].Item2;
                        int l = (int)model.Predict(test);
                        if (l != datas[i].Item2)
                        {
                            Program.logIt($"predict: {l} vs {datas[i].Item2}");
                        }
                    }

                    // test
                    //test[0, 0] = 515f;
                    //test[0, 1] = 1032f;
                    //int p = (int)model.Predict(test);
                    //Program.logIt($"predict: {p} ");
                }
                else
                {
                    model.TermCriteria = new MCvTermCriteria(100, 0.00001);
                    model.C = 1;
                    model.Type = SVM.SvmType.CSvc;
                    model.SetKernel(SVM.SvmKernelType.Linear);
                    //bool trained = model.TrainAuto(trainData, trainClasses, null, null, p.MCvSVMParams, 5);
                    bool trained = model.Train(td);

                    model.Save("iPhone_size.xml");

                    Matrix<float> test = new Matrix<float>(1, 1);
                    for (int i = 0; i < datas.Count; i++)
                    {
                        test[0, 0] = (float)datas[i].Item1;
                        //test[0, 1] = (float)datas[i].Item2;
                        int l = (int)model.Predict(test);
                        if (l != datas[i].Item2)
                        {
                            Program.logIt($"predict: {l} vs {datas[i].Item2}");
                        }
                    }

                    // test
                    //test[0, 0] = 515f;
                    //test[0, 1] = 1032f;
                    //int p = (int)model.Predict(test);
                    //Program.logIt($"predict: {p} ");
                }
            }
        }
        static void train_iphone_size_data()
        {
            //string fn = @"../../../test/M4_testing_data.txt";
            string fn = @"C:\Tools\avia\images\avia_m0_pc\train_data.txt";
            Regex re = new Regex(@"^.+size={Width=([\d\.]*), Height=([\d\.]*)},.+slabel=(\d+).*$");
            List<Tuple<double, double, int>> datas = new List<Tuple<double, double, int>>();
            foreach (string l in System.IO.File.ReadAllLines(fn))
            {
                if (!string.IsNullOrEmpty(l))
                {
                    if (l.StartsWith("#"))
                    {
                        // comment line
                    }
                    else
                    {
                        Match m = re.Match(l);
                        if (m.Success)
                        {
                            double w = Double.Parse(m.Groups[1].Value);
                            double h = Double.Parse(m.Groups[2].Value);
                            int label = Int32.Parse(m.Groups[3].Value);
                            Tuple<double, double, int> d = new Tuple<double, double, int>(w, h, label);
                            datas.Add(d);
                        }
                    }
                }
            }
            foreach (var v in datas)
            {
                Program.logIt($"w={v.Item1}, h={v.Item2}, label={v.Item3}");
            }

            int trainSampleCount = datas.Count;
            Matrix<float> trainData = new Matrix<float>(trainSampleCount, 2);
            Matrix<int> trainClasses = new Matrix<int>(trainSampleCount, 1);
            for (int i = 0; i < datas.Count; i++)
            {
                trainData[i, 0] = (float)datas[i].Item1;
                trainData[i, 1] = (float)datas[i].Item2;
                trainClasses[i, 0] = datas[i].Item3;
            }
            TrainData td = new TrainData(trainData, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, trainClasses);
            using (SVM model = new SVM())
            {
                if (System.IO.File.Exists("iPhone_size.xml"))
                {
                    model.Load("iPhone_size.xml");

                    Matrix<float> test = new Matrix<float>(1, 2);
                    for (int i = 0; i < datas.Count; i++)
                    {
                        test[0, 0] = (float)datas[i].Item1;
                        test[0, 1] = (float)datas[i].Item2;
                        int l = (int)model.Predict(test);
                        if (l != datas[i].Item3)
                        {
                            Program.logIt($"predict: {l} vs {datas[i].Item3}");
                        }
                    }

                    // test
                    test[0, 0] = 515f;
                    test[0, 1] = 1032f;
                    int p = (int)model.Predict(test);
                    Program.logIt($"predict: {p} ");
                }
                else
                {
                    model.TermCriteria = new MCvTermCriteria(100, 0.00001);
                    model.C = 1;
                    model.Type = SVM.SvmType.CSvc;
                    model.SetKernel(SVM.SvmKernelType.Linear);
                    //bool trained = model.TrainAuto(trainData, trainClasses, null, null, p.MCvSVMParams, 5);
                    bool trained = model.Train(td);

                    model.Save("iPhone_size.xml");

                    Matrix<float> test = new Matrix<float>(1, 2);
                    for (int i = 0; i < datas.Count; i++)
                    {
                        test[0, 0] = (float)datas[i].Item1;
                        test[0, 1] = (float)datas[i].Item2;
                        int l = (int)model.Predict(test);
                        if (l != datas[i].Item3)
                        {
                            Program.logIt($"predict: {l} vs {datas[i].Item3}");
                        }
                    }

                    // test
                    test[0, 0] = 515f;
                    test[0, 1] = 1032f;
                    int p = (int)model.Predict(test);
                    Program.logIt($"predict: {p} ");
                }
            }
        }

        static void test_ML()
        {
            string fn = @"../../test/iphone_color.txt";
            Regex re = new Regex(@"^.+color=\[([\d\.]*),([\d\.]*),([\d\.]*)\], label=(\d+).*$");
            List<Tuple<double, double, double, int>> datas = new List<Tuple<double, double, double, int>>();
            foreach (string l in System.IO.File.ReadAllLines(fn))
            {
                if (!string.IsNullOrEmpty(l))
                {
                    Match m = re.Match(l);
                    if (m.Success)
                    {
                        double r = Double.Parse(m.Groups[1].Value);
                        double g = Double.Parse(m.Groups[2].Value);
                        double b = Double.Parse(m.Groups[3].Value);
                        int label = Int32.Parse(m.Groups[4].Value);
                        Tuple<double, double, double, int> d = new Tuple<double, double, double, int>(r, g, b, label);
                        datas.Add(d);
                    }
                }
            }
            foreach (var v in datas)
            {
                Program.logIt($"R={v.Item1}, G={v.Item2}, B={v.Item3}, label={v.Item4}");
            }
            int trainSampleCount = datas.Count;
            Matrix<float> trainData = new Matrix<float>(trainSampleCount, 3);
            Matrix<int> trainClasses = new Matrix<int>(trainSampleCount, 1);
            for (int i = 0; i < datas.Count; i++)
            {
                trainData[i, 0] = (float)datas[i].Item1;
                trainData[i, 1] = (float)datas[i].Item2;
                trainData[i, 2] = (float)datas[i].Item3;
                trainClasses[i, 0] = datas[i].Item4;
            }
            NormalBayesClassifier classifier = new NormalBayesClassifier();
            classifier.Load(@"traindata/iPhone_color.xml");
            if (true)
            {
                //classifier.Save("iPhone_color.xml");

                Matrix<float> test = new Matrix<float>(1, 3);
                for (int i = 0; i < datas.Count; i++)
                {
                    test[0, 0] = (float)datas[i].Item1;
                    test[0, 1] = (float)datas[i].Item2;
                    test[0, 2] = (float)datas[i].Item3;
                    int l = (int)classifier.Predict(test);
                    if (l != datas[i].Item4)
                    {
                        Program.logIt($"predict: {l} vs {datas[i].Item4}");
                    }
                }
                test[0, 0] = 8.96270396270396f;
                test[0, 1] = 6.96270396270396f;
                test[0, 2] = 6.96270396270396f;
                int p = (int)classifier.Predict(test);
                Program.logIt($"predict: {p} ");
            }

        }
        static Rectangle found_device_image(Image<Gray, Byte> img, double ratio = 10)
        {
            CvInvoke.GaussianBlur(img, img, new Size(3, 3), 0);
            Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
            //img = img.MorphologyEx(MorphOp.Open, k, new Point(-1, -1), 3, BorderType.Default, new MCvScalar(0));
            img = img.MorphologyEx(MorphOp.Gradient, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
            double v = CvInvoke.Threshold(img, img, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
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
            v = ratio * roi.X;
            roi.X = (int)v;
            v = ratio * roi.Y;
            roi.Y = (int)v;
            v = ratio * roi.Width;
            roi.Width = (int)v;
            v = ratio * roi.Height;
            roi.Height = (int)v;
            Program.logIt($"{roi}");
            return roi;
        }
        static Rectangle found_apple_text(Image<Gray, Byte> img, double ratio = 10)
        {
            Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
            img = img.MorphologyEx(MorphOp.Open, k, new Point(-1, -1), 3, BorderType.Default, new MCvScalar(0));
            img = img.MorphologyEx(MorphOp.Gradient, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
            //img = img.MorphologyEx(MorphOp.Open, k, new Point(-1, -1), 3, BorderType.Default, new MCvScalar(0));
            double db = CvInvoke.Threshold(img, img, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
            img.Save("temp_1.jpg");
            Point p = new Point(img.Width / 2, img.Height * 3 / 4);
            Rectangle roi = Rectangle.Empty;
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(img, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    VectorOfPoint contour = contours[i];
                    double a = CvInvoke.ContourArea(contour);
                    if (a > 1000)
                    {
                        Rectangle r = CvInvoke.BoundingRectangle(contour);
                        if (r.Contains(p))
                        {
                            roi = r;
                            Program.logIt($"area={a}, r={roi}");
                        }
                    }
                }
            }
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
            Program.logIt($"{roi}");
            return roi;
        }
        static Dictionary<int, object> find_contour(Dictionary<int, object> data, int id)
        {
            Dictionary<int, object> ret = null;
            foreach (KeyValuePair<int, object> kvp in data)
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
            for (int i = 0; i < cnt; i++)
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
            foreach (string fn in System.IO.Directory.GetFiles(folder, "*.bmp", System.IO.SearchOption.AllDirectories))
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
                    new Tuple<int, int, double>(7,0,0.05),
                    new Tuple<int, int, double>(5,2,0.05),
                    new Tuple<int, int, double>(2,0,0.05),
                    new Tuple<int, int, double>(0,0,0.05),
                    new Tuple<int, int, double>(0,3,0.05),
                    new Tuple<int, int, double>(3,5,0.05),
                    new Tuple<int, int, double>(5,7,0.05),
                };
            Tuple<VectorOfPoint, VectorOfPoint> apple_logo = get_apple_logo();
            foreach (Tuple<int, int, double> p in param)
            {
                using (Mat m = CvInvoke.Imread(filename))
                {
                    double score = 1.0;
                    int most_match = -1;
                    Mat b = prepare_image(m);
                    Image<Gray, Byte> img = b.ToImage<Gray, Byte>();
                    //img = img.Rotate(90, new Gray(0), false);
                    if (p.Item1 > 0)
                        img = img.Erode(p.Item1);
                    if (p.Item2 > 0)
                        img = img.Dilate(p.Item2);
                    CvInvoke.GaussianBlur(img, img, new Size(3, 3), 0);
                    double otsu = CvInvoke.Threshold(img, new Mat(), 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
                    double sigma = 0.25;
                    double lower = Math.Max(1, (1.0 - sigma) * otsu);
                    double upper = Math.Min(255, (1.0 + sigma) * otsu);
                    CvInvoke.Canny(img, img, lower, upper);
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
                        if (most_match >= 0 && most_match < count)
                        {
                            Rectangle r = CvInvoke.BoundingRectangle(contours[most_match]);
                            if (is_apple_logo(r, img.Size))
                            {
                                ret = r;
                            }
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
                    new Tuple<int, int, double>(5,3,0.05),
                    //new Tuple<int, int, double>(5,2,0.05),
                    //new Tuple<int, int, double>(2,0,0.05),
                    //new Tuple<int, int, double>(0,0,0.05),
                    //new Tuple<int, int, double>(0,3,0.05),
                    //new Tuple<int, int, double>(3,5,0.05),
                    //new Tuple<int, int, double>(5,7,0.05),
                };
            string filename = @"C:\Tools\avia\images\test.2\iphone6 Plus Gold\5901.1.jpg";
            foreach (Tuple<int, int, double> p in param)
            {
                using (Mat m = CvInvoke.Imread(filename))
                {
                    Mat b = prepare_image(m);
                    //CvInvoke.Rotate(m, m, RotateFlags.Rotate90Clockwise);
                    double score = 1.0;
                    int most_match = -1;
                    Image<Gray, Byte> img = b.ToImage<Gray, Byte>();
                    //img = img.Rotate(90, new Gray(0), false);
                    if (p.Item1 > 0)
                        img = img.Erode(p.Item1);
                    if (p.Item2 > 0)
                        img = img.Dilate(p.Item2);
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
                                CvInvoke.DrawContours(b, contours, i, new MCvScalar(0, 255, 0));
                            }
                        }
                        if (most_match >= 0 && most_match < count)
                        {
                            Rectangle r = CvInvoke.BoundingRectangle(contours[most_match]);
                            if (is_apple_logo(r, img.Size))
                            {
                                ret = r;
                                CvInvoke.Rectangle(b, ret, new MCvScalar(0, 0, 255), 3);
                            }
                        }
                    }
                    b.Save("temp_2.jpg");
                    if (!ret.IsEmpty)
                        break;
                }
            }
            Program.logIt($"{ret}");
            //return ret;
        }
        static Mat prepare_image(Mat src)
        {
            Mat ret = null;
            Image<Gray, Byte> img = src.ToImage<Gray, Byte>();
            CvInvoke.GaussianBlur(img, img, new Size(3, 3), 0);
            double otsu = CvInvoke.Threshold(img, new Mat(), 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
            double sigma = 0.25;
            double lower = Math.Max(1, (1.0 - sigma) * otsu);
            double upper = Math.Min(255, (1.0 + sigma) * otsu);
            CvInvoke.Canny(img, img, lower, upper);
            //img.Save("temp_1.jpg");

            Rectangle roi = Rectangle.Empty;
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(img, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    Rectangle r = CvInvoke.BoundingRectangle(contours[i]);
                    if (roi.IsEmpty) roi = r;
                    else roi = Rectangle.Union(roi, r);
                }
            }
            //Program.logIt($"{roi}");
            if (!roi.IsEmpty)
            {
                ret = new Mat(src, roi);
                CvInvoke.Rotate(ret, ret, RotateFlags.Rotate90Clockwise);
                //n.Save("temp_2.jpg");
            }
            return ret;
        }
        static Tuple<Mat, Mat> prepare_image(string filename)
        {
            Mat m0 = null;
            Mat m1 = null;
            double ratio = 0.1;
#if true
            using (Mat m = CvInvoke.Imread(filename))
            {
                Image<Gray, Byte> img = m.ToImage<Gray, Byte>().Resize(ratio, Inter.Linear);
                CvInvoke.GaussianBlur(img, img, new Size(3, 3), 0);
                double otsu = CvInvoke.Threshold(img, new Mat(), 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
                double sigma = 0.25;
                double lower = Math.Max(1, (1.0 - sigma) * otsu);
                double upper = Math.Min(255, (1.0 + sigma) * otsu);
                CvInvoke.Canny(img, img, lower, upper);
                Rectangle roi = Rectangle.Empty;
                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(img, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                    int count = contours.Size;
                    for (int i = 0; i < count; i++)
                    {
                        Rectangle r = CvInvoke.BoundingRectangle(contours[i]);
                        if (roi.IsEmpty) roi = r;
                        else roi = Rectangle.Union(roi, r);
                    }
                }
                if (!roi.IsEmpty)
                {
                    //m1 = new Mat(img.Mat, roi);
                    //CvInvoke.Rotate(m1, m1, RotateFlags.Rotate90Clockwise);
                    roi.X = (int)((double)roi.X / ratio);
                    roi.Y = (int)((double)roi.Y / ratio);
                    roi.Width = (int)((double)roi.Width / ratio);
                    roi.Height = (int)((double)roi.Height / ratio);
                    m0 = new Mat(m, roi);
                    CvInvoke.Rotate(m0, m0, RotateFlags.Rotate90Clockwise);
                    m1 = new Mat();
                    CvInvoke.Resize(m0, m1, new Size(0, 0), ratio, ratio);
                }
            }

#else
            using(Mat m = CvInvoke.Imread(filename))
            {
                Image<Gray, Byte> img = m.ToImage<Gray, Byte>();
                CvInvoke.GaussianBlur(img, img, new Size(3, 3), 0);
                double otsu = CvInvoke.Threshold(img, new Mat(), 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
                double sigma = 0.25;
                double lower = Math.Max(1, (1.0 - sigma) * otsu);
                double upper = Math.Min(255, (1.0 + sigma) * otsu);
                CvInvoke.Canny(img, img, lower, upper);
                Rectangle roi = Rectangle.Empty;
                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(img, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                    int count = contours.Size;
                    for (int i = 0; i < count; i++)
                    {
                        Rectangle r = CvInvoke.BoundingRectangle(contours[i]);
                        if (roi.IsEmpty) roi = r;
                        else roi = Rectangle.Union(roi, r);
                    }
                }
                if (!roi.IsEmpty)
                {
                    m0 = new Mat(m, roi);
                    CvInvoke.Rotate(m0, m0, RotateFlags.Rotate90Clockwise);
                    m1 = new Mat();
                    CvInvoke.Resize(m0, m1, new Size(0, 0), 0.1, 0.1);
                }
            }
#endif
            return new Tuple<Mat, Mat>(m0, m1);
        }
        static bool is_apple_logo(Rectangle r, Size sz)
        {
            bool ret = false;
            double w = 0.12 * sz.Width;
            double l = 1.0 * sz.Height / 3;
            Point center = new Point(sz.Width / 2, sz.Height / 2);
            Rectangle target = new Rectangle((int)(center.X - w / 2), (int)(center.Y - l), (int)w, (int)l);
            center.X = r.X + r.Width / 2;
            center.Y = r.Y + r.Height / 2;
            ret = target.Contains(center);
            return ret;
        }
        static void test_ocr()
        {
            Mat b = CvInvoke.Imread(@"temp_1.jpg");
            //Mat b = CvInvoke.Imread(@"C:\projects\avia\pytest\temp_text_2.jpg");
            Image<Gray, Byte> img = b.ToImage<Gray, byte>();
            //CvInvoke.FastNlMeansDenoising(img, img);
            CvInvoke.GaussianBlur(img, img, new Size(3, 3), 0);
            img = img.InRange(new Gray(85), new Gray(95));
            img.Save("temp_2.jpg");
            img._Dilate(3);
            //CvInvoke.GaussianBlur(img, img, new Size(7, 7), 2.0, 2.0);
            //Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));
            //img = img.MorphologyEx(MorphOp.Dilate, k, new Point(-1, -1), 2, BorderType.Default, new MCvScalar(0));
            //img = img.MorphologyEx(MorphOp.Erode, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
            //double v = CvInvoke.Threshold(img, img, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
            //img.Save("temp_2.jpg");
            //CvInvoke.GaussianBlur(img, img, new Size(7, 7), 2.0, 2.0);
            //img = img.Dilate(2);
            //img = img.Erode(1);
            //img = img.MorphologyEx(MorphOp.Blackhat, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
            img.Save("temp_3.jpg");

#if true
            using (TesseractEngine TE = new TesseractEngine("tessdata", "eng", EngineMode.TesseractOnly))
            {
                //Bitmap b = new Bitmap(@"temp_text_3.jpg");
                var p = TE.Process(img.ToBitmap());
                string s = p.GetText();
                s = p.GetHOCRText(0);
            }
#endif
        }
        static void found_text()
        {
            string filename = @"C:\Tools\avia\images\test.1\iphone6 Gold\0123.1.bmp";
            //string folder = @"C:\Tools\avia\images\";
            //foreach (string filename in System.IO.Directory.GetFiles(folder, "*.1.jpg", System.IO.SearchOption.AllDirectories))
            {
                Tuple<Mat, Mat> org_img = prepare_image(filename);
                //using (Mat m = CvInvoke.Imread(filename))
                {
                    //Mat b = prepare_image(m);
                    //CvInvoke.Rotate(m, m, RotateFlags.Rotate90Clockwise);
                    Image<Gray, Byte> img = org_img.Item2.ToImage<Gray, Byte>();
                    img = img.Erode(7);
                    img = img.Dilate(7);
                    CvInvoke.GaussianBlur(img, img, new Size(3, 3), 0);
                    double otsu = CvInvoke.Threshold(img, new Mat(), 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
                    double sigma = 0.25;
                    double lower = Math.Max(1, (1.0 - sigma) * otsu);
                    double upper = Math.Min(255, (1.0 + sigma) * otsu);
                    CvInvoke.Canny(img, img, lower, upper);
                    img.Save("temp_1.jpg");
                    // test
                    Point p = new Point(img.Width / 2, img.Height / 2);
                    double w = 0.12 * img.Width;
                    double h = 0.20 * img.Height;
                    Rectangle r_txt = new Rectangle((int)(img.Width / 2 - w / 2), (int)(2.0 / 3 * img.Height), (int)w, (int)h);

                    List<Rectangle> r_txt_list = new List<Rectangle>();
                    using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                    {
                        CvInvoke.FindContours(img, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
                        int count = contours.Size;
                        for (int i = 0; i < count; i++)
                        {
                            double a = CvInvoke.ContourArea(contours[i]);
                            if (a > 1.0)
                            {
                                Rectangle r = CvInvoke.BoundingRectangle(contours[i]);
                                Point pc = new Point(r.X + r.Width / 2, r.Y + r.Height / 2);
                                if (r_txt.Contains(pc))
                                {
                                    //Program.logIt($"{r}");
                                    CvInvoke.DrawContours(org_img.Item2, contours, i, new MCvScalar(0, 255, 0));
                                    r_txt_list.Add(r);
                                }
                                else
                                {
                                }
                                //CvInvoke.DrawContours(b, contours, i, new MCvScalar(0, 255, 0));
                            }
                        }
                    }
                    CvInvoke.Rectangle(org_img.Item2, r_txt, new MCvScalar(0, 0, 255), 3);
                    // merge txt rect
                    List<List<Rectangle>> ll1 = new List<List<Rectangle>>();
                    foreach (Rectangle r in r_txt_list)
                    {
                        bool add_new_list = true;
                        foreach (List<Rectangle> l in ll1)
                        {
                            foreach (Rectangle r1 in l)
                            {
                                if (r1.IntersectsWith(r))
                                {
                                    l.Add(r);
                                    add_new_list = false;
                                }
                                if (!add_new_list)
                                    break;
                            }
                            if (!add_new_list)
                                break;
                        }
                        if (add_new_list)
                        {
                            List<Rectangle> l = new List<Rectangle>();
                            l.Add(r);
                            ll1.Add(l);
                        }
                    }
                    List<Rectangle> ret = new List<Rectangle>();
                    foreach (List<Rectangle> l in ll1)
                    {
                        Rectangle rr = Rectangle.Empty;
                        foreach (Rectangle r in l)
                        {
                            if (rr.IsEmpty) rr = r;
                            else rr = Rectangle.Union(rr, r);
                        }
                        ret.Add(rr);
                        //CvInvoke.Rectangle(org_img.Item2, rr, new MCvScalar(0, 0, 255), 3);
                    }
                    Program.logIt($"{filename}: dump: text rectangle: ({ret.Count})");
                    for (int i = 0; i < ret.Count; i++)
                    {
                        Rectangle r = ret[i];
                        Program.logIt($"{r}");
                        r.Inflate(r.Width / 10, r.Height / 10);
                        r.X *= 10;
                        r.Y *= 10;
                        r.Width *= 10;
                        r.Height *= 10;
                        Mat c = new Mat(org_img.Item1, r);
                        c.Save($"temp_text_{i + 1}.jpg");
                    }
                    org_img.Item2.Save("temp_2.jpg");
                }
            }
        }
        static void test_5()
        {
            Rectangle r1 = new Rectangle(610, 350, 600, 1100);
            /*
            Mat m0 = CvInvoke.Imread(@"C:\Tools\avia\images\avia_m0_pc\BackGround.jpg");
            Mat m1 = CvInvoke.Imread(@"C:\Tools\avia\images\avia_m0_pc\IPhone6 Gold .jpg");

            CvInvoke.Rotate(m0, m0, RotateFlags.Rotate90CounterClockwise);
            CvInvoke.Rotate(m1, m1, RotateFlags.Rotate90CounterClockwise);

            Image<Bgr, Byte> img0 = m0.ToImage<Bgr, Byte>();
            Image<Bgr, Byte> img1 = m1.ToImage<Bgr, Byte>();

            img0.ROI = r1;
            img1.ROI = r1;

            Image<Hsv, Byte> hsv0 = img0.Convert<Hsv, Byte>();
            Image<Hsv, Byte> hsv1 = img1.Convert<Hsv, Byte>();

            Image<Gray, Byte> mask = hsv0.InRange(new Hsv(45, 100, 50), new Hsv(75, 255, 255));
            int[] area = mask.CountNonzero();
            double r = (double)area[0] / (mask.Width * mask.Height);

            mask = hsv1.InRange(new Hsv(45, 100, 50), new Hsv(75, 255, 255));
            area = mask.CountNonzero();
            r = (double)area[0] / (mask.Width * mask.Height);
            */

            Mat m00 = CvInvoke.Imread(@"C:\Tools\avia\images\avia_m0_pc\FromMyCam\BackGround.jpg");
            CvInvoke.Rotate(m00, m00, RotateFlags.Rotate90CounterClockwise);
            Image<Bgr, Byte> img00 = m00.ToImage<Bgr, Byte>().Copy(r1);

            string dir = @"C:\Tools\avia\images\avia_m0_pc\FromMyCam";
            StringBuilder sb = new StringBuilder();
            foreach (string fn in System.IO.Directory.GetFiles(dir))
            //string fn = @"C:\Tools\avia\images\avia_m0_pc\Iphone7 JetBlack.jpg";
            {
                // check device inplace
                Mat m0 = CvInvoke.Imread(fn);
                CvInvoke.Rotate(m0, m0, RotateFlags.Rotate90CounterClockwise);
                Image<Bgr, Byte> img0 = m0.ToImage<Bgr, Byte>().Copy(r1);
                //img0.ROI = r1;
                Tuple<bool, bool, double> res = AviaGetPhoneSize.AVIAGetPhoneSize.check_device_inplace_v2(img0, 0.33);
                Program.logIt($"{System.IO.Path.GetFileName(fn)}: ret={res.Item1}, device inplace={res.Item2}");
                Size sz = Size.Empty;
                Bgr bgr;
                // get size
                if (res.Item1 && res.Item2)
                {
                    //Image<Bgr, Byte> diff = img0.AbsDiff(img00);
                    //sz = AviaGetPhoneSize.AVIAGetPhoneSize.detect_size(diff.Convert<Gray, Byte>());
                    //Program.logIt($"{System.IO.Path.GetFileName(fn)}: size={sz}");

                    // sample color
                    bgr = AviaGetPhoneSize.AVIAGetPhoneSize.sample_color(img0, new Rectangle(388, 84, 30, 200));
                    Program.logIt($"{System.IO.Path.GetFileName(fn)}: size={bgr}");

                    sb.AppendLine($"{System.IO.Path.GetFileNameWithoutExtension(fn)}, color={bgr}");
                }
            }

            Program.logIt(sb.ToString());
        }
        static bool is_same_frame(Mat m1, Mat m2, double th = 17)
        {
            bool ret = false;
            Mat diff = new Mat();
            CvInvoke.AbsDiff(m1, m2, diff);
            Gray g0 = diff.ToImage<Gray, Byte>().GetAverage();
            if (g0.MCvScalar.V0 < th)
                ret = true;
            return ret;
        }

        static void test_ss()
        {
#if true
            TcpClient client = new TcpClient();
            bool done = false;
            string root = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "AVIA", "frames");
            Regex r = new Regex(@"^ACK frame (.+)\s*$", RegexOptions.IgnoreCase);
            Mat bg = null;
            try
            {
                client.Connect(IPAddress.Loopback, 6280);
                NetworkStream ns = client.GetStream();
                byte[] cmd = System.Text.Encoding.UTF8.GetBytes("QueryFrame\n");
                byte[] data = new byte[1024];
                while (!done)
                {
                    ns.Write(cmd, 0, cmd.Length);
                    int read = ns.Read(data, 0, data.Length);
                    string str = System.Text.Encoding.UTF8.GetString(data, 0, read);
                    Match m = r.Match(str);
                    if (m.Success)
                    {
                        //Mat cm = CvInvoke.Imread(System.IO.Path.Combine(root, m.Groups[1].Value));
                        //if (bg == null)
                        //    bg = cm;
                        //else
                        //{
                        //    Mat diff = new Mat();
                        //    CvInvoke.AbsDiff(cm, bg, diff);
                        //    Image<Gray, byte> img = diff.ToImage<Gray, Byte>();
                        //    Gray ga = img.GetAverage();
                        //    if (ga.MCvScalar.V0 > 17)
                        //        done = true;
                        //    double db = CvInvoke.Threshold(img, new Mat(), 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
                        //}
                    }
                }
            }
            catch (Exception) { }
#else
            TcpClient client = new TcpClient();
            bool done = false;
            string root = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "AVIA", "frames");
            Regex r = new Regex(@"^ACK frame (.+)\s*$", RegexOptions.IgnoreCase);
            BackgroundSubtractorMOG2 bgs = new BackgroundSubtractorMOG2();
            try
            {
                client.Connect(IPAddress.Loopback, 6280);
                NetworkStream ns = client.GetStream();
                while (!done)
                {
                    byte []b = System.Text.Encoding.UTF8.GetBytes("QueryFrame\n");
                    ns.Write(b, 0, b.Length);
                    b = new byte[1024];
                    int read = ns.Read(b, 0, b.Length);
                    string s = System.Text.Encoding.UTF8.GetString(b, 0, read);
                    Match m = r.Match(s);
                    if (m.Success)
                    {
                        string fn = System.IO.Path.Combine(root, m.Groups[1].Value);
                        Mat cm = CvInvoke.Imread(fn);
                        Mat mask = new Mat();
                        bgs.Apply(cm, mask);
                        Image<Gray, Byte> g = mask.ToImage<Gray, Byte>();
                        Gray ga = g.GetAverage();
                        if (ga.MCvScalar.V0 > 17)
                        {
                            done = true;
                        }
                    }
                    System.Threading.Thread.Sleep(500);
                }
                client.Close();
            }
            catch (Exception) { }
#endif
            /*
            VideoCapture vc = new VideoCapture(0);
            if (vc.IsOpened)
            {
                bool b = false;
                double db = vc.GetCaptureProperty(CapProp.Mode);
                b = vc.SetCaptureProperty(CapProp.Mode, 0);
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
            }
            */
        }
        static bool check_apple_icon(string filename, Tuple<VectorOfPoint, VectorOfPoint> apple_logo, double threhold = 0.05)
        {
            bool ret = false;
            Mat m = CvInvoke.Imread(filename);
            RectangleF rf = new RectangleF(0.35f * m.Width, 0.18f * m.Height, 0.30f * m.Width, 0.20f * m.Height);
            Image<Gray, Byte> img = m.ToImage<Gray, Byte>().Copy(Rectangle.Round(rf));
            CvInvoke.GaussianBlur(img, img, new Size(3, 3), 0);
            Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
            img = img.MorphologyEx(MorphOp.Gradient, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
            CvInvoke.Threshold(img, img, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
            double score = 1.0;
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(img, contours, null, RetrType.List, ChainApproxMethod.ChainApproxNone);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    VectorOfPoint contour = contours[i];
                    double a = CvInvoke.ContourArea(contour);
                    if (a > 10000)
                    {
                        double d1 = CvInvoke.MatchShapes(apple_logo.Item1, contours[i], ContoursMatchType.I1);
                        double d2 = CvInvoke.MatchShapes(apple_logo.Item2, contours[i], ContoursMatchType.I1);
                        if (d1 < score)
                        {
                            score = d1;
                        }
                        if (d2 < score)
                        {
                            score = d2;
                        }
                    }
                }
            }
            if (score < threhold)
                ret = true;
            Program.logIt($"{filename}: ret={ret}, score={score}");
            return ret;
        }
        static void test_6()
        {
            Rectangle ROI = new Rectangle(615, 345, 610, 1110);
            string fn0 = @"C:\Tools\avia\images\avia_m0_pc\FromMyCam\BackGround.jpg";
            //string fn0 = @"C:\Tools\avia\images\FromMyCam\BackGround.jpg";

            Image<Bgr, Byte> img_bg = new Image<Bgr, byte>(fn0).Rotate(-90.0, new Bgr(0, 0, 0), false).Copy(ROI);
            Image<Hsv, byte> hsv_bg = img_bg.Convert<Hsv, byte>();
            Image<Gray, Byte> mask_bg = hsv_bg.InRange(new Hsv(45, 100, 50), new Hsv(75, 255, 255));

            StringBuilder sb = new StringBuilder();
            //string fn1 = @"C:\Tools\avia\images\avia_m0_pc\FromMyCam\Iphone7P Gold.jpg";
            //string fn1 = @"C:\Tools\avia\images\FromMyCam\Iphone6 Gold .jpg";
            foreach (string fn1 in System.IO.Directory.GetFiles(@"C:\Tools\avia\images\avia_m0_pc\FromMyCam"))
            {
                if (string.Compare(fn1, fn0, true) == 0)
                    continue;
                Program.logIt($"{System.IO.Path.GetFileNameWithoutExtension(fn1)}");
                Image<Bgr, byte> img1 = new Image<Bgr, byte>(fn1).Rotate(-90.0, new Bgr(0, 0, 0), false).Copy(ROI);
                Image<Hsv, byte> hsvimg1 = img1.Convert<Hsv, byte>();
                Image<Gray, Byte> mask1 = hsvimg1.InRange(new Hsv(45, 100, 50), new Hsv(75, 255, 255));
                //img1.Save(System.IO.Path.GetFileName(fn1));
                Size ret_sz = Size.Empty;
                Bgr ret_bgr = new Bgr(0, 0, 0);
                Hsv ret_hsv = new Hsv(0, 0, 0);
                Lab ret_lab = new Lab(0, 0, 0);
                int w = 0;
                // get size
                if (true)
                {
                    Image<Gray, Byte> diff = mask1.AbsDiff(mask_bg);
                    diff._Erode(1);
                    diff.ROI = new Rectangle(diff.Width / 2, diff.Height / 2, diff.Width / 2, diff.Height / 2);
                    Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
                    diff._MorphologyEx(MorphOp.Gradient, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
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
                    }
                    w = roi.Width - 80;
                    ret_sz = new Size(roi.Width + diff.Width, roi.Height + diff.Height);
                    Program.logIt($"size: {ret_sz}");
                    img1.Copy(new Rectangle(new Point(0, 0), ret_sz)).Save($"{System.IO.Path.GetFileName(fn1)}");
                }

                // get color
                if (true)
                {
                    int x = img1.Width / 2 + 80;
                    int y = 100;
                    Rectangle rc = new Rectangle(x, y, ret_sz.Width - x, ret_sz.Height - y);
                    x = rc.Width;
                    y = rc.Height;
                    SizeF sf = new SizeF(-0.2f * x, -0.2f * y);
                    //rc.Inflate(Size.Round(sf));
                    Image<Bgr, byte> img_c = img1.Copy(rc);
                    //img_c.Save(System.IO.Path.GetFileName(fn1));
                    img_c.Save($"{System.IO.Path.GetFileNameWithoutExtension(fn1)}_color.jpg");
                    Image<Hsv, Byte> img_hsv = img_c.Convert<Hsv, Byte>();
                    Image<Lab, Byte> img_lab = img_c.Convert<Lab, Byte>();
                    Hsv avg_hsv = img_hsv.GetAverage();
                    Program.logIt($"AVG HSV: {avg_hsv}, LAB: {img_lab.GetAverage()}");
                    mask1 = img_hsv.InRange(new Hsv(0, 0, avg_hsv.Value), new Hsv(255, 255, avg_hsv.Value + 11));
                    //mask1 = img_hsv.InRange(new Hsv(0, 0, avg_hsv.Value-1), new Hsv(255, 255, avg_hsv.Value+1));
                    //mask1.Save("temp_4.jpg");

                    Image<Bgr, byte> img_c1 = img_c.Copy(mask1);
                    //img_c1.Save("temp_4.jpg");
                    img_c1.Save($"{System.IO.Path.GetFileNameWithoutExtension(fn1)}_mask.jpg");
                    ret_bgr = img_c.GetAverage(mask1);
                    ret_hsv = img_hsv.GetAverage(mask1);
                    ret_lab = img_lab.GetAverage(mask1);
                    Program.logIt($"GRB: {ret_bgr}, HSV: {ret_hsv}, LAB: {ret_lab}");

                    // test
#if false
                    Bgr g = new Bgr(182, 204, 223);
                    //int rgb_diff = 5;
                    for (int i = 0; i < 5; i++)
                    {
                        int rgb_diff = i;
                        mask1 = img_c.InRange(new Bgr(g.Blue - rgb_diff, g.Green - rgb_diff, g.Red - rgb_diff), new Bgr(g.Blue + rgb_diff, g.Green + rgb_diff, g.Red + rgb_diff));
                        int[] count = mask1.CountNonzero();
                        if (count[0] > 0)
                        {
                            ret_bgr = img_c.GetAverage(mask1);
                            ret_hsv = img_hsv.GetAverage(mask1);
                            Program.logIt($"[Check Gold]: {System.IO.Path.GetFileNameWithoutExtension(fn1)} RGB Diff: {rgb_diff}, GRB: {ret_bgr}, HSV: {ret_hsv}");
                            break;
                        }
                    }
#endif
                    //mask1 = img_hsv.InRange(new Hsv(21, 0, 191), new Hsv(21, 255, 191));
                    //count = mask1.CountNonzero();
                    //ret_bgr = img_c.GetAverage(mask1);
                    //ret_hsv = img_hsv.GetAverage(mask1);
                    //Program.logIt($"GRB: {ret_bgr}, HSV: {ret_hsv}");
#if false
                    Tuple<int, int, int, int>[] colors = new Tuple<int, int, int, int>[]
                    {
                        new Tuple<int, int, int, int>(223,204,182,10),
                        new Tuple<int, int, int, int>(194,199,230,10),
                    };
                    foreach(var c in colors)
                    {
                        Bgr g = new Bgr(c.Item3, c.Item2, c.Item1);
                        for(int i = 0; i < c.Item4; i++)
                        {
                            mask1 = img_c.InRange(new Bgr(g.Blue - i, g.Green - i, g.Red - i), new Bgr(g.Blue + i, g.Green + i, g.Red + i));
                            int[] count = mask1.CountNonzero();
                            if (count[0] > 0)
                            {
                                Bgr bgr1 = img_c.GetAverage(mask1);
                                Hsv hsv1 = img_hsv.GetAverage(mask1);
                                Program.logIt($"[Check #{g}]: {System.IO.Path.GetFileNameWithoutExtension(fn1)} RGB Diff: {i}, GRB: {bgr1 }, HSV: {hsv1}");
                                break;
                            }
                        }
                    }
#endif
                }
                //Program.logIt($"{System.IO.Path.GetFileNameWithoutExtension(fn1)}: size={ret_sz}, RGB={ret_bgr}, hsv={ret_hsv}");
                sb.AppendLine($"{System.IO.Path.GetFileNameWithoutExtension(fn1)}: size={ret_sz}, RGB={ret_bgr}, hsv={ret_hsv}, lab={ret_lab}");
            }
            Program.logIt($"report: {sb.ToString()}");
        }
        static void test_7()
        {
#if true
            //string fn1 = "Iphone7P Gold.jpg";
            //Image<Bgr, Byte> img = new Image<Bgr, byte>(fn);
            //Image<Hsv, Byte> img_hsv = img.Convert<Hsv, Byte>();
            //Image<Lab, Byte> img_lab = img.Convert<Lab, Byte>();

            Dictionary<string, object> colors = null;
            try
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                colors = jss.Deserialize<Dictionary<string, object>>(System.IO.File.ReadAllText("colors.json"));
            }
            catch (Exception) { }
            foreach (string fn1 in System.IO.Directory.GetFiles(@"C:\Tools\avia\images\avia_m0_pc\image_color"))
            {
                Program.logIt($"{fn1}: ++");
                Image<Bgr, Byte> img = new Image<Bgr, byte>(fn1);
                Image<Hsv, Byte> img_hsv = img.Convert<Hsv, Byte>();
                Image<Lab, Byte> img_lab = img.Convert<Lab, Byte>();
                Image<Gray, Byte>[] XYZ = img_hsv.Split();
                double X = CvInvoke.Threshold(XYZ[0], new Mat(), 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
                Lab value = new Lab(0, 0, 0);
#if true
                {
                    Point minLoc = Point.Empty;
                    Point maxLoc = Point.Empty;
                    double minH = 0.0;
                    double maxH = 0.0;
                    CvInvoke.MinMaxLoc(XYZ[0], ref minH, ref maxH, ref minLoc, ref maxLoc);
                    double minS = 0.0;
                    double maxS = 0.0;
                    CvInvoke.MinMaxLoc(XYZ[1], ref minS, ref maxS, ref minLoc, ref maxLoc);
                    Gray s1;
                    MCvScalar s2;
                    XYZ[1].AvgSdv(out s1, out s2);
                    double minV = 0.0;
                    double maxV = 0.0;
                    CvInvoke.MinMaxLoc(XYZ[2], ref minV, ref maxV, ref minLoc, ref maxLoc);
                    Gray v1;
                    MCvScalar v2;
                    XYZ[2].AvgSdv(out v1, out v2);
                    double v3 = CvInvoke.Threshold(XYZ[2], new Mat(), 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
                    DenseHistogram dh = new DenseHistogram(256, new RangeF(0, 255));
                    dh.Calculate<Byte>(new Image<Gray, byte>[] { XYZ[2] }, false, null);
                    Program.logIt($"Value: avg={v1.MCvScalar.V0}, std={v2.V0}, otsu={v3}, overexposure=");
                    for (int i = 0; i < 10; i++)
                    {
                        Image<Gray, Byte> mask = img_hsv.InRange(
                            new Hsv(minH, minS, v1.MCvScalar.V0 - v2.V0),
                            new Hsv(maxH, maxS, v1.MCvScalar.V0 + v2.V0));
                        int[] count = mask.CountNonzero();
                        if (count[0] > 0)
                        {
                            img.Copy(mask).Save($"{System.IO.Path.GetFileNameWithoutExtension(fn1)}_mask.jpg");
                            value = img_lab.GetAverage(mask);
                            break;
                        }
                    }
                }
#else
                {
                    double minY = 0.0;
                    double maxY = 0.0;
                    Point minLoc = Point.Empty;
                    Point maxLoc = Point.Empty;
                    CvInvoke.MinMaxLoc(XYZ[1], ref minY, ref maxY, ref minLoc, ref maxLoc);
                    double minZ = 0.0;
                    double maxZ = 0.0;
                    CvInvoke.MinMaxLoc(XYZ[2], ref minZ, ref maxZ, ref minLoc, ref maxLoc);
                    for(int i=0; i < 10; i++)
                    {
                        Image<Gray, Byte> mask = img_lab.InRange(new Lab(X-i,minY,minZ), new Lab(X+i,maxY,maxZ));
                        int[] count = mask.CountNonzero();
                        if (count[0] > 0)
                        {
                            value = img_lab.GetAverage(mask);
                            break;
                        }
                    }
                }
#endif
                Program.logIt($"{fn1}: color: {value}");
                double score = 100;
                KeyValuePair<string, object> the_color = default(KeyValuePair<string, object>);
                foreach (KeyValuePair<string, object> c in colors)
                {
#if true
                    Dictionary<string, object> cd = (Dictionary<string, object>)c.Value;
                    int step = (int)cd["step"];
                    int x = (int)cd["x"];
                    int y = (int)cd["y"];
                    int z = (int)cd["z"];
                    int th = (int)cd["th"];
                    Lab std = new Lab(x, y, z);
                    double d = getDeltaE(value, std);
                    Program.logIt($"{fn1}: check color: {c.Key} score={d}");
                    if (d <= th)
                    {
                        if (d < score)
                        {
                            score = d;
                            the_color = c;
                        }
                    }

#else
                    Program.logIt($"{fn1}: check color: {c.Key}");
                    //if (string.Compare(fn1, @"C:\Tools\avia\images\avia_m0_pc\image_color\Iphone8 Gold.jpg") == 0)
                    //{

                    //}
                    Dictionary<string, object> cd = (Dictionary<string, object>)c.Value;
                    int step = (int)cd["step"];
                    int x = (int)cd["x"];
                    int y = (int)cd["y"];
                    int z = (int)cd["z"];
                    int th = (int)cd["th"];
                    Lab std = new Lab(x, y, z);
                    for (int i = 0; i < step; i++)
                    {
                        //Image<Gray, Byte> mask = img_lab.InRange(
                        //    new Lab(x - i, y > 128 ? 128 : 0, z > 128 ? 128 : 0), new Lab(x + i, y > 128 ? 255 : 127, z > 128 ? 255: 127));
                        Image<Gray, Byte> mask = img_lab.InRange(
                            new Lab(x - i, y - i, z - i), new Lab(x + i, y + i, z + i));
                        int[] count = mask.CountNonzero();
                        if (count[0] > 0)
                        {
                            Lab avg1 = img_lab.GetAverage(mask);
                            double d = getDeltaE(avg1, std);
                            if (d < th)
                            {
                                // found
                                Program.logIt($"{c.Key}: score={d} step={i}");
                                if (d < score)
                                {
                                    score = d;
                                    the_color = c;
                                }
                                break;
                            }
                        }
                    }
#endif
                }
                string s = $"colorid={the_color.Key}, score={score}";
                Program.logIt($"{fn1}: -- {s}");
            }
#else
            Dictionary<string, Bgr> colors = new Dictionary<string, Bgr>()
            {
                { "iPhone 7 Gold", new Bgr(183,204,223)},
                { "iPhone 7 RoseGold", new Bgr(194,199,230)},
                { "iPhone 7 Silver", new Bgr(226,228,228)},
                { "iPhone 7 Black", new Bgr(51,48,46)},
            };

            foreach(KeyValuePair<string,Bgr> kvp in colors)
            {
                Image<Bgr, Byte> img = new Image<Bgr, byte>(100, 100, kvp.Value);
                Image<Hsv, Byte> img_hsv = img.Convert<Hsv, Byte>();
                Image<Lab, Byte> img_lab = img.Convert<Lab, Byte>();
                Program.logIt($"{kvp.Key}: BGR={img.GetAverage()}, HSV={img_hsv.GetAverage()}, LAB={img_lab.GetAverage()}");
            }

            if (true)
            {
                string fn = "Iphone6P Gold.jpg";
                Image<Bgr, Byte> img = new Image<Bgr, byte>(fn);
                Image<Hsv, Byte> img_hsv = img.Convert<Hsv, Byte>();
                Image<Lab, Byte> img_lab = img.Convert<Lab, Byte>();

                Image<Bgr, Byte> img_gold = new Image<Bgr, byte>(img.Width, img.Height, new Bgr(183, 204, 223));
                Image<Hsv, Byte> img_hsv1 = img_gold.Convert<Hsv, Byte>();
                Image<Lab, Byte> img_lab1 = img_gold.Convert<Lab, Byte>();
                double d1 = distance(img.GetAverage().MCvScalar, img_gold.GetAverage().MCvScalar);
                double d2 = distance(img_hsv.GetAverage().MCvScalar, img_hsv1.GetAverage().MCvScalar);
                double d3 = distance(img_lab.GetAverage().MCvScalar, img_lab1.GetAverage().MCvScalar);
                for (int i = 0; i < 10; i++)
                {
                    Image<Gray, Byte> mask = img_hsv.InRange(new Hsv(0, 45 - i, 221 - i), new Hsv(179, 45 + i, 221 + i));
                    int[] count = mask.CountNonzero();
                    if (count[0] > 0)
                    {
                        Bgr v1 = img.GetAverage(mask);
                        Hsv v2 = img_hsv.GetAverage(mask);
                        Lab v3 = img_lab.GetAverage(mask);
                        d1 = distance(v1.MCvScalar, img_gold.GetAverage().MCvScalar);
                        d2 = distance(v2.MCvScalar, img_hsv1.GetAverage().MCvScalar);
                        d3 = distance(v3.MCvScalar, img_lab1.GetAverage().MCvScalar);
                        //break;
                    }
                }
            }
#endif
        }
        static void test_8()
        {
            Dictionary<string, object> colors = null;
            try
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                colors = jss.Deserialize<Dictionary<string, object>>(System.IO.File.ReadAllText("colors.json"));
            }
            catch (Exception) { }
#if true
            Rectangle rect1 = new Rectangle(0, 0, 400, 825);
            //string fn1 = @"C:\Tools\avia\images\avia_m0_pc\device\IPhone6 Gold .jpg";
            foreach (string fn1 in System.IO.Directory.GetFiles(@"C:\Tools\avia\images\avia_m0_pc\device"))
            {
                Program.logIt($"{System.IO.Path.GetFileName(fn1)} ++");
                Image<Bgr, Byte> img = new Image<Bgr, byte>(fn1);
                Image<Hsv, Byte> img_hsv = img.Convert<Hsv, Byte>();
                int case_type = check_deviceimagetype(img.Copy(new Rectangle(rect1.Width,0,img.Width-rect1.Width,img.Height)), System.IO.Path.GetFileNameWithoutExtension(fn1));
                Program.logIt($"{System.IO.Path.GetFileName(fn1)} case is {case_type}");

                //img.Draw(rect1, new Bgr(0, 0, 0), -1);
                //Image<Gray, Byte> mask = img_hsv.InRange(new Hsv(0, 0, 240), new Hsv(255, 255, 255));
                //img = img.Copy(mask.Not());
                //img.Save("temp_1.jpg");
                //img_hsv = img.Convert<Hsv, Byte>();
                //mask = img_hsv.InRange(new Hsv(45, 10, 0), new Hsv(75, 255, 255));
                //img = img.Copy(mask.Not());
                //img.Save("temp_2.jpg");
                //Image<Bgr, Byte>[] imgs = new Image<Bgr, byte>[]
                //{
                //    img.Copy(new Rectangle(rect1.Width, 0, img.Width-rect1.Width, img.Height)),
                //    img.Copy(new Rectangle(0, rect1.Height, rect1.Width, img.Height-rect1.Height)),
                //};
                //mask = img_hsv.InRange(new Hsv(15, 0, 0), new Hsv(16, 255, 255));
                //img.Copy(mask).Save("temp_2.jpg");
                //mask = img_hsv.InRange(new Hsv(29, 0, 0), new Hsv(31, 255, 255));
                //img.Copy(mask).Save("temp_3.jpg");
                //mask = img_hsv.InRange(new Hsv(114, 0, 0), new Hsv(116, 255, 255));
                //img.Copy(mask).Save("temp_4.jpg");
                //mask = img_hsv.InRange(new Hsv(89, 0, 0), new Hsv(91, 255, 255));
                //img.Copy(mask).Save("temp_5.jpg");
                //img_hsv = img.Convert<Hsv, Byte>();
                //mask = img_hsv.InRange(new Hsv(0, 0, 200), new Hsv(255, 255, 240));
                //img.Copy(mask).Save("temp_2.jpg");
                if (case_type == 1)
                {
                    //sample_color_case_type_1(img);
#if true
                    Rectangle rect = new Rectangle(rect1.Width, 85, img.Width - rect1.Width, img.Height - 250);
                    SizeF sf = new SizeF(-0.2f * rect.Width, 0f);
                    rect.Inflate(Size.Round(sf));
                    img = img.Copy(rect);
                    //img.Save("temp_1.jpg");

                    foreach (KeyValuePair<string, object> kvp in colors)
                    {
                        Dictionary<string, object> ci = (Dictionary<string, object>)kvp.Value;
                        if ((int)ci["case"] == case_type)
                        {
                            Image<Lab, Byte> img_lab = img.Convert<Lab, byte>();
                            Image<Bgr, Byte> img_color = new Image<Bgr, byte>(img.Width, img.Height, new Bgr((int)ci["b"], (int)ci["g"], (int)ci["r"]));
                            //Image<Lab, Byte> img_lab_color = img_color.Convert<Lab, Byte>();
                            Lab img_lab_color = img_color.Convert<Lab, Byte>().GetAverage();
                            Image<Gray, Byte> mask = new Image<Gray, byte>(img.Width, img.Height, new Gray(0));
                            double score = 0;
                            for (int r=0; r<img.Rows; r++)
                            {
                                for(int c=0; c < img.Cols; c++)
                                {
                                    double d = getDeltaE(img_lab[r, c], img_lab_color);
                                    if (d < 4)
                                    {
                                        score += d;
                                        mask[r, c] = new Gray(255);
                                    }
                                }
                            }
                            int cnt = CvInvoke.CountNonZero(mask);
                            if (cnt > 0)
                            {
                                Moments m = CvInvoke.Moments(mask);
                                Point pc = new Point((int)(m.M10 / m.M00), (int)(m.M01 / m.M00));
                                Program.logIt($"[{kvp.Key}]: score={score}, count={cnt}, center={pc}");
                                if (pc.Y > 100)
                                {
                                    double d = score / cnt;
                                    Lab l1 = img_lab.GetAverage(mask);
                                    score = getDeltaE(l1, img_lab_color);
                                    Program.logIt($"[{kvp.Key}]: per pixel={d}, score={score}");
                                }
                            }
                            //Image<Lab, Byte> diff = img.AbsDiff(img_color).Convert<Lab, Byte>();
                            ////diff.Save("temp_1.jpg");
                            //Image<Gray, Byte>  mask = diff.InRange(new Lab(0, 127, 127), new Lab(2, 129, 129));
                            //int cnt = CvInvoke.CountNonZero(mask);
                            //double score = 0;
                            //if (cnt > 0)
                            //{
                            //    // debug
                            //    //img.Copy(mask).Save("temp_3.jpg");
                            //    Moments m = CvInvoke.Moments(mask);
                            //    Point pc = new Point((int)(m.M10 / m.M00), (int)(m.M01 / m.M00));
                            //    //
                            //    Lab l1 = img.Convert<Lab, Byte>().GetAverage(mask);
                            //    double d = getDeltaE(l1, img_color.Convert<Lab, Byte>().GetAverage());
                            //    score = d / cnt;
                            //    Program.logIt($"[{kvp.Key}]: score={score}, {pc}, {img.Convert<Hsv, Byte>().GetAverage(mask)}");
                            //}
                        }
                    }
#endif
                }
                Program.logIt($"{System.IO.Path.GetFileName(fn1)} --");
            }
#else
                colors colors_data = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(colors));
                StreamReader reader = new StreamReader("device_colors.xml");
                colors_data = (colors)serializer.Deserialize(reader);
                reader.Close();
            }
            catch (Exception) { }

            //string fn1 = @"C:\Tools\avia\images\avia_m0_pc\device\Iphone7P Silver.jpg";
            foreach (string fn1 in System.IO.Directory.GetFiles(@"C:\Tools\avia\images\avia_m0_pc\color_bar"))
            {
                Program.logIt($"{fn1}: ++");
                Image<Bgr, byte> img = new Image<Bgr, byte>(fn1);
                Image<Hsv, byte> img_hsv = img.Convert<Hsv, byte>();
                Image<Gray, Byte> mask = img_hsv.InRange(
                    new Hsv(0, 0, 0), new Hsv(179, 255, 127));
                int cnt = CvInvoke.CountNonZero(mask);
                double r = 1.0 * cnt / (img.Width * img.Height);
                if (r > 0.5)
                {
                    // darker color, i.e. black
                    continue;
                }
                else
                {
                    mask = img_hsv.InRange(
                        new Hsv(0, 0, 235), new Hsv(179, 255, 255));
                    cnt = CvInvoke.CountNonZero(mask);
                    r = 1.0 * cnt / (img.Width * img.Height);
                    Program.logIt($"{fn1}: over={r:P} ");
                    if (r < 0.35)
                    {
                        // glass surface
                    }
                    else
                    {
                    }
                    r = 0.3 * img.Width;
                    Image<Bgr, byte> img1 = img.Copy(new Rectangle((int)r, 0, (int)(img.Width - 2 * r), img.Height - 300));
                    img1.Save($"{System.IO.Path.GetFileName(fn1)}");
                    img_hsv = img1.Convert<Hsv, byte>();
                    mask = img_hsv.InRange(new Hsv(0, 0, 240), new Hsv(179, 255, 255));
                    mask = mask.Not();
                    img1.Copy(mask).Save($"{System.IO.Path.GetFileName(fn1)}");
                    Hsv avg = img_hsv.GetAverage(mask);
                    Lab avg1 = img1.Convert<Lab, Byte>().GetAverage(mask);
                    Program.logIt($"{fn1}: {avg} {avg1}");
                }
                //Image<Gray, Byte> mask1 = img_hsv.InRange(new Hsv(45, 100, 50), new Hsv(75, 255, 255));
                //mask._And(mask1.Not());
                //mask.Save($"{System.IO.Path.GetFileNameWithoutExtension(fn1)}_mask.jpg");
                //img.Copy(mask).Save($"{System.IO.Path.GetFileName(fn1)}");
                Program.logIt($"{fn1}: -- ");
            }
#endif
        }
        static void test_9()
        {
            process_image(null);
#if true
            string fn = @"C:\Tools\avia\images\temp2\background.jpg";
            Image<Gray, Byte> img = new Image<Gray, byte>(fn);
            img.ROI = new Rectangle(img.Width / 2, img.Height / 2, img.Width / 2, img.Height / 2);
            
            img._Erode(1);
            Mat k = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
            img._MorphologyEx(MorphOp.Gradient, k, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
            img.Save("temp_1.jpg");

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
                Program.logIt($"size: {ret_sz}");
            }
#endif
        }
        static void process_image(Bitmap img01)
        {
            string fn = @"C:\Tools\avia\images\temp2\background.jpg";
            Bitmap mBackGround = new Bitmap(fn);
            string fn1 = @"C:\Tools\avia\images\temp2\WIN_20190819_16_01_20_Pro.jpg";
            Bitmap img = new Bitmap(fn1);
            Rectangle ROI = new Rectangle(783, 582, 528, 1068);
            Hsv hsv_low = new Hsv(75, 0, 50);
            Hsv hsv_high = new Hsv(95, 255, 255);

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
                }
            }
        }
        static double getDeltaE(Lab l1, Lab l2)
        {
            return Math.Sqrt(
                Math.Pow(l1.X - l2.X, 2) +
                Math.Pow(l1.Y - l2.Y, 2) +
                Math.Pow(l1.Z - l2.Z, 2));
        }
        static double distance(MCvScalar i1, MCvScalar i2)
        {
            double ret = 0;
            //MCvScalar v = new MCvScalar(i1.V0 - i2.V0, i1.V1 - i2.V1, i1.V2 - i2.V2, i1.V3 - i2.V3);
            ret = Math.Sqrt(
                Math.Pow(i1.V0 - i2.V0, 2) +
                Math.Pow(i1.V1 - i2.V1, 2) +
                Math.Pow(i1.V2 - i2.V2, 2) +
                Math.Pow(i1.V3 - i2.V3, 2));
            return ret;
        }
        static Image<Hsv, Byte> exposure(Image<Hsv, Byte> img, double diff)
        {
            Image<Hsv, Byte> ret = img;
            Image<Gray, Byte>[] chs = img.Split();
            Image<Gray, Byte> img_add = new Image<Gray, byte>(img.Width, img.Height, new Gray(0));
            Image<Gray, Byte> nv = chs[2].AddWeighted(img_add, 1 + diff, 0, 0);
            ret = new Image<Hsv, byte>(new Image<Gray, Byte>[] { chs[0], chs[1], nv });
            return ret;
        }
        static void toXml()
        {
            Tuple<string, Bgr, Lab>[] colors = new Tuple<string, Bgr, Lab>[]
            {
                new Tuple<string, Bgr, Lab>("9", new Bgr(183,204,223),new Lab(212,131,141)),
                new Tuple<string, Bgr, Lab>("5", new Bgr(197,221,245),new Lab(228,133,143)),
            };
            try
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                string s = jss.Serialize(colors);
                System.IO.File.WriteAllText("colors.json", s);

                XmlSerializer serializer = new XmlSerializer(typeof(colors));
                StreamReader reader = new StreamReader("device_colors.xml");
                colors c = (colors)serializer.Deserialize(reader);
                reader.Close();

            }
            catch (Exception) { }
        }
        static int check_deviceimagetype(Image<Bgr, Byte> img, string fn = "")
        {
            int ret = 0;
            Program.logIt($"[{fn}]check_deviceimagetype: ++");
            int area = img.Width * img.Height;
            Image<Hsv, Byte> img_hsv = img.Convert<Hsv, Byte>();
            Image<Gray, Byte> mask = img_hsv.InRange(new Hsv(0, 0, 0), new Hsv(255, 255, 127));
            double ratio = 1.0 * CvInvoke.CountNonZero(mask) / area;
            if (ratio > 0.5)
            {
                // dark color surface
                ret = 3;
            }
            else
            {
                mask = img_hsv.InRange(new Hsv(0, 0, 242), new Hsv(255, 255, 255));
                ratio = 1.0 * CvInvoke.CountNonZero(mask) / area;
                //Program.logIt($"[{fn}]check_deviceimagetype: ratio={ratio:P}");
                if (ratio < 0.3)
                {
                    // glass surface
                    ret = 2;
                }
                else
                {
                    // metal surface
                    ret = 1;
                }
            }
            Program.logIt($"[{fn}]check_deviceimagetype: -- ret={ret} ratio={ratio}");
            return ret;
        }
        static Image<Bgr, Byte> filter_color(Image<Bgr, Byte> src, Hsv h, Hsv l)
        {
            Image<Bgr, Byte> ret = null;
            Image<Hsv, float> hsv = src.Convert<Hsv, float>();
            Image<Gray, Byte> mask = hsv.InRange(h, l);
            ret = src.Copy(mask.Not());
            return ret;
        }
        static void sample_color_case_type_1(Image<Bgr,Byte> src, int case_type=1)
        {
            Program.logIt("sample_color_case_type_1: ++");
            Rectangle tray_rect = new Rectangle(0, 0, 400, 825);
            Rectangle rect = new Rectangle(tray_rect.Width, 85, src.Width - tray_rect.Width, src.Height - 250);
            SizeF sf = new SizeF(-0.2f * rect.Width, 0f);
            rect.Inflate(Size.Round(sf));
            Image<Bgr,Byte> img = src.Copy(rect);
            Program.logIt($"sample_color_case_type_1: {img.Convert<Hsv,Byte>().GetAverage()}");
            // all colors
            Dictionary<string, object> colors = null;
            try
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                colors = jss.Deserialize<Dictionary<string, object>>(System.IO.File.ReadAllText("colors.json"));
            }
            catch (Exception) { }
            //
            double score = 100;
            KeyValuePair<string, object> ret = default(KeyValuePair<string, object>);
            //List<Tuple<double, Point, KeyValuePair<string, object>>> result = new List<Tuple<double, Point, KeyValuePair<string, object>>>();
            foreach (KeyValuePair<string,object> kvp in colors)
            {
                Dictionary<string, object> ci = (Dictionary<string, object>)kvp.Value;
                if ((int)ci["case"] == case_type)
                {
                    Image<Bgr, Byte> img_color = new Image<Bgr, byte>(img.Width, img.Height, new Bgr((int)ci["b"], (int)ci["g"], (int)ci["r"]));
                    Image<Lab, Byte> diff = img.AbsDiff(img_color).Convert<Lab, Byte>();
                    //diff.Save("temp_1.jpg");
                    Image<Gray, Byte> mask = diff.InRange(new Lab(0, 127, 127), new Lab(2, 129, 129));
                    int cnt = CvInvoke.CountNonZero(mask);
                    if (cnt > 0)
                    {
                        // debug
                        //img.Copy(mask).Save("temp_3.jpg");
                        Moments m = CvInvoke.Moments(mask);
                        Point pc = new Point((int)(m.M10 / m.M00), (int)(m.M01 / m.M00));
                        //
                        Lab l1 = img.Convert<Lab, Byte>().GetAverage(mask);
                        double d = getDeltaE(l1, img_color.Convert<Lab, Byte>().GetAverage());
                        d = d / cnt;
                        Program.logIt($"[{kvp.Key}]: score={d}, {pc}, {img.Convert<Hsv, Byte>().GetAverage(mask)}");
                        //result.Add(new Tuple<double, Point, KeyValuePair<string, object>>(score, pc, kvp));
                        if(pc.Y>100 && d < score)
                        {
                            score = d;
                            ret = kvp;
                        }
                    }
                }
            }
            Program.logIt($"sample_color_case_type_1: -- ret={ret.Key} score={score}");
        }
        [STAThread]
        static void test_form()
        {
            Application.EnableVisualStyles();
            Application.Run(new Form1());
        }
    }
}
