using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerCore
{
    public delegate bool PowerEventHandler(IPowerEvent evt);

    /// <summary>
    /// Zobecneny job
    /// </summary>
    interface IJob
    {
        // PROPERTY
        string Name { get; }
        List<IPowerEvent> Events { get; }
        PowerTask Task { get; }
        // Functions
        bool ExecuteTask();
        // Events
        event PowerEventHandler OnEvent; // nastane udalost, ziska parametry a spusti ukol
    }

    public interface IPowerEvent
    {

    }
}
