using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IWorkCore
    {
        bool Inizialize();
        bool Start();
        void Stop();
        void loadConfig();
        void saveConfig();
        //
        string GetName();
        string Version { get; }
    }
}
