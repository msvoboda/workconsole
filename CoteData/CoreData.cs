using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using PgSql;
using PgSql.Config;
using System.ServiceModel.Channels;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Threading.Tasks;
using System.Threading;
using Core.Interfaces;
using Log;
using System.Reflection;

// Jedna se o spolecnou cast - spolecne sdilene data a prostredky pro vsechny ostatni
// Melo by to byt soucasti serveru - lec bohuzel nemam pristup do serveru. musim se tam dostat jako CORE !!!
//
// Instalovan package - Microsoft ASP.NET MVC4
// Instalovan package - Microsoft ASP.NET WebAPI SelfHost
// Instalovan package - Microsoft ASP.NET WebAPI WebHost ???
namespace ServerData
{        
    [Export(typeof(IWorkCore))]
    public class CoreData : IWorkCore
    {
        private static bool _init = false;
        private static CartmasterConfiguration _cmConfig = null;
        private static PgSqlManager _pgManager = null;
        private static RadioData _radio = null;
        private string _api_port = "";

        [Import(typeof(ILogger))]
        private ILogger _log;

        private static ILogger _staticlog;
        public static ILogger Log
        {
            get
            {
                return _staticlog;
            }
        }

        public string GetName()
        {
            return "CoreAPI";
        }
                
        public static ICmConfiguration CmConfig
        {
            get { return _cmConfig; }
            private set { _cmConfig = (CartmasterConfiguration)value; }
        }

        
        public static PgSqlManager Database
        {
            get { return _pgManager; }
            private set { _pgManager = value; }
        }

        public static RadioData Radio
        {
            get { return _radio; }
        }

        public bool InitCore()
        {            
            _cmConfig = new CartmasterConfiguration();                
            return true;
        }

        CancellationTokenSource cancelToken = null;
        private void InitWebAPI(string port)
        {            
            try
            {
                _staticlog = _log;

                //SingletonService.Service = this;
                string url = "http://localhost:"+port+"/";
                var config = new HttpSelfHostConfiguration(url);

                config.Routes.MapHttpRoute(
                name: "default",
                routeTemplate: "{controller}/{action}/{id}",
                defaults: new
                {
                    id = System.Web.Http.RouteParameter.Optional,
                    controller = "Radio",
                    action = "Get"
                });

                cancelToken = new CancellationTokenSource();
                using (HttpSelfHostServer server = new HttpSelfHostServer(config))
                {
                    _log.Info("WebAPI Started to url:" + url);
                    Task web = server.OpenAsync();
                    while (true)
                    {
                        web.Wait();
                        //web.Start(                                                        
                        if (cancelToken.IsCancellationRequested == true)
                        {
                            cancelToken.Token.ThrowIfCancellationRequested();
                            break;
                        }
                        System.Threading.Thread.Sleep(100);
                    }
                }
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    _log.Error(e.Message.ToString() + " - " + e.InnerException.Message);
                }
                else
                {
                    _log.Error(e.Message);
                }
            }

        }

        // nacteni configuracniho souboru

        private void LoadConfiguration()
        {
            string cfg_file = "cmcfg.ini";
            string pg_section = "8bc";

            //Configuration config = ConfigurationManager.OpenMachineConfiguration();
            if (ConfigurationManager.AppSettings["pgConfig"] != null)
            {
                cfg_file = ConfigurationManager.AppSettings["pgConfig"];
            }

            if (ConfigurationManager.AppSettings["pgConfigSection"] != null)
            {
                pg_section = ConfigurationManager.AppSettings["pgConfigSection"];
            }

            if (ConfigurationManager.AppSettings["api_port"] != null)
            {
                _api_port = ConfigurationManager.AppSettings["api_port"];
            }

            if (_cmConfig.LoadPgConfig(cfg_file) == false)
            {
                _log.Error("LoadPgConfig failed");
                //Log("LoadPgConfig failed", XType.ERROR, GetName());
            }
            else
            {
                _log.Info("LoadPgConfig OK");
                //Log("LoadPgConfig OK", XType.ERROR, GetName());
            }
        }

        public bool Refresh()
        {
            return true;
        }

        Task _coreTask = null;
        public bool Start()
        {
            try
            {
                _coreTask = new Task(new Action(() =>
                    {

                        InitWebAPI(_api_port);
                    }));
                _coreTask.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public void Stop()
        {
            
        }

        public bool Initialized
        {
            get { return _init; }
        }


        public bool Inizialize()
        {
            return InitCore();
        }


        public void loadConfig()
        {
            LoadConfiguration();
            _pgManager = new PgSqlManager(_cmConfig, _log, 3);
            //_cmConfig.LoadPgConfig();
            _init = true;

            string section = ConfigurationManager.AppSettings["pgConfigSection"];

            _radio = new RadioData(_pgManager, section);           
        }

        public void saveConfig()
        {
            
        }

        public string Version
        {
            get
            {
                //AssemblyName ass_name = Assembly.GetExecutingAssembly().GetName();
                //return "ver. "+ass_name.Version.ToString();
                var ass = Assembly.GetExecutingAssembly();
                var attributes = ass.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false).Cast<AssemblyFileVersionAttribute>();
                var versionAttribute = attributes.Single();
                return versionAttribute.Version.ToString();
            }
        }
    }
}
