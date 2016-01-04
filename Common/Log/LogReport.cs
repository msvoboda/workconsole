using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.ComponentModel.Composition;


namespace Log
{

    public static class LogInstance
    {
        static ILogger _log;
        static string fname;

        static LogInstance()
        {
            fname = Assembly.GetEntryAssembly().Location+".log";

        }

        public static ILogger Log 
        {
            get 
            {
                if (_log == null)
                {
                    _log = LogFileFactory.CreateLogFile(fname, true, true, true);
                }
                
                return _log; 
            } 
        }        
    }



    /// <summary>
    /// Log level pro logovani
    /// </summary>
    public enum LogLevel { Debug, Info, Warning, Error, FatalError, Custom }

    /// <summary>
    /// Globalni nastaveni pro log
    /// Nejvhodnejsi je nastavovat pres LogReport
    /// </summary>
    public static class LogOptions
    {        
        public static char Delimiter = '>';
        public static string[] Category = new string[] { "Debug", "Info", "Warning", "Error", "FatalError", "Custom" };
        public static string DateTimeMask = "[yyyy/MM/dd HH:mm:ss.fff]";
        public static string LogTime()
        {
           DateTime now = DateTime.Now;

           return now.ToString(DateTimeMask);
        }
    }

    public class LogTags : Dictionary<string, string>
    {
        public LogTags()
        {
        }

        public LogTags(Dictionary<string, string> tags)
        {
            foreach(KeyValuePair<string,string> kvp in tags)
            {
                Add(kvp.Key, kvp.Value);
            }
        }

        public override string ToString()
        {
            string line = "";
            bool first = true;
            foreach (KeyValuePair<string, string> kvp in this)
            {
                if (first == true)
                {
                    line = kvp.Key + "=" + kvp.Value;
                    first = false;
                }
                else
                {
                    line += ";"+kvp.Key + "=" + kvp.Value;
                }                
            }
            return line;
        }
    }

    /// <summary>
    /// Facada na logovani - rozhrani
    /// </summary>
    public interface ILogger
    {        
        bool Info(string message, string location="");
        bool Error(string message, string location="");
        bool Debug(string message, string location="");
        bool Warning(string message, string location = "");
        bool Write(string message, LogLevel cat = LogLevel.Info, string location = "",  Dictionary<string, string> tags=null);
        // logovaci funkce, ktera neni zavisla na 
        bool Log(string cat, string message, string location = "", LogTags tags = null);
        //bool Write(string message, LogLevel cat = LogLevel.Info, string location = "", LogTags tags = null);
        // kdyz jsem psal performanceconsole ... zjistil jsem, ze chybi tato funkce
        bool WriteString(string str);
        // funkce volane na zacatku a na konci ... inicializace a zruseni Start / Stop
        void Start();
        void Stop();
    }
    
    /// <summary>
    /// Trida na logovani - design pattern Facade
    /// pouzivam 2 fasady ... ILogger a ILoggerFacade pro prism ??? nevim, zda je nutne
    /// omezuje zavislost na konkretni implementaci
    /// </summary>
    public class LogReport : ILogger
    {
         public LogReport()
         {
             if (Loggers == null)
             {
                 Loggers = new List<ILogger>();
             }
            // NENI POTREBA POUZIVAT MEF ... pridavam rucne 
            /*                      
            var catalog = new AggregateCatalog();
            //var as_catalog = new AssemblyCatalog(typeof(LogReport).Assembly); 
             string dir_cat = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
             AssemblyCatalog call = new AssemblyCatalog(Assembly.GetEntryAssembly());
            catalog.Catalogs.Add(call);
            catalog.Catalogs.Add(new DirectoryCatalog(dir_cat));

            CompositionContainer container = new CompositionContainer(catalog);
            container.ComposeParts(this);             
            */ 
            
         }

         bool _info = true;
         bool _debug = true;
         bool _err = true;
         bool _warn = true;
         public void setWhatWrite(bool info, bool debug, bool err, bool warning)
         {
             _info = info;
             _debug = debug;
             _err = err;
             _warn = warning;
         }

         public void setOptions(char delimiter, string datemask)
         {
             LogOptions.Delimiter = delimiter;
             LogOptions.DateTimeMask = datemask;
         }

        /// <summary>
        /// Pridej logovac do hl. logovace
        /// </summary>
        /// <param name="logger">logovac</param> 
        public void AddLogger(ILogger logger)
        {
            Loggers.Add(logger);
        }

        public void RemoveLogger(ILogger logger)
        {
            Loggers.Remove(logger);
        }
        
        protected List<ILogger> Loggers 
        { get; private set; }

        /// <summary>
        /// Zaloguj info
        /// </summary>
        /// <param name="message">zprava</param>
        /// <param name="location">location</param>
        /// <returns></returns>
        public bool Info(string message, string location="")
        {
            if (_info == false)
                return true;

            return Write(message,LogLevel.Info, location);
        }

