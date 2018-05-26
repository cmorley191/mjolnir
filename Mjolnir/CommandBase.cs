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

        private string info;
        public string Info => info;

        public CommandAttr(string info, params string[] names) {
            this.names = names;
            this.info = info;
        }

        override public string ToString() {
            return "CommandAttr: " + names.ToSequenceString() + "Info: " + info;
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

        [CommandAttr("Waves Hello", "Hello")]
        public async Task HelloWorld(Message message) {
            Console.WriteLine("Hello World");
            await http.CreateReaction(message, "👋");
        }

        [CommandAttr("Unknown Command", CommandInterface.UnknownCommandKey)]
        public async Task UnknownCommand(Message message) {
            Console.WriteLine("Hello World");
            await http.CreateReaction(message, "❓");
        }

        [CommandAttr("Lists commands", "Help", "List")]
        public async Task CommandList(Message message) {

            EmbedField[] commands = this.GetType().GetMethods()
                    .Where(m => m.GetCustomAttributes(true).Length != 0)
                    .Select(m => (
                        ((CommandAttr)m.GetCustomAttributes(true).Single(a => a is CommandAttr))))
                    .Where(a => !(a.Names.Contains(CommandInterface.UnknownCommandKey)))
                    .Select(a => EmbedField.Build((a.Names.Length > 1) ? $"__**{a.Names[0]}**__ ({String.Join(",", a.Names.Skip(1))})" : $"__**{a.Names[0]}**__", a.Info))
                    .ToArray();

            Embed temp = Embed.Build(title: "__***List of Commands***__", fields: commands);

            await http.CreateMessage(message, $"{{\"content\": \"\",\"embed\":{temp.Serialize()} }}");
        }
    }
}
