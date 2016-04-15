using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Hamlet.Tcp
{
    internal class LineTcpClient : IDisposable
    {
        private readonly List<IDisposable> _listToDispose = new List<IDisposable>();
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;
        private TcpClient _tcpClient;
        public bool IsConnected
        {
            get
            {
                try
                {
                    if (_tcpClient?.Client != null && _tcpClient.Client.Connected)
                    {
                        /* pear to the documentation on Poll:
                        * When passing SelectMode.SelectRead as a parameter to the Poll method it will return
                        * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
                        * -or- true if data is available for reading;
                        * -or- true if the connection has been closed, reset, or terminated;
                        * otherwise, returns false
                        */

                        // Detect if client disconnected
                        if (_tcpClient.Client.Poll(0, SelectMode.SelectRead))
                        {
                            var buff = new byte[1];
                            if (_tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                            {
                                // Client disconnected
                                return false;
                            }
                            return true;
                        }
                        return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        public void Dispose()
        {
            foreach (var obj in _listToDispose)
                obj.Dispose();
        }

        public async Task ConnectAsync(string ip, int port)
        {
            _tcpClient = new TcpClient();
            _listToDispose.Add(_tcpClient);
            await _tcpClient.ConnectAsync(ip, port);
            var stream = _tcpClient.GetStream();
            _listToDispose.Add(_streamWriter = new StreamWriter(stream));
            _streamWriter.AutoFlush = true;
            _listToDispose.Add(_streamReader = new StreamReader(stream));
        }

        public void Disconnect()
        {
            _tcpClient.Close();
        }

        public async Task<string> ReadLineAsync()
        {
            if (IsConnected && _streamReader != null)
                return await _streamReader.ReadLineAsync();
            throw new ClientIsNotConnectedException();
        }

        public async Task WriteLineAsync(string value)
        {
            if (IsConnected && _streamWriter != null)
            {
                await _streamWriter.WriteLineAsync(value);
            }
            else
                throw new ClientIsNotConnectedException();
        }
    }
}