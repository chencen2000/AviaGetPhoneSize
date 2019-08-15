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
            test_6();
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

            Image<Gray,Byte> mask = img0.InRange(new Bgr(38,58,39), new Bgr(90,120,70));
            //mask.Save("temp_2.jpg");
            int[] area = mask.CountNonzero();
            Program.logIt($"{(double)area[0]/(mask.Width*mask.Height):P}");

            mask = img1.InRange(new Bgr(38, 58, 39), new Bgr(90, 120, 70));
            area = mask.CountNonzero();
            Program.logIt($"{(double)area[0] / (mask.Width * mask.Height):P}");
        }
        static void test_2()
        {
            string fn = @"../../test/iphone_color.txt";
            Regex re = new Regex(@"^.+color=\[([\d\.]*),([\d\.]*),([\d\.]*)\], label=(\d+).*$");
            List<Tuple<double, double, double, int>> datas = new List<Tuple<double, double, double, int>>();
            foreach(string l in System.IO.File.ReadAllLines(fn))
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
            foreach(var v in datas)
            {
                Program.logIt($"R={v.Item1}, G={v.Item2}, B={v.Item3}, label={v.Item4}");
            }

            int trainSampleCount = datas.Count;
            Matrix<float> trainData = new Matrix<float>(trainSampleCount, 3);
            Matrix<int> trainClasses = new Matrix<int>(trainSampleCount, 1);
            for(int i=0; i<datas.Count; i++)
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
        static Rectangle found_device_image(Image<Gray, Byte> img, double ratio=10)
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
        static Rectangle found_apple_text(Image<Gray,Byte> img, double ratio= 10)
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
                for(int i=0; i<count; i++)
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
                        if(most_match>=0 && most_match < count)
                        {
                            Rectangle r = CvInvoke.BoundingRectangle(contours[most_match]);
                            if(is_apple_logo(r, img.Size))
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
        static Tuple<Mat,Mat> prepare_image(string filename)
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
                    for(int i=0; i< ret.Count; i++)
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
                if(res.Item1 && res.Item2)
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
        static bool is_same_frame(Mat m1, Mat m2, double th=17)
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
        static bool check_apple_icon(string filename, Tuple<VectorOfPoint, VectorOfPoint> apple_logo, double threhold=0.05)
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
            //string fn0 = @"C:\Tools\avia\images\avia_m0_pc\FromMyCam\BackGround.jpg";
            string fn0 = @"C:\Tools\avia\images\FromMyCam\BackGround.jpg";

            Image<Bgr, Byte> img_bg = new Image<Bgr, byte>(fn0).Rotate(-90.0, new Bgr(0, 0, 0), false).Copy(ROI);
            Image<Hsv, byte> hsv_bg = img_bg.Convert<Hsv, byte>();
            Image<Gray, Byte> mask_bg = hsv_bg.InRange(new Hsv(45, 100, 50), new Hsv(75, 255, 255));

            StringBuilder sb = new StringBuilder();
            //string fn1 = @"C:\Tools\avia\images\avia_m0_pc\FromMyCam\Iphone6s Gray.jpg";
            string fn1 = @"C:\Tools\avia\images\FromMyCam\Iphone6 Gold .jpg";
            //foreach (string fn1 in System.IO.Directory.GetFiles(@"C:\Tools\avia\images\FromMyCam"))
            {
                //if (string.Compare(fn1, fn0, true) == 0)
                //    continue;
                Program.logIt($"{System.IO.Path.GetFileNameWithoutExtension(fn1)}");
                Image<Bgr, byte> img1 = new Image<Bgr, byte>(fn1).Rotate(-90.0, new Bgr(0, 0, 0), false).Copy(ROI);
                Image<Hsv, byte> hsvimg1 = img1.Convert<Hsv, byte>();
                Image<Gray, Byte> mask1 = hsvimg1.InRange(new Hsv(45, 100, 50), new Hsv(75, 255, 255));
                //img1.Save(System.IO.Path.GetFileName(fn1));
                Size ret_sz = Size.Empty;
                Bgr ret_bgr = new Bgr(0, 0, 0);
                Hsv ret_hsv = new Hsv(0, 0, 0);
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
                    ret_sz = new Size(roi.Width + diff.Width / 2, roi.Height + diff.Height / 2);
                    Program.logIt($"size: {ret_sz}");
                }

                // get color
                if (true)
                {
                    int x = img1.Width / 2 + 80;
                    int y = 100;
                    Rectangle rc = new Rectangle(x, y, w, 300);
                    x = rc.Width;
                    y = rc.Height;
                    SizeF sf = new SizeF(-0.2f * x, -0.2f * y);
                    rc.Inflate(Size.Round(sf));
                    Image<Bgr, byte> img_c = img1.Copy(rc);
                    img_c.Save(System.IO.Path.GetFileName(fn1));
                    Image<Hsv, Byte> img_hsv = img_c.Convert<Hsv, Byte>();
                    Hsv avg_hsv = img_hsv.GetAverage();                    
                    Program.logIt($"AVG HSV: {avg_hsv}");
                    mask1 = img_hsv.InRange(new Hsv(0, Math.Floor(avg_hsv.Satuation)-1, Math.Floor(avg_hsv.Value - 2)-1), new Hsv(255, Math.Ceiling(avg_hsv.Satuation)+1, Math.Ceiling(avg_hsv.Value)+1));
                    //mask1 = img_hsv.InRange(new Hsv(0, 0, avg_hsv.Value-1), new Hsv(255, 255, avg_hsv.Value+1));
                    //mask1.Save("temp_4.jpg");

                    Image<Bgr, byte> img_c1 = img_c.Copy(mask1);
                    //img_c1.Save("temp_4.jpg");
                    img_c1.Save($"{System.IO.Path.GetFileNameWithoutExtension(fn1)}_mask.jpg");
                    ret_bgr = img_c.GetAverage(mask1);
                    ret_hsv = img_hsv.GetAverage(mask1);
                    Program.logIt($"GRB: {ret_bgr}, HSV: {ret_hsv}");

                    // test
                    mask1=img_c.InRange(new Bgr(201,224,246),new Bgr(201,224,246));
                    int[] count=mask1.CountNonzero();
                    mask1 = img_hsv.InRange(new Hsv(20,0,0), new Hsv(20, 255, 255));
                    count = mask1.CountNonzero();
                }
                //Program.logIt($"{System.IO.Path.GetFileNameWithoutExtension(fn1)}: size={ret_sz}, RGB={ret_bgr}, hsv={ret_hsv}");
                sb.AppendLine($"{System.IO.Path.GetFileNameWithoutExtension(fn1)}: size={ret_sz}, RGB={ret_bgr}, hsv={ret_hsv}");
            }
            Program.logIt($"report: {sb.ToString()}");
        }
        [STAThread]
        static void test_form()
        {
            Application.EnableVisualStyles();
            Application.Run(new Form1());
        }
    }
}
