using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
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


    }
}
