using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace Core.Commands
{
    public class Commands : List<Command>
    {
        ManagerCore _manager;

        public Commands(ManagerCore manager)
        {
            _manager = manager;
        }
    }

    // vymyslet paramtery !!!
    public class Command
    {
        public virtual string Name { get; protected set; }

        public virtual bool Execute()
        {
            return true;
        }
    }

    [Export(typeof(Command))]
    public class ExitCommand : Command
    {
        public override string Name
        {
            get
            {
                return "exit";
            }
            protected set
            {
                base.Name = value;
            }
        }

        public override bool Execute()
        {
            return false;
        }
    }
}
