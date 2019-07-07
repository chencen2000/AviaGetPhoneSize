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

namespace AviaGetPhoneSize
{
    class Program
    {
        public static void logIt(string msg)
        {
            System.Diagnostics.Trace.WriteLine(msg);
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

            return ret;
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
