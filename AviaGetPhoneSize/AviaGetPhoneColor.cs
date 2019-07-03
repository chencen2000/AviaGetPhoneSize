using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.IO;
using Emgu.CV;

namespace AviaGetPhoneSize
{
    class AviaGetPhoneColor
    {
        static SerialPort sp;
        static System.Collections.Concurrent.ConcurrentQueue<string> _queue = new System.Collections.Concurrent.ConcurrentQueue<string>();
        static int Main(string[] args)
        {
            int ret = 0;
            //test();
            //test_1();
            test_2();
            return ret;
        }

        static bool openSerialPort(string port = "COM4")
        {
            bool ret = false;
            try
            {
                sp = new SerialPort();
                sp.PortName = port;
                sp.BaudRate = 9600;
                sp.Parity = Parity.None;
                sp.DataBits = 8;
                sp.StopBits = StopBits.One;
                sp.Open();
                ret = sp.IsOpen;
            }
            catch (Exception) { }
            return ret;
        }
        static void test_2()
        {
            using (FileStorage fs = new FileStorage("color_sensor.xml", FileStorage.Mode.Read))
            {
                FileNode fn = fs.GetNode("data");
                Mat data = new Mat();
                fn.ReadMat(data);
                fn = fs.GetNode("label");
                Mat label = new Mat();
                fn.ReadMat(label);
                Matrix<int> l = new Matrix<int>(data.Rows, 1);
                double ret = CvInvoke.Kmeans(data, 4, l, new Emgu.CV.Structure.MCvTermCriteria(15), 2, Emgu.CV.CvEnum.KMeansInitType.PPCenters);

                Matrix<float> md = new Matrix<float>(data.Rows, data.Cols);
                data.CopyTo(md);
            }
        }
        static void test_1()
        {
            List<Tuple<int, int, int, int, int, int>> data = new List<Tuple<int, int, int, int, int, int>>();
            using (StreamReader sr = new StreamReader("data.csv"))
            {
                string line = sr.ReadLine();
                while (line != null)
                {
                    line = sr.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        string[] s = line.Split(',');
                        int t = Int32.Parse(s[0]);
                        int l = Int32.Parse(s[1]);
                        int r = Int32.Parse(s[2]);
                        int g = Int32.Parse(s[3]);
                        int b = Int32.Parse(s[4]);
                        int c = Int32.Parse(s[5]);
                        data.Add(new Tuple<int, int, int, int, int, int>(t, l, r, g, b, c));
                    }
                }
            }
            Matrix<float> td = new Matrix<float>(data.Count, 6);
            Matrix<int> label = new Matrix<int>(data.Count, 1);
            for (int i=0; i<data.Count; i++)
            {
                td[i, 0] = data[i].Item1;
                td[i, 1] = data[i].Item2;
                td[i, 2] = data[i].Item3;
                td[i, 3] = data[i].Item4;
                td[i, 4] = data[i].Item5;
                td[i, 5] = data[i].Item6;
            }
            double ret = CvInvoke.Kmeans(td, 4, label, new Emgu.CV.Structure.MCvTermCriteria(15), 2, Emgu.CV.CvEnum.KMeansInitType.PPCenters);
            using (StreamWriter sw = new StreamWriter("data_l.csv"))
            {
                sw.WriteLine("temp,lux,r,g,b,c,label");
                for(int i=0; i< td.Rows; i++)
                {
                    sw.WriteLine($"{td[i, 0]},{td[i, 1]}, {td[i, 2]}, {td[i, 3]}, {td[i, 4]}, {td[i, 5]}, {label[i, 0]}");
                }
            }
            using(FileStorage fs = new FileStorage("color_sensor.xml", FileStorage.Mode.Write))
            {
                fs.Write(td.Mat, "data");
                fs.Write(label.Mat, "label");
            }
        }
        static void test()
        {
            List<Tuple<int, int, int, int, int, int>> datas = new List<Tuple<int, int, int, int, int, int>>();
            if (openSerialPort())
            {
                System.Threading.ThreadPool.QueueUserWorkItem((o) =>
                {
                    try
                    {
                        while (sp.IsOpen)
                        {
                            if (sp.BytesToRead > 0)
                            {
                                string s = sp.ReadLine();
                                _queue.Enqueue(s);
                            }
                        }
                    }
                    catch (Exception) { }
                });
                System.Threading.ThreadPool.QueueUserWorkItem((o) =>
                {
                    Regex re = new Regex(@"^Color Temp: (\d+) K - Lux: (\d+) - R: (\d+) G: (\d+) B: (\d+) C: (\d+)\s*$");
                    while (true)
                    {
                        while (!_queue.IsEmpty)
                        {
                            string s;
                            if(_queue.TryDequeue(out s))
                            {
                                Match m = re.Match(s);
                                if (m.Success)
                                {
                                    int t = Int32.Parse(m.Groups[1].Value);
                                    int l = Int32.Parse(m.Groups[2].Value);
                                    int r = Int32.Parse(m.Groups[3].Value);
                                    int g = Int32.Parse(m.Groups[4].Value);
                                    int b = Int32.Parse(m.Groups[5].Value);
                                    int c = Int32.Parse(m.Groups[6].Value);
                                    //System.Console.WriteLine($"t={m.Groups[1].Value}, l={m.Groups[2].Value}, r={m.Groups[3].Value}, g={m.Groups[4].Value}, b={m.Groups[5].Value}, c={m.Groups[6].Value}");
                                    System.Console.WriteLine($"t={t}, l={l}, r={r}, g={g}, b={b}, c={c}");
                                    datas.Add(new Tuple<int, int, int, int, int, int>(t, l, r, g, b, c));
                                }                                
                            }
                        }
                    }
                });

                System.Console.WriteLine("press any key to termine.");
                System.Console.ReadKey();
                sp.Close();
            }
            using (StreamWriter sw = new StreamWriter("data.csv"))
            {
                sw.WriteLine("temp,lux,r,g,b,c");
                foreach(var v in datas)
                {
                    sw.WriteLine($"{v.Item1},{v.Item2},{v.Item3},{v.Item4},{v.Item5},{v.Item6}");
                }
            }
        }
    }
}
