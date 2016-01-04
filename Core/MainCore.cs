using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    [Export(typeof(IWorkCore))]
    class MainCore : IWorkCore
    {
        public bool Inizialize()
        {
            return true;  
        }

        public bool Start()
        {
            return true;
        }

        public void Stop()
        {
            
        }

        public string GetName()
        {
            return "MainCore";
        }


        public void loadConfig()
        {
           
        }

        public void saveConfig()
        {
           
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
