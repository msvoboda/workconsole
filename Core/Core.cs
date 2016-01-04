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
    [Export(typeof(IWorkCore))]
    public class WorkCore : IWorkCore
    {

        public bool Start()
        {
            return true;
        }

        public void Stop()
        {
           
        }

        public string GetName()
        {
            return "work";
        }

        public bool Inizialize()
        {
            return true;
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
