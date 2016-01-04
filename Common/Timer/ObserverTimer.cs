using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Log;

namespace ModuleManager.Core.Timer
{
    public delegate void PerformanceTimeNotify(object sender, TimeSpan span);

    /// <summary>
    /// Performance timer - postaveny na Stopwatch based on QueryPerformanceTimer
    /// cervenec 2013
    /// </summary>
    public class ObserverTimer
    {
        Thread _thread;
        ILogger _log;
        private bool _autoEventStop = false;
        private bool _run;
        private bool _pause=false;
        private string _name = "";

        Stopwatch _watch = new Stopwatch();
        //
        public event PerformanceTimeNotify OnEvent;
        public event PerformanceTimeNotify OnTick;

        public ObserverTimer(bool autostop=false)
        {
            _autoEventStop = autostop;
        }

        public ObserverTimer(ILogger log, bool autostop = false)
        {
            _log = log;
            _autoEventStop = autostop;
            if (Stopwatch.Frequency == 0)
            {
                if (_log != null)
                    _log.Warning("QueryPerformanceCounter won't work! Frequence=" + Stopwatch.Frequency);
            }
        }

        TimeSpan _oldTime=new TimeSpan();
        private const short _sleep=500;
        private void Tick()        
        {
            if (_watch.ElapsedMilliseconds > 0)
            {
                _watch.Reset();
            }
            _watch.Start();
            if (_log != null)
            {
                _log.Info("Elapsed after start:" +_watch.Elapsed.ToString());
            }
            while (_run)            
            {
                //_log.Warning("Tick");
                try
                {
                    if (_pause == true)
                    {

                        _oldTime = _watch.Elapsed;              
                        Thread.Sleep(_sleep);
                        continue;
                    }

                    TimeSpan now = _watch.Elapsed;
                    TimeSpan rozdil = now - _oldTime;
                    if (rozdil.TotalMilliseconds > (_sleep * 2.5))
                    {                        
                        if (_log != null)
                        {
                            _log.Warning("LongTick:" + rozdil);
                        }                        
                    }
                    else
                    {
                        //rozdil = new TimeSpan();
                    }

                    /*
                    if (_log != null)
                    {
                        _log.Info(string.Format("Now:{0} Rozdil:{1}", now, rozdil));
                    }*/

                    if (OnTick != null)
                    {
                        OnTick(this, now);
                    }

                    _oldTime = _watch.Elapsed;                    
                    Thread.Sleep(_sleep);               
                }
                catch (Exception e)
                {
                    if (_log != null)
                    {
                        _log.Error(e.Message);
                    }
                }
            }            
        }

        public TimeSpan GetElapsed()
        {
            return _watch.Elapsed;                
        }

        static int cislo = 1;
        public void Start(TimeSpan event_time)
        {

            try
            {
                string name = "PerformanceTimer" + cislo++;
                if (_log != null)
                {
                    _log.Info("StartTimer:" + name + ">" + event_time);                    
                }
               
                _run = true;
                if (_thread == null)
                {
                    _pause = false;
                    _thread = new Thread(Tick);
                    _thread.Name = name;
                    _name = _thread.Name;
                    _thread.Start();                    
                    _oldTime = _watch.Elapsed;
                }
                else
                {
                    Restart();
                    _pause = false;
                }
            }
            catch (Exception e)
            {
                if (_log != null)
                    _log.Error(e.Message);
            }
        }



        public void Restart()
        {
            _watch.Restart();
            //if (_log != null)
            //  _log.Info("Restart time:" + _watch.Elapsed.ToString()); 
            _oldTime = _watch.Elapsed;
        }

        public void Pause()
        {
            _pause = true;
        }

        public void Stop()
        {
            _watch.Stop();
            if (_log != null)
                _log.Info("Stop time:"+_watch.Elapsed.ToString());            
            _run = false;
            _thread = null;

        }
    }
}
