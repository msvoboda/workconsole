using Common.Log;
using Core;
using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WorkConsole
{
    class Program
    {
        static ManagerCore _manager;

        static void Main(string[] args)
        {
            LogReport report = new LogReport();
            report.AddLogger(LogInstance.Log);

            LogConsole console = new LogConsole();
            report.AddLogger(console);
            report.Info("Starting cores");
            report.Info("----------------");
            string cores_dir= new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName;


            _manager = new ManagerCore(report, cores_dir);
            _manager.Start();
            /// inizialize
            /// start
            /// work            
        }
    }
}
