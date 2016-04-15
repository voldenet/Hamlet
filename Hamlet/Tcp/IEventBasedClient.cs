using System.Threading.Tasks;

namespace Hamlet.Tcp
{
    internal interface IEventBasedClient
    {
        event EventBasedEventHandler Connected;
        event EventBasedEventHandler Disconnected;
        event LineReceivedEventHandler Receive;
        Task Write(string line);
        Task Connect();
        Task Disconnect();
    }
}