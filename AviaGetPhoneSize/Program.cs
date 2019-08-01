using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AviaGetPhoneSize
{
    class Program
    {
        static String eventName = "DEVICEMONITOREVENT";
        static String TAG = "[AviaGetPhoneSize]";
        public static void logIt(string msg)
        {
            System.Diagnostics.Trace.WriteLine($"{TAG}: {msg}");
        }
        public static string getCurrentExeFilename()
        {
            return System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        }
        public static XmlDocument getDebugOutputXml()
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                string fn = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(getCurrentExeFilename()), "AviaGetPhoneSizeDebugOutput.xml");
                if (System.IO.File.Exists(fn))
                {
                    doc.Load(fn);
                }
            }
            catch (Exception) { }
            return doc;
        }
        static int Main(string[] args)
        {
             

            int ret = 0;
            System.Configuration.Install.InstallContext _args = new System.Configuration.Install.InstallContext(null, args);
            if (_args.IsParameterTrue("debug"))
            {
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            if (_args.IsParameterTrue("QueryISP"))
            {
                //Tuple<bool, int> res = run_debug("QueryISP");
                //if (res.Item1)
                //{
                //    ret = res.Item2;
                //}
                //else
                //{
                //    // do real work
                //}
                handle_QueryISP_Command(_args.Parameters);
            }
            else if (_args.IsParameterTrue("QueryPMP"))
            {
                //
                ret = handle_QueryPMP_Command(_args.Parameters);
            }
            else if (_args.IsParameterTrue("detect"))
            {
                Tuple<bool, int> res = run_debug("detect");
                if (res.Item1)
                {
                    ret = res.Item2;
                }
                else
                {
                    // do real work
                }
            }
            else if (_args.IsParameterTrue("QueryFrame"))
            {
                // test
                TcpClient client = new TcpClient();
                try
                {
                    string root = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "AVIA", "frames");
                    client.Connect(IPAddress.Loopback, 6280);
                    NetworkStream ns = client.GetStream();
                    byte[] cmd = System.Text.Encoding.UTF8.GetBytes("QueryFrame\n");
                    byte[] data = new byte[1024];
                    DateTime _start = DateTime.Now;
                    while ((DateTime.Now - _start).TotalSeconds < 10)
                    {
                        System.Threading.Thread.Sleep(500);
                        ns.Write(cmd, 0, cmd.Length);
                        int read = ns.Read(data, 0, data.Length);
                    }
                }
                catch (Exception) { }
            }
            else
            {
                Program.logIt($"{System.Environment.Is64BitProcess}");
            }
            return ret;
        }

        static Tuple<bool,int> run_debug(string command)
        {
            bool ret = false;
            int i = -1;
            XmlDocument doc = getDebugOutputXml();
            if (doc.DocumentElement != null)
            {
                ret = true;
                XmlNode node = doc.DocumentElement[command];
                if (node != null)
                {
                    try
                    {
                        i = Int32.Parse(node["exitcode"]?.InnerText);
                        foreach(XmlNode n in node["stdout"]?.ChildNodes)
                        {
                            System.Console.WriteLine(n.InnerText);
                        }
                    }
                    catch (Exception) { }
                }
            }
            return new Tuple<bool, int>(ret, i);
        }
        static public Dictionary<string, object> parse_hocr_title(string title)
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            // title='bbox 666 157 1597 494; x_wconf 75'
            string[] ss = title.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach(string s in ss)
            {
                string[] sss = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string k = sss[0];
                if (string.Compare(k, "bbox") == 0)
                {
                    int x = Int32.Parse(sss[1]);
                    int y = Int32.Parse(sss[2]);
                    int x1 = Int32.Parse(sss[3]);
                    int y1 = Int32.Parse(sss[4]);
                    Rectangle r = new Rectangle(x, y, x1 - x, y1 - y);
                    ret.Add("bbox", r);
                }
                else if (string.Compare(k, "x_wconf")==0)
                {
                    double d = double.Parse(sss[1]);
                    ret.Add("x_wconf", d);
                }
            }
            return ret;
        }
        static public Dictionary<string, object>[] parse_hocr_result(string hocr)
        {
            List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();
            var doc = new HtmlDocument();
            doc.LoadHtml(hocr);
            var words = doc.DocumentNode.SelectNodes(@"//span[@class='ocrx_word']");
            foreach(var w in words)
            {
                string s = w.Attributes?["title"]?.Value;
                Dictionary<string, object> wd = parse_hocr_title(s);
                s = w.Attributes?["id"]?.Value;
                wd.Add("id", s);
                wd.Add("text", w.InnerText);
                ret.Add(wd);
            }
            return ret.ToArray();
        }
        static void handle_QueryISP_Command(System.Collections.Specialized.StringDictionary args)
        {
            if (args.ContainsKey("start-service"))
            {
                bool own;
                System.Threading.EventWaitHandle e = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset, eventName, out own);
                if (own)
                {
                    AVIAGetPhoneSize.start(e);
                }
                else
                {
                    // device monitor already started.
                }
            }
            else if (args.ContainsKey("kill-service"))
            {
                try
                {
                    System.Threading.EventWaitHandle e = System.Threading.EventWaitHandle.OpenExisting(eventName);
                    e.Set();
                }
                catch (Exception) { }
            }
        }
        static bool save_image_file(string fn, string target)
        {
            bool ret = false;
            Process p1 = new Process();
            p1.StartInfo.FileName = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "AVIA", "FDPhoneRecognition.exe");
            p1.StartInfo.Arguments = $"-mmi={fn} -output={target}";
            p1.StartInfo.UseShellExecute = false;
            p1.StartInfo.CreateNoWindow = true;
            p1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p1.Start();
            p1.WaitForExit();
            if (System.IO.File.Exists(target))
                ret = true;
            return ret;
        }
        static int handle_QueryPMP_Command(System.Collections.Specialized.StringDictionary args)
        {
            int ret = -1;
            utility.IniFile ini = new utility.IniFile(System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "AVIA", "AviaDevice.ini"));
            string fn = ini.GetString("query", "filename", "");
            string fn1 = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "AVIA", $"{fn}.bmp");
            if (!string.IsNullOrEmpty(fn) && save_image_file(fn, fn1))
            {
                // check model by images
                //Console.WriteLine("model=iphone8 plus red_M2_N");
                ret = AviaGetPhoneModel.start(fn1);
            }
            return ret;
        }
    }
}
