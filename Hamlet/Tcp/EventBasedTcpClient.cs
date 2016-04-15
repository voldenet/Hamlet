using System.Threading;
using System.Threading.Tasks;

namespace Hamlet.Tcp
{
    internal class EventBasedTcpClient : IEventBasedClient
    {
        private LineTcpClient _client;
        private int _state;
        private readonly string _address;
        private readonly int _port;
        public event EventBasedEventHandler Connected;
        public event EventBasedEventHandler Disconnected;
        public event LineReceivedEventHandler Receive;

        public EventBasedTcpClient(string address, int port)
        {
            _address = address;
            _port = port;
        }

        public async Task Write(string line)
        {
            if (_state == (int) State.Finished)
                return;
            while (_state < (int) State.Connected)
                await Task.Delay(100);
            await _client.WriteLineAsync(line);
        }

        private bool AdvanceState(State from, State to)
        {
            return (int) from != Interlocked.CompareExchange(ref _state, (int) to, (int) from);
        }

        /// <summary>
        /// Connects to the given server
        /// </summary>
        /// <returns>Task that's valid until connection's over</returns>
        public async Task Connect()
        {
            // was this class initiated already?
            if (AdvanceState(State.New, State.Connecting))
                return;
            using (var client = _client = new LineTcpClient())
            {
                try
                {
                    await client.ConnectAsync(_address, _port);
                    _state = (int) State.Connected;
                    var c = Connected;
                    c?.Invoke(this);
                    while (true)
                    {
                        var line = await client.ReadLineAsync();
                        if (line != null)
                        {
                            var ev = Receive;
                            ev?.Invoke(this, new LineReceivedEventArgs {Line = line});
                        }
                    }
                }
                catch (ClientIsNotConnectedException)
                {
                    var c = Disconnected;
                    c?.Invoke(this);
                }
                _state = (int) State.Finished;
            }
        }

        public Task Disconnect()
        {
            _client.Disconnect();
            return Task.FromResult(true);
        }

        private enum State
        {
            New,
            Connecting,
            Connected,
            Finished
        }
    }
}