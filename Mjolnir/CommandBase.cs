using System;
using System.Collections.Generic;
using System.Text;
using MjolnirCore.Extensions;
using System.Linq;
using Discord.Structures;
using Discord.Http;
using System.Threading.Tasks;

namespace Mjolnir {
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class CommandAttr : System.Attribute {
        private string[] names;
        public string[] Names => names.ToArray();

        public CommandAttr(string[] names) {
            this.names = names;
        }

        override public string ToString() {
            return "CommandAttr: " + names.ToSequenceString();
        }
        public string namesToString() {
            return names.ToSequenceString();
        }
    }

    public partial class Commands {

        private HttpBotInterface http;

        public Commands(HttpBotInterface http) {
            this.http = http;
        }

        [CommandAttr("Hello")]
        public async Task HelloWorld(Message message) {
            Console.WriteLine("Hello World");
            await http.CreateReaction(message, "👋");
        }

        [CommandAttr(CommandInterface.UnknownCommandKey)]
        public async Task UnknownCommand(Message message) {
            Console.WriteLine("Hello World");
            await http.CreateReaction(message, "❓");
        }

        [CommandAttr("Help")]
        public async Task CommandList(Message message) {
            var test = this.GetType().GetMethods()
                    .Where(m => m.GetCustomAttributes(true).Length != 0)
                    .Select(m => (m.Name,
                    ((CommandAttr)m.GetCustomAttributes(true).Single(a => a is CommandAttr)).namesToString()))
                    .ToSequenceString();

            await http.CreateMessage(message, test);
        }
    }
}
