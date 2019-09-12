using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.IO;
using Emgu.CV;
using Microsoft.Win32;
using System.Drawing;

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
            //test_2();
            //load_data_for_background();
            //train_bg_data();
            //test_device_detection();
            Tuple<bool,Color> res= read_color();
            Console.WriteLine($"readcolor={res}");
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
        static void load_data_for_background()
        {
            string s = @"..\..\test\background.txt";
            string[] lines = System.IO.File.ReadAllLines(s);
            Regex re = new Regex(@"^Color Temp: (\d+) K - Lux: (\d+) - R: (\d+) G: (\d+) B: (\d+) C: (\d+)\s*$");
            List<Dictionary<string, object>> bg_data = new List<Dictionary<string, object>>();
            foreach(string l in lines)
            {
                Match m = re.Match(l);
                if (m.Success)
                {
                    Dictionary<string, object> r = new Dictionary<string, object>();
                    r.Add("t", Int32.Parse(m.Groups[1].Value));
                    r.Add("l", Int32.Parse(m.Groups[2].Value));
                    r.Add("r", Int32.Parse(m.Groups[3].Value));
                    r.Add("g", Int32.Parse(m.Groups[4].Value));
                    r.Add("b", Int32.Parse(m.Groups[5].Value));
                    r.Add("c", Int32.Parse(m.Groups[6].Value));
                    bg_data.Add(r);
                }
            }

            s = @"..\..\test\all_data.txt";
            lines = System.IO.File.ReadAllLines(s);
            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
            foreach (string l in lines)
            {
                Match m = re.Match(l);
                if (m.Success)
                {
                    Dictionary<string, object> r = new Dictionary<string, object>();
                    r.Add("t", Int32.Parse(m.Groups[1].Value));
                    r.Add("l", Int32.Parse(m.Groups[2].Value));
                    r.Add("r", Int32.Parse(m.Groups[3].Value));
                    r.Add("g", Int32.Parse(m.Groups[4].Value));
                    r.Add("b", Int32.Parse(m.Groups[5].Value));
                    r.Add("c", Int32.Parse(m.Groups[6].Value));
                    data.Add(r);
                }
            }
            Random rd = new Random();
            List<int> index = new List<int>();
            for (int i=0; i<bg_data.Count; i++)
            {
                while (true)
                {
                    int n = rd.Next(0,data.Count);
                    if (!index.Contains(n))
                    {
                        index.Add(n);
                        break;
                    }
                }
            }
            List<Dictionary<string, object>> r_data = new List<Dictionary<string, object>>();
            foreach(int i in index)
            {
                r_data.Add(data[i]);
            }
            Dictionary<string, object> ret = new Dictionary<string, object>();
            ret.Add("bg", bg_data);
            ret.Add("data", bg_data);
            try
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                s = jss.Serialize(ret);
                System.IO.File.WriteAllText("bg_train.json", s);
            }
            catch (Exception) { }
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
            for (int i = 0; i < data.Count; i++)
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
                for (int i = 0; i < td.Rows; i++)
                {
                    sw.WriteLine($"{td[i, 0]},{td[i, 1]}, {td[i, 2]}, {td[i, 3]}, {td[i, 4]}, {td[i, 5]}, {label[i, 0]}");
                }
            }
            using (FileStorage fs = new FileStorage("color_sensor.xml", FileStorage.Mode.Write))
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
                            if (_queue.TryDequeue(out s))
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
                foreach (var v in datas)
                {
                    sw.WriteLine($"{v.Item1},{v.Item2},{v.Item3},{v.Item4},{v.Item5},{v.Item6}");
                }
            }
        }
        static void test_device_detection()
        {
            string s = @"..\..\test\background.txt";
            string[] lines = System.IO.File.ReadAllLines(s);
            Regex re = new Regex(@"^Color Temp: (\d+) K - Lux: (\d+) - R: (\d+) G: (\d+) B: (\d+) C: (\d+)\s*$");
            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
            foreach (string l in lines)
            {
                Match m = re.Match(l);
                if (m.Success)
                {
                    Dictionary<string, object> r = new Dictionary<string, object>();
                    r.Add("t", Int32.Parse(m.Groups[1].Value));
                    r.Add("l", Int32.Parse(m.Groups[2].Value));
                    int rr = Int32.Parse(m.Groups[3].Value);
                    int rg = Int32.Parse(m.Groups[4].Value);
                    int rb = Int32.Parse(m.Groups[5].Value);
                    int rc = Int32.Parse(m.Groups[6].Value);
                    r.Add("rr", Int32.Parse(m.Groups[3].Value));
                    r.Add("rg", Int32.Parse(m.Groups[4].Value));
                    r.Add("rb", Int32.Parse(m.Groups[5].Value));
                    r.Add("rc", Int32.Parse(m.Groups[6].Value));
                    r.Add("r", 255.0 * rr / rc);
                    r.Add("g", 255.0 * rg / rc);
                    r.Add("b", 255.0 * rb / rc);
                    data.Add(r);
                }
            }
            try
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                s = jss.Serialize(data);
                System.IO.File.WriteAllText("test.json", s);
            }
            catch (Exception) { }
        }
        public static Tuple<bool, string> found_CH340_port()
        {
            bool ret = false;
            string rets = string.Empty;
            Program.logIt("found_CH340_port: ++");
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\CH341SER_A64\Enum");
                if (key != null)
                {
                    object o = key.GetValue("count");
                    if (o != null && o.GetType() == typeof(Int32))
                    {
                        int cnt = (int)o;
                        if (cnt == 1)
                        {
                            o = key.GetValue("0");
                            if (o != null && o.GetType() == typeof(string))
                            {
                                RegistryKey k = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Enum\{o.ToString()}\Device Parameters");
                                if (k != null)
                                {
                                    o = k.GetValue("Portname");
                                    if (o != null && o.GetType() == typeof(string))
                                    {
                                        ret = true;
                                        rets = o.ToString();
                                    }
                                    k.Close();
                                }
                            }
                        }
                        else if (cnt == 0)
                        {
                            Program.logIt($"found_CH340_port: no comport on system");
                        }
                        else
                            Program.logIt($"found_CH340_port: more than one comport on system");
                    }
                    key.Close();
                }
            }
            catch (Exception) { }
            Program.logIt($"found_CH340_port: -- ret={ret} port={rets}");
            return new Tuple<bool, string>(ret, rets);
        }
        public static Tuple<bool, Color> read_color()
        {
            Program.logIt("read_color: ++");
            bool done = false;
            Color c = Color.Empty;
            Tuple<bool, string> comport = found_CH340_port();
            if (comport.Item1)
            {
                Regex regx = new Regex(@"^R: (\d+) G: (\d+) B: (\d+)\s*$");
                SerialPort sp = new SerialPort();
                sp.PortName = comport.Item2;
                sp.BaudRate = 9600;
                sp.Parity = Parity.None;
                sp.DataBits = 8;
                sp.StopBits = StopBits.One;
                sp.ReadTimeout = SerialPort.InfiniteTimeout;
                sp.Open();
                if (sp.IsOpen)
                {
                    sp.DiscardInBuffer();

                    string s = sp.ReadLine();
                    Program.logIt($"first: {s}");
                    //while (string.Compare(s, "Found Sensor\r") != 0)
                    //{
                    //    s = sp.ReadLine();
                    //} 

                    //System.Threading.Thread.Sleep(1000);
                    // led on
                    sp.Write(new byte[] { 0xff }, 0, 1);
                    s = sp.ReadLine();
                    //System.Threading.Thread.Sleep(200);
                    while (sp.IsOpen && !done)
                    {
                        s = sp.ReadLine();
                        Match m = regx.Match(s);
                        if (m.Success)
                        {
                            int r, g, b;
                            if (Int32.TryParse(m.Groups[1].Value, out r) && Int32.TryParse(m.Groups[2].Value, out g) && Int32.TryParse(m.Groups[3].Value, out b))
                            {
                                if (r != 0 && g != 0 && b != 0 && r == c.R && g == c.G && b == c.B)
                                {
                                    done = true;
                                }
                                else
                                {
                                    c = Color.FromArgb(r, g, b);
                                }
                            }
                        }
                    }
                    sp.Write(new byte[] { 0x00 }, 0, 1);
                    //System.Threading.Thread.Sleep(200);
                    s = sp.ReadLine();
                    sp.Close();
                }
            }
            Program.logIt($"read_color: -- ret={done}, color={c}");
            return new Tuple<bool, Color>(done, c);
        }
    }
}
