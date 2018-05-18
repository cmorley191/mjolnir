using Discord.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MjolnirCore;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Discord.Structures;
using System.Reflection;

namespace Mjolnir {
    public abstract class CommandInterface {

        public delegate Task CommandListener(Message message);
        protected IDictionary<string, IList<CommandListener>> Listeners = new Dictionary<string, IList<CommandListener>>();
        public const string UnknownCommandKey = "";

        public void AddListener(string command, CommandListener listener) {
            command = command.ToLower();
            if (Listeners.ContainsKey(command))
                Listeners[command].Add(listener);
            else
                Listeners[command] = new List<CommandListener>() { listener };
        }

        public void AddListeners(object commandObject) {
            Type objtype = commandObject.GetType();
            var methods = objtype.GetMethods();
            foreach (MethodInfo p in methods) {
                // for every property loop through all attributes
                foreach (Attribute a in p.GetCustomAttributes(true)) {
                    if (a is CommandAttr) {
                        var ca = a as CommandAttr;
                        foreach (string name in ca.Names)
                            AddListener(name, m => (Task)p.Invoke(commandObject, new object[] { m }));
                    }
                }
            }
        }

        protected async Task ProcessMessage(Message message) {
            Debug.WriteLine($"Processing {message.Id}");

            var commandRegex = @"!([^\s]+)(.*)";
            if (Regex.IsMatch(message.Content, commandRegex)) {
                var groups = Regex.Match(message.Content, commandRegex).Groups;
                var command = groups[1].ToString().ToLower();
                var arguments = groups[2].ToString().Trim();
                Debug.WriteLine($"Processing {command}: {arguments}");

                if (Listeners.ContainsKey(command)) {
                    Debug.WriteLine($"Invoking {Listeners[command].Count} listeners");
                    foreach (var listener in Listeners[command])
                        await listener.Invoke(message);
                } else {
                    Debug.WriteLine($"Unknown command");
                    if (Listeners.ContainsKey(UnknownCommandKey))
                        foreach (var listener in Listeners[UnknownCommandKey])
                            await listener.Invoke(message);
                }
            } else {
                Debug.WriteLine($"Found a non-command message.");
            }
        }
    }
}
