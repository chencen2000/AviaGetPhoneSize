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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace AviaGetPhoneSize
{
    class Program
    {
        static String eventName = "DEVICEMONITOREVENT";
        static String TAG = "[AviaGetPhoneSize]";
        static Dictionary<string, object> avia_config = null;
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
        #region load data from config
        public static Rectangle config_load_rectangle(Dictionary<string,object> config, string key)
        {
            Rectangle ret = new Rectangle(744, 266, 540, 1116);
            if (string.Compare(key, "rectangle2", false) == 0)
                ret = new Rectangle(375, 450, 30, 200);
            if (config.ContainsKey(key) && config[key] != null && config[key].GetType() == typeof(Dictionary<string, object>))
            {
                Dictionary<string, object> rect = (Dictionary<string, object>)config[key];
                int x = -1;
                int y = -1;
                int w = -1;
                int h = -1;
                if(rect.ContainsKey("x") && rect["x"]!=null && rect["x"].GetType() == typeof(int))
                {
                    x = (int)rect["x"];
                }
                if (rect.ContainsKey("y") && rect["y"] != null && rect["y"].GetType() == typeof(int))
                {
                    y = (int)rect["y"];
                }
                if (rect.ContainsKey("w") && rect["w"] != null && rect["w"].GetType() == typeof(int))
                {
                    w = (int)rect["w"];
                }
                if (rect.ContainsKey("h") && rect["h"] != null && rect["h"].GetType() == typeof(int))
                {
                    h = (int)rect["h"];
                }
                if (x >= 0 && y >= 0 && w > 0 && h > 0)
                    ret = new Rectangle(x, y, w, h);
            }
            return ret;
        }
        public static Dictionary<string, object> loadConfig(string name)
        {
            Dictionary<string, object> ret = null;
            loadConfig();
            if (avia_config.ContainsKey(name))
            {
                try { ret = (Dictionary<string, object>)avia_config[name]; }
                catch (Exception) { }
            }
            //else
            //{
            //    ret = avia_config;
            //}
            return ret;
        }
        public static Dictionary<string,object> loadConfig()
        {            
#if false
            string root = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "AVIA", System.Environment.MachineName);
            if (System.IO.Directory.Exists(root))
            {
                try
                {
                    string fn = System.IO.Path.Combine(root, "avia_config.json");
                    var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                    ret = jss.Deserialize<Dictionary<string, object>>(System.IO.File.ReadAllText(fn));
                }
                catch (Exception) { }
                ret["root"] = root;
            }
            else
                ret["root"] = System.IO.Path.GetDirectoryName(root);
#else
            // load avia config
            if (avia_config == null)
            {
                Dictionary<string, object> ret = new Dictionary<string, object>();
                string s = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "AVIA", "avia_config.json");
                Dictionary<string, object> m00 = null;
                // load main config
                if (System.IO.File.Exists(s))
                {
                    try
                    {
                        var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                        m00 = jss.Deserialize<Dictionary<string, object>>(System.IO.File.ReadAllText(s));
                    }
                    catch (Exception) { }
                }
                // load each machines
                if (m00 != null && m00.ContainsKey("machines") && m00["machines"] != null && m00["machines"].GetType() == typeof(Dictionary<string, object>))
                {
                    ret.Add("main", m00);
                    if (m00.ContainsKey("machines") && m00["machines"] != null && m00["machines"].GetType() == typeof(Dictionary<string, object>))
                    {
                        Dictionary<string, object> ms = (Dictionary<string, object>)m00["machines"];
                        foreach (KeyValuePair<string, object> kvp in ms)
                        {
                            s = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "AVIA", kvp.Value.ToString(), "avia_config.json");
                            if (System.IO.File.Exists(s))
                            {
                                try
                                {
                                    var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                                    Dictionary<string, object> m = jss.Deserialize<Dictionary<string, object>>(System.IO.File.ReadAllText(s));
                                    m["root"] = System.IO.Path.GetDirectoryName(s);
                                    ret[kvp.Key] = m;
                                }
                                catch (Exception) { }
                            }
                        }
                    }
                    avia_config = ret;
                }
            }
#endif
            return avia_config;
        }
        #endregion

        [STAThread]
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
            else if (_args.IsParameterTrue("gentemp"))
            {
                AviaGetPhoneModel.save_template_image(_args.Parameters["dir"]);
            }
            else
            {
                Application.EnableVisualStyles();
                //Application.Run(new Form1());
                //Application.Run(new Form2());
                Application.Run(new Form3());
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
            string image_filename = string.Empty;
            if (args.ContainsKey("img") && System.IO.File.Exists(args["img"]))
                image_filename = args["img"];
            else
            {
                utility.IniFile ini = new utility.IniFile(System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "AVIA", "AviaDevice.ini"));
                string fn = ini.GetString("query", "filename", "");
                string fn1 = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "AVIA", $"{fn}.bmp");
                if (save_image_file(fn, fn1))
                    image_filename = fn1;
            }
            if (!string.IsNullOrEmpty(image_filename) && System.IO.File.Exists(image_filename))
            {
                // check model by images
                //Console.WriteLine("model=iphone8 plus red_M2_N");
                ret = AviaGetPhoneModel.start(image_filename);
            }
            return ret;
        }
        public static string md5(string filename)
        {
            string ret = string.Empty;
            if (System.IO.File.Exists(filename))
            {
                using (var md5 = MD5.Create())
                {
                    Byte[] h = md5.ComputeHash(System.IO.File.ReadAllBytes(filename));
                    ret = BitConverter.ToString(h);
                }
            }
            return ret;
        }
    }
}
