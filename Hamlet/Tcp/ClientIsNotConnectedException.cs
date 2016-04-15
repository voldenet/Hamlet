using System;

namespace Hamlet.Tcp
{
    [Serializable]
    internal class ClientIsNotConnectedException : Exception
    {
        public ClientIsNotConnectedException()
            : base("Client is not connected")
        {
        }
    }
}