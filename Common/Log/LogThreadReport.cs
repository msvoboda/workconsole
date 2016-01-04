using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Log;

namespace Log
{
   
    
    public class LogThreadFile : ILogger
    {
        string m_path = null;
        Thread _thread;
        bool started = false;
        ConcurrentQueue<string> _data = new ConcurrentQueue<string>();
        AutoResetEvent _auto = new AutoResetEvent(false);

        long total_count=0;
        long sample_count=0;

        public LogThreadFile()
        {

        }

        public void initLog(string fname)
        {
            m_path = fname;
        }

        public void closeLog()
        {
            started = false;
            _auto.Set();
        }

        public void Start()
        {
            ThreadStart th_start = new ThreadStart(this.Execute);
            _thread = new Thread(th_start);
            _thread.Start();
        }

        public void Stop()
        {
            System.Diagnostics.Debug.WriteLine(String.Format("Average queue length:{0}",(float)total_count/(float)sample_count));
            closeLog();
        }

        public void Execute()
        {
            started = true;

            while (_auto.WaitOne()&&started ==true)
            {
                total_count += _data.Count;
                sample_count++;
                string data = "";
                using (FileStream fs = new FileStream(m_path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                {
                    fs.Seek(0, SeekOrigin.End);
                    using (StreamWriter swFromFileStream = new StreamWriter(fs))
                    {
                        while (_data.TryDequeue(out data))
                        {
                            swFromFileStream.WriteLine(data);
                            swFromFileStream.Flush();
                        }
                    }
                }                
            }
        }

        public bool Write(string line, LogLevel cat)        
        {
            return Write(line, cat, "");
        }

        public bool Info(string line, string location="")
        {
            return Write(line, LogLevel.Info, "");
        }

        public bool Info(string line, MethodBase method)
        {
            return Write(line, LogLevel.Info, MethodToString(method));
        }

        /// <summary>
        /// Zapis Debug hlasky do logu
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public bool Debug(string line, string location="")
        {            
            return Write(line, LogLevel.Debug, "");
        }

        public bool Debug(string line, MethodBase method)
        {
            return Write(line, LogLevel.Debug, MethodToString(method));
        }

        public bool Warning(string message, string location = "")
        {
            return Write(message, LogLevel.Warning, location);
        }

        /// <summary>
        /// Zapis Error hlasky do logu
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public bool Error(string line, string location="")
        {
            return Write(line, LogLevel.Error, "");
        }

        public bool Error(string line, MethodBase method)
        {
            return Write(line, LogLevel.Error, MethodToString(method));
        }

        public bool Log(string level, string messgage, string location = "", LogTags tags = null)
        {
            return true;
        }

        private static string MethodToString(MethodBase metod)
        {
            string str = metod.Name+"()";
            return str;
        }

        public bool Write(string line, LogLevel cat = LogLevel.Info, string location = "", Dictionary<string, string> tags = null)
        {
            string headLine = LogOptions.LogTime();
            if (location.Length > 0)
            {
                headLine += location + LogOptions.Delimiter;
            }

            headLine += cat.ToString() + LogOptions.Delimiter;

            if (cat == LogLevel.Error && tags != null && tags.ContainsKey("stack") == true)
            {
                line += LogOptions.Delimiter + tags["stack"];
            }

            try
            {
                _data.Enqueue(headLine + line);
                _auto.Set();
            }
            catch (Exception)
            {
                return false;
            }

            return true;            
        }

        public bool WriteString(string str)
        {
            try
            {
                _data.Enqueue(str);
                _auto.Set();
            }
            catch
            {
                return false;
            }

            return true;
        }

    }
}
