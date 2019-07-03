using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.IO;

namespace AviaGetPhoneSize
{
    class AviaGetPhoneColor
    {
        static SerialPort sp;
        static System.Collections.Concurrent.ConcurrentQueue<string> _queue = new System.Collections.Concurrent.ConcurrentQueue<string>();
        static int Main(string[] args)
        {
            int ret = 0;
            test();
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