        /// <summary>
        /// Zaloguj chybu
        /// </summary>
        /// <param name="message">zprava</param>
        /// <param name="location">location</param>
        /// <returns></returns>
        public bool Error(string message, string location = "")
        {
            if (_err == false)
                return true;

            return Write(message, LogLevel.Error, location);
        }

        /// <summary>
        /// Warning
        /// </summary>
        /// <param name="message">zprava</param>
        /// <param name="location">location</param>
        /// <returns></returns>
        public bool Warning(string message, string location = "")
        {
            if (_warn == false)
                return false;

            return Write(message, LogLevel.Warning, location);
        }

        /// <summary>
        /// Debug
        /// </summary>
        /// <param name="message">zprava</param>
        /// <param name="location">location</param>
        /// <returns></returns>
        public bool Debug(string message, string location = "")
        {
            if (_debug == false)
                return true;

            return Write(message, LogLevel.Debug,location);
        }

        /// <summary>
        /// Zapis do logovacu
        /// </summary>
        /// <param name="message">zprava</param>
        /// <param name="cat">kategorie zpravy</param>
        /// <param name="location">location - kdo ypravu pise</param>
        /// <param name="tags">tagy</param>
        /// <returns></returns>
        public bool Write(string message, LogLevel cat=LogLevel.Info, string location="", Dictionary<string, string> tags=null)
        {
            bool ret = true;

            //Dictionary<string, string> stack = null;
            if (cat == LogLevel.Error)
            {
                StackTrace stackTrace = new StackTrace();           // get call stack
                StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)
                string calls = "";

                // write call stack method names
                int start = 5;
                if (start > stackFrames.Length - 1)
                {
                    start = stackFrames.Length - 1;
                }
                for (int i = start; i >= 0; i--)
                {
                    StackFrame stackFrame = stackFrames[i];      
                    MethodBase meth_base = stackFrame.GetMethod();
                    if (meth_base.DeclaringType != null && meth_base.DeclaringType != typeof(LogReport))
                    {
                        calls += meth_base.DeclaringType.ToString()+"."+meth_base.Name + "()";
                    }
                }

                /*
                //location+= ";CallStack:"+ calls;
                if (tags == null)
                    tags = new Dictionary<string, string>() { { "stack", calls } };
                else
                    tags.Add("stack", calls);
                 */
                message = message + LogOptions.Delimiter + calls;
            }

            if (Loggers == null)
            {
                return false;
            }

            foreach (var logger in Loggers)
            {
                if (logger.Write(message, cat, location,tags) == false)
                {
                    ret = false;
                }
            }

            return ret;
        }

        /// <summary>
        /// Zapise do logu text - bez dalsiho formatovani, bez dalsich pridavku
        /// </summary>
        /// <param name="str">string</param>
        /// <returns></returns>
        public bool WriteString(string str)
        {
            bool ret = true;
            if (Loggers == null)
            {
                return false;
            }

            foreach (var logger in Loggers)
            {
                if (logger.WriteString(str) == false)
                {
                    ret = false;
                }
            }

            return ret;
        }


        public bool Log(string level, string message, string location = "", LogTags tags = null)
        {
            bool ret = true;
            if (Loggers == null)
            {
                return false;
            }

            foreach (var logger in Loggers)
            {
                if (logger.Log(level,message,location,tags) == false)
                {
                    ret = false;
                }
            }

            return ret;
        }

        public void Start()
        {
            foreach (ILogger l in Loggers)
            {
                l.Start();
            }
        }

