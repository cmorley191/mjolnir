using System;
using System.Collections.Generic;
using System.Text;
using MjolnirCore.Extensions;
using System.Linq;
using Discord.Structures;

namespace Mjolnir {
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class CommandAttr : System.Attribute {
        private string[] names;

        public CommandAttr(params string[] names) {
            this.names = names;
        }

        override public string ToString() {
            return "CommandAttr: " + names.ToSequenceString();
        }
    }

    public partial class Commands {

        [CommandAttr("Hello")]
        public void HelloWorld(Message message) {
            Console.WriteLine("Hello World");
        }


        [CommandAttr("Help")]
        public void CommandList(Message message) {
            Console.WriteLine((new Commands()).GetType().GetMethods().Where(m => m.GetCustomAttributes(true).Length != 0).Select(m => m.Name).ToSequenceString());
        }
    }
}
