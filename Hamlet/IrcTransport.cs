using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hamlet.Tcp;

namespace Hamlet
{
    internal class IrcTransport
    {
        private IEventBasedClient _client;
        private readonly Func<IEventBasedClient> _clientFactory;
        private TaskCompletionSource<bool> _ready = new TaskCompletionSource<bool>();
        private string _nick;
        public readonly string Realname;
        public readonly string User;
        readonly HashSet<string> _channels = new HashSet<string>();

        public IrcTransport(Func<IEventBasedClient> clientFactory, string nick, string realname, string user)
        {
            _clientFactory = clientFactory;
            _nick = nick;
            Realname = realname;
            User = user;
            Reconnect();
        }

        private async void ReceiveWelcome(IEventBasedClient sender, LineReceivedEventArgs args)
        {
            if (args.Command == "001")
            {
                Console.WriteLine("Got 001");
                _ready.SetResult(true);
                _client.Receive -= ReceiveWelcome;

                foreach (var chan in _channels)
                {
                    await Write("JOIN " + chan);
                }
            }
        }

        private async void Reconnect()
        {
            if (_client != null)
            {
                await _client.Disconnect();
            }
            _client = _clientFactory();
            _client.Connected += async me =>
            {
                await me.Write("USER " + User + " 0 * :" + Realname);
                await me.Write("NICK " + _nick);
                Console.WriteLine(_nick + " connected!");
            };
            _client.Disconnected += me =>
            {
                Console.WriteLine(_nick + " Disonnected!");
                _ready = new TaskCompletionSource<bool>();
                Reconnect();
            };
            _client.Receive += ReceiveWelcome;
            _client.Receive += (me, arg) =>
            {
                var l = arg.Line;
                if (l.StartsWith("PING "))
                {
                    me.Write("PONG " + l.Substring(5));
                }
                Console.WriteLine("I > " + l);
            };
            await _client.Connect();
        }

        public async Task Join(string channel)
        {
            if (_channels.Add(channel))
            {
                await Write("JOIN " + channel);
            }
        }

        public async Task Part(string channel)
        {
            _channels.Remove(channel);
            await Write("PART " + channel);
        }

        public async Task SetNick(string newNick)
        {
            if (_nick != newNick)
            {
                await Write("NICK " + newNick);
                _nick = newNick;
            }
        }

        internal async Task Write(string line)
        {
            await _ready.Task;
            Console.WriteLine("O: " + line);
            await _client.Write(line);
        }
        public string Nick => _nick;
        public Task Ready => _ready.Task;
    }
}