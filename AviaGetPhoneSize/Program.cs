using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AviaGetPhoneSize
{
    class Program
    {
        public static void logIt(string msg)
        {
            System.Diagnostics.Trace.WriteLine(msg);
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
            if (_args.IsParameterTrue("wait"))
            {
                Tuple<bool, int> res = run_debug("wait");
                if (res.Item1)
                {
                    ret = res.Item2;
                }
                else
                {
                    // do real work
                }
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
    }
}
