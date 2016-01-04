using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerData
{
    public interface ICmConfiguration
    {
        IConnectionConfig GetConfiguration(string name);
    }

    public interface IConnectionConfig
    {
        string Name {get; set;}
        string Server { get; set; }
        int Port { get; set; }
        string User { get; set; }
        string Password { get; set; }
        string Database { get; set; }                
    }
}
