using Core.Interfaces;
using Log;
using ModuleManager.Core.Timer;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public delegate void TimeDelegate(object sender, TimeSpan time);

    public class DateEvent : IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class TimeEvent : IDisposable
    {

        public TimeEvent(/*timeEvent t,*/ IWorkCore owner, TimeSpan event_time, bool snap_to = false, bool is_async=false)
        {
            //Type = t;
            Owner = owner;
            EventTime = event_time;
            SnapToClock = snap_to;
            LastEvent = new TimeSpan();            
        }


        //public enum timeEvent { timeRepeat, timeEvent };
        //public timeEvent Type { get; set; }
        public IWorkCore Owner {get; internal set;}
        public TimeSpan EventTime { get; internal set; }        
        public bool SnapToClock { get; internal set; }
        public bool IsAsyncEvent { get; internal set; }

        TimeSpan _lastEvent;
        public TimeSpan LastEvent 
        {
            get { return _lastEvent; }
            internal set 
            {
                _lastEvent = value;
                if (SnapToClock == false)
                {
                    NextEvent = _lastEvent + EventTime;
                }
                else
                {
                    // snap
                }
            }
        }

        public TimeSpan NextEvent { get; internal set; }
        public event TimeDelegate OnEvent;

        public void DoEvent(TimeSpan time)
        {
            if (OnEvent != null)
            {
                OnEvent(this, time);
            }
        }

        public void Dispose()
        {
 	
        }
    }

    /// <summary>
    /// Timer - design patterns Observer
    /// </summary>
    [Export(typeof(IWorkCore))]
    [Export(typeof(TimerCore))]
    public class TimerCore : ObserverTimer, IWorkCore
    {        
        int timer_interval = 500;

        [Import(typeof(ILogger))]
        ILogger _log;

        [Import(typeof(ICoreManager))]
        ICoreManager _core;

        public bool Inizialize()
        {
            base.OnTick += TimerCore_OnTick;            
            return true;
        }

        public bool Start()
        {

            base.Start(new TimeSpan(0,0,0,0,timer_interval));
            return true;
        }

        public void Stop()
        {
            base.Stop();
        }

        public void loadConfig()
        {
            string val = ConfigurationManager.AppSettings["timer"];
            timer_interval = Convert.ToInt32(val);
        }

        public void saveConfig()
        {
            
        }

        public string GetName()
        {
            return "Timer";
        }

        void TimerCore_OnTick(object sender, TimeSpan span)
        {
                    DateTime now = DateTime.Now;
            
            //var vyber = from TimeEvent evt in _times select evt;
            foreach (object udalost in _times)
            {
                if (udalost is TimeEvent && span >= ((TimeEvent)udalost).NextEvent && ((TimeEvent)udalost).SnapToClock == false) 
                {
                    TimeEvent time_ud = (TimeEvent)udalost;
                    time_ud.LastEvent = span;                    
                    // obesli event
                    // delej neco
                    /// ... working           
                    if (time_ud.IsAsyncEvent == false)
                    {
                        time_ud.DoEvent(span);
                        time_ud.LastEvent = GetElapsed();
                    }
                    else
                    {
                        Task t = new Task(new Action(() =>
                            {
                                time_ud.DoEvent(span);                                
                            }));
                        t.Start();
                    }
                }
                else if (udalost is TimeEvent && ((TimeEvent)udalost).SnapToClock == true) 
                {
                    TimeEvent evt = (TimeEvent)udalost;
                    DateTime dt = DateTime.Now;
                    if (dt.Minute == evt.LastEvent.Minutes && dt.Hour != evt.LastEvent.Hours)
                    {
                        evt.LastEvent = dt.TimeOfDay;
                        if (evt.IsAsyncEvent == false)
                        {
                            evt.DoEvent(dt.TimeOfDay);
                        }
                        else
                        {
                            Task t = new Task(new Action(() =>
                            {
                                evt.DoEvent(dt.TimeOfDay);
                            }));
                            t.Start();
                        }                                               
                    }
                    
                }
                else if (udalost is DateEvent)
                {

                }
            }            
        }

        List<object> _times=new List<object>();
        public void Subscribe(object evt)
        {
            _times.Add(evt);                        
        }

        public void Unsubscribe(TimeEvent evt)
        {
            _times.Remove(evt);
        }

        public string Version
        {
            get
            {
                var ass = Assembly.GetExecutingAssembly();
                var attributes = ass.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false).Cast<AssemblyFileVersionAttribute>();
                var versionAttribute = attributes.Single();
                return versionAttribute.Version.ToString();
            }
        }
    }
}
