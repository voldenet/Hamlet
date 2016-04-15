using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hamlet.Tcp;

namespace Hamlet.PlayCore
{
    internal class ActorsPool
    {
        private readonly List<Actor> _actors = new List<Actor>();
        private readonly object _actorsLock = new object();
        private int _actorId;
        private readonly Func<IEventBasedClient> _clientFactory;
        private readonly string _channel;
        public ActorsPool(Func<IEventBasedClient> clientFactory, string channel)
        {
            _clientFactory = clientFactory;
            _channel = channel;
        }

        private async Task<Actor> GetFirstUsableWithNamePriority(string name)
        {
            Actor actor;
            lock (_actorsLock)
            {
                actor = _actors.FirstOrDefault(a => !a.Used && a.IrcTransport.Nick == name) ??
                            _actors.FirstOrDefault(a => !a.Used);
                if (actor == null)
                {
                    actor = new Actor { IrcTransport = null };
                    _actors.Add(actor);
                }
                actor.Used = true;
            }
            var id = "tmp-" + ++_actorId;
            if (actor.IrcTransport == null)
            {
                var transport = new IrcTransport(_clientFactory, id, id, id);
                actor.IrcTransport = transport;
                await transport.Ready;
            }
            return actor;
        }
        public async Task<ActorUsage> GetActor(string name)
        {
            var actor = await GetFirstUsableWithNamePriority(name);
            Console.WriteLine("Got actor");
            if (actor.IrcTransport.Nick != name)
            {
                await actor.IrcTransport.Part(_channel);
                await Task.Delay(700);
                await actor.IrcTransport.Write("NICK " + name);
                await Task.Delay(700);
                await actor.IrcTransport.SetNick(name);
                await actor.IrcTransport.Join(_channel);
            }
            return new ActorUsage(actor, _channel);
        }
    }
}