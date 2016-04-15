using System;
using System.Threading.Tasks;
using Hamlet.PlayCore;
using Hamlet.Tcp;

namespace Hamlet
{
    internal class Program
    {
        public static async Task MainAsync(string[] args)
        {
            try
            {
                var play = new Play(@"DRAMA.txt", "#hamlet", ()=>new EventBasedTcpClient(args[0], 6667));
                await play.Perform();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.ToString());
            }
        }

        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }
    }
}