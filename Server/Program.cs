using System;
using System.Threading;
using System.Windows.Forms;

namespace Server
{
    public class Program 
    {
        #region Declarations
        public static Server service = null;
        #endregion

        #region Memberfunction
        #endregion
        public static void Main(string[] args) 
        {
            Server server = new Server(5577);
            server.Start();
            Console.WriteLine("WebSocket server started. Press any key to stop.");
            Console.ReadKey();
            server.Stop();

        }
    }
}