        public void Stop()
        {
            foreach (ILogger l in Loggers)
            {
                l.Stop();
            }
        }
    }

    public class LogFileFactory
    {
        private static LogFile _log = null;

        public static LogFile CreateLogFile(string fname, bool info, bool debug, bool err)
        {
            LogFile lf = new LogFile();
            lf.initLog(fname);
            lf.setWhatWrite(info, debug, err);
            _log = lf;
            return (LogFile)lf;
        }

        public static LogThreadFile CreateThreadLogFile(string fname)
        {
            LogThreadFile lf = new LogThreadFile();
            lf.initLog(fname);                      
            return (LogThreadFile)lf;
        }

        public static LogFile GetLog()
        {
            if (_log != null)
                return _log;

            Debug.Assert(_log != null, "LogFile was not set. LogFile is null");
            //throw new Exception("LogFile was not set. LogFile is null");
            return null;
        }

        public static string ExceptionToString(Exception e)
        {
            StringBuilder str = new StringBuilder();

            str.AppendLine(e.Message);
            if (e.InnerException != null)
            {
                str.AppendLine("; InnerException: " + e.InnerException.Message);
            }

            return str.ToString();
        }   
    }
    
    public class LogFile : ILogger
    {
        private string m_path = null;
          
        private bool m_write_info = true;
        private bool m_write_debug = true;
        private bool m_write_error = true;
        private StringBuilder _not_writed = new StringBuilder();
               
        public void initLog(string fname)
        {            
            m_path = fname;                        
        }

        public void setWhatWrite(bool info, bool debug, bool err)
        {
            m_write_info = info;
            m_write_debug = debug;
            m_write_error = err;
        }

        public void Start()
        {
            
        }

        public void Stop()
        {

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

        private static string MethodToString(MethodBase metod)
        {
            string str = metod.Name+"()";
            return str;
        }

        public bool Log(string cat, string message, string location = "", LogTags tags = null)
        {
            string headLine = LogOptions.LogTime();
            if (location.Length > 0)
            {
                headLine += location + LogOptions.Delimiter;
            }

            headLine += cat.ToString() + LogOptions.Delimiter;

            /*
            if (cat == LogLevel.Error.ToString() && tags != null && tags.ContainsKey("stack") == true)
            {
                message += LogOptions.Delimiter + tags["stack"];
            }*/

            try
            {
                FileStream fs = new FileStream(m_path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                fs.Seek(0, SeekOrigin.End);
                StreamWriter swFromFileStream = new StreamWriter(fs);
                lock (_not_writed)
                {
                    if (_not_writed.Length > 0)
                    {
                        swFromFileStream.WriteLine(_not_writed.ToString());
                        _not_writed.Clear();
                    }
                }

                if (tags == null || tags.Count == 0)
                    swFromFileStream.WriteLine(headLine + message);
                else
                    swFromFileStream.WriteLine(headLine + message + LogOptions.Delimiter + tags.ToString());

                swFromFileStream.Flush();
                swFromFileStream.Close();
                fs.Close();
                fs.Dispose();
                swFromFileStream.Dispose();
            }
            catch (IOException)
            {
                lock (_not_writed)
                {
                    _not_writed.Append(headLine + message);
                    System.Diagnostics.Debug.WriteLine("Can't write into Log:" + _not_writed.ToString());
                    if (_not_writed.Length > 4096)
                    {
                        _not_writed.AppendLine("Can't write into Log. Length=" + _not_writed.Length);
                        System.Diagnostics.Debug.WriteLine("Can't write into Log. Length=" + _not_writed.Length);
                    }
                }
                return false;
            }

            return true;
        }

        public bool Write(string line, LogLevel cat, string location, Dictionary<string, string> tags=null)
        {

            if (cat == LogLevel.Info && m_write_info == false)
                return true;
            if (cat == LogLevel.Error && m_write_error == false)
                return true;
            if (cat == LogLevel.Debug && m_write_debug == false)
                return true;

           
            if (tags != null)
                return Log(cat.ToString(), line, location, new LogTags(tags));
            else
                return Log(cat.ToString(), line, location);
        }

        

        /// <summary>
        /// Zapisuju str do souboru, nepridavame zadne dalsi informace
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool WriteString(string str)
        {
            try
            {
                FileStream fs = new FileStream(m_path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                fs.Seek(0, SeekOrigin.End);
                StreamWriter swFromFileStream = new StreamWriter(fs);
                lock (_not_writed)
                {
                    if (_not_writed.Length > 0)
                    {
                        swFromFileStream.WriteLine(_not_writed.ToString());
                        _not_writed.Clear();
                    }
                }
                swFromFileStream.WriteLine(str);
                swFromFileStream.Flush();
                swFromFileStream.Close();
                fs.Close();
                fs.Dispose();
                swFromFileStream.Dispose();
            }
            catch (IOException)
            {
                lock (_not_writed)
                {
                    _not_writed.Append(str);
                    System.Diagnostics.Debug.WriteLine("Can't write into Log:" + _not_writed.ToString());
                    if (_not_writed.Length > 4096)
                    {
                        _not_writed.AppendLine("Can't write into Log. Length=" + _not_writed.Length);
                        System.Diagnostics.Debug.WriteLine("Can't write into Log. Length=" + _not_writed.Length);
                    }
                }
                return false;
            }

            return true;
        }
    }

    [Export(typeof(ILogger))]
    public class LogDebug : ILogger
    {
        public bool Info(string message, string location)
        {
            return Write(message, LogLevel.Info);
        }

        public bool Error(string message, string location)
        {
            return Write(message, LogLevel.Error);
        }

        public bool Debug(string message, string location)
        {
            return Write(message, LogLevel.Debug);
        }

        public bool Warning(string message, string location = "")
        {
            return Write(message, LogLevel.Warning, location);
        }

        public bool Write(string message, LogLevel cat, string location="", Dictionary<string, string> tags=null)
        {
            System.Diagnostics.Debug.WriteLine(message);
            //Console.WriteLine(message);
            return true;
        }

        public bool WriteString(string str)
        {
            System.Diagnostics.Debug.WriteLine(str);
            //Console.WriteLine(message);
            return true;
        }

        public bool Log(string level, string message, string location = "", LogTags tags = null)
        {
            System.Diagnostics.Debug.WriteLine(level+LogOptions.Delimiter+message);
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
