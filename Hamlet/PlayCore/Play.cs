using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Hamlet.Tcp;

namespace Hamlet.PlayCore
{
    internal class Play
    {
        private readonly Queue<PlayLine> _play = new Queue<PlayLine>();
        private readonly ActorsPool _pool;

        public Play(string txt, string channel, Func<IEventBasedClient> clientFactory)
        {
            _pool = new ActorsPool(clientFactory, channel);
            var drama = File.ReadAllLines(txt);
            foreach (var scriptline in drama)
            {
                var func = "";
                string charname;
                var line = scriptline;
                if (scriptline.StartsWith(">") || scriptline.StartsWith("<") || scriptline.StartsWith("*"))
                {
                    func = scriptline.Substring(0, 1);
                    line = scriptline.Substring(1);
                }
                if (line.IndexOf(' ') > -1)
                {
                    charname = line.Substring(0, line.IndexOf(' '));
                    line = line.Substring(line.IndexOf(' ') + 1);
                }
                else
                {
                    charname = line;
                    line = "";
                }
                _play.Enqueue(new PlayLine {Charname = charname, Func = func, Line = line});
            }
        }

        public async Task Perform()
        {
            var actors = new Dictionary<string, ActorUsage>();
            var funcs = new Dictionary<string, Func<ActorUsage, string, Task>>
            {
                {"", async (au, line) => await au.Write(line)},
                {"*", async (au, line) => await au.Act(line)},
                {"<", async (au, line) => await au.Leave()},
                {">", async (au, line) => await au.Join()}
            };
            var tasks = new List<Task>();
            foreach (var p in _play)
            {
                if (!actors.ContainsKey(p.Charname))
                {
                    var name = p.Charname.Substring(0, 1).ToUpper() + p.Charname.Substring(1).ToLower();
                    actors[p.Charname] = null;
                    tasks.Add(Task.Run(async () => actors[p.Charname] = await _pool.GetActor(name)));
                }
            }
            Console.WriteLine("Getting actors...");
            await Task.WhenAll(tasks);
            Console.WriteLine("Got all actors");
            while (_play.Count > 0)
            {
                var line = _play.Dequeue();
                await funcs[line.Func](actors[line.Charname], line.Line);
            }
        }

        private class PlayLine
        {
            public string Charname;
            public string Func;
            public string Line;
        }
    }
}