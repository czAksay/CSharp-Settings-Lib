using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SettingsLib;
using System.IO;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Settings st = new Settings("Set.txt", true, "=", "\r\n");
        }
    }
}
