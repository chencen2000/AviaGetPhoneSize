using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviaGetPhoneSize
{
    class Program
    {
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
