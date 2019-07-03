using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Text.RegularExpressions;

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
                                    System.Console.WriteLine($"t={m.Groups[1].Value}, l={m.Groups[2].Value}, r={m.Groups[3].Value}, g={m.Groups[4].Value}, b={m.Groups[5].Value}, c={m.Groups[6].Value}");
                                }                                
                            }
                        }
                    }
                });

                System.Console.WriteLine("press any key to termine.");
                System.Console.ReadKey();
                sp.Close();
            }
        }
    }
}
