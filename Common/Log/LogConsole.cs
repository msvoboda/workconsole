using Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Log
{
    public class LogConsole : ILogger
    {
        public bool Info(string message, string location = "")
        {
            return Log(LogLevel.Info.ToString(), message, location);
        }

        public bool Error(string message, string location = "")
        {
            Console.ForegroundColor = ConsoleColor.Red;
            return Log(LogLevel.Error.ToString(), message, location);
        }

        public bool Debug(string message, string location = "")
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            return Log(LogLevel.Debug.ToString(), message, location);
        }

        public bool Warning(string message, string location = "")
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            return Log(LogLevel.Warning.ToString(), message, location);
        }

        public bool Write(string message, LogLevel cat = LogLevel.Info, string location = "", Dictionary<string, string> tags = null)
        {
            string headLine = LogOptions.LogTime();
            if (location.Length > 0)
            {
                headLine += location + LogOptions.Delimiter;
            }

            if (cat == LogLevel.Info)
                Console.ForegroundColor = ConsoleColor.White;
            else if (cat == LogLevel.Error)
                Console.ForegroundColor = ConsoleColor.Red;
            else if (cat == LogLevel.Warning)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else if (cat == LogLevel.FatalError)
                Console.ForegroundColor = ConsoleColor.DarkRed;
            else if (cat == LogLevel.Debug)
                Console.ForegroundColor = ConsoleColor.Cyan;

            headLine += cat.ToString() + LogOptions.Delimiter;

            if (tags == null || tags.Count == 0)
                Console.WriteLine(headLine + message);
            else
                Console.WriteLine(headLine + message + LogOptions.Delimiter + tags.ToString());            
            return true;
        }

        public bool Log(string cat, string message, string location = "", LogTags tags = null)
        {
            string headLine = LogOptions.LogTime();
            if (location.Length > 0)
            {
                headLine += location + LogOptions.Delimiter;
            }

            headLine += cat.ToString() + LogOptions.Delimiter;

            if (tags == null || tags.Count == 0)
                Console.WriteLine(headLine + message);
            else
                Console.WriteLine(headLine + message + LogOptions.Delimiter + tags.ToString());

            Console.ForegroundColor = ConsoleColor.White;
            return true;
        }

        public bool WriteString(string str)
        {
            Console.WriteLine(str);
            return true;
        }

        public void Start()
        {
            

        }

        public void Stop()
        {
           
        }
    }
}
