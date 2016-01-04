using Core.Commands;
using Core.Interfaces;
using Log;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class ManagerCore : IWorkCore, ICoreManager
    {
        private CompositionContainer _container;
        ILogger _log;

        [ImportMany(typeof(IWorkCore))]
        List<IWorkCore> _cores;

        [ImportMany(typeof(Command))]
        List<Command> _commands;

        public ManagerCore(ILogger log, string path)
        {
            _log = log;
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog(path));
            catalog.Catalogs.Add(new DirectoryCatalog(path+"\\Cores\\"));            
            _container = new CompositionContainer(catalog);

            try            
            {
                _container.ComposeExportedValue<ILogger>(_log);
                _container.ComposeExportedValue<ICoreManager>(this);
                this._container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                Console.WriteLine(compositionException.ToString());
            }

            Inizialize();            
        }

        public bool Inizialize()
        {
            try
            {
                loadConfig();

                List<Task> list = new List<Task>();                
                foreach (IWorkCore wc in _cores)
                {
                    Task new_t = new Task(new Action(() =>
                        {
                            try
                            {
                                wc.Inizialize();                                
                            }
                            catch (Exception e)
                            {
                                _log.Error(e.Message, wc.GetName()+".Inizialize()");
                            }

                            try
                            {
                                wc.loadConfig();
                            }
                            catch(Exception e)
                            {
                                _log.Error(e.Message, wc.GetName() + ".loadConfig()");
                            }

                            _log.Info("Core:" + wc.GetName()+" (v."+wc.Version+")");
                        }));
                    list.Add(new_t);
                }
                foreach (Task t in list)
                {
                    t.Start();                    
                }
                Task.WaitAll(list.ToArray());

            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
            
            return true;
        }

        public bool Start()
        {
            if (_cores == null)
                return false;

            Task[] t_array = new Task[_cores.Count];
            int idx = 0;
            foreach (IWorkCore wc in _cores)
            {
                t_array[idx++] = Task.Factory.StartNew<bool>(delegate() { return wc.Start(); });                
            }
            Task.WaitAll(t_array);

            _log.Info("WORKS STARTED");
            _log.Info("-------------");

            TimeEvent tm_evt = new TimeEvent(/*TimeEvent.timeEvent.timeEvent,*/ this, new TimeSpan(0, 0,10));
            tm_evt.OnEvent += tm_evt_OnEvent;
            _timer.Subscribe(tm_evt);

            while (true)
            {
                string expression = Console.ReadLine();
                bool exit = false;
                foreach (Command cmd in _commands)
                {
                    if (cmd.Name == expression.ToLower())
                    {
                        if (cmd.Execute() == false)
                        {
                            exit = true;
                            break;
                        }
                    }
                }

                if (exit == true)
                    break;
                //if (expression.ToLower() == "exit")
                   //break;
            }

            Stop();
            return true;
        }

        void tm_evt_OnEvent(object sender, TimeSpan time)
        {
            _log.Info("OnTime: " + time.ToString());
        }

        public void Stop()
        {
            Task[] t_array = new Task[_cores.Count];
            int idx = 0;
            foreach (IWorkCore wc in _cores)
            {
                t_array[idx++] = Task.Factory.StartNew(delegate() { wc.Stop(); });                
            }
            Task.WaitAll(t_array);
            _log.Info("CORES STOPED");
            _log.Info("-------------");
        }

        public void loadConfig()
        {
           
        }

        public void saveConfig()
        {
           
        }

        public string GetName()
        {
            return "ManagerCore";
        }

        public string Version
        {
            get
            {
                //AssemblyName ass_name = Assembly.GetExecutingAssembly().GetName();
                //return "ver. "+ass_name.Version.ToString();
                var attributes = Assembly.GetExecutingAssembly();
                return "ver." + attributes.FullName;
            }
        }

        [Import(typeof(TimerCore))]
        TimerCore _timer;

        public TimerCore Timer
        {
            get { return _timer; }
        }
    }
}
