using System;
using System.Threading.Tasks;

namespace Hamlet.PlayCore
{
    internal class ActorUsage : IDisposable
    {
        private readonly Actor _actor;
        private readonly string _channel;

        public ActorUsage(Actor actor, string channel)
        {
            _actor = actor;
            _channel = channel;
        }

        public void Dispose()
        {
            _actor.Used = false;
        }

        public async Task Leave()
        {
            await Act("Leaves");
            await Task.Delay(500);
        }

        public async Task Join()
        {
            await Act("Enters");
            await Task.Delay(500);
        }

        public async Task Act(string s)
        {
            Console.WriteLine("*" + _actor.IrcTransport.Nick + ": " + s);
            var line = $"PRIVMSG {_channel} :" + (char) 1 + "ACTION " + s + (char) 1;
            await Task.Delay(200 + s.Length*50);
            await _actor.IrcTransport.Write(line);
            await Task.Delay(200 + s.Length*50);
        }

        public async Task Write(string s)
        {
            Console.WriteLine(_actor.IrcTransport.Nick + ": " + s);
            var line = $"PRIVMSG {_channel} :" + s;
            await Task.Delay(200 + s.Length*50);
            await _actor.IrcTransport.Write(line);
            await Task.Delay(200 + s.Length*50);
        }
    }
}