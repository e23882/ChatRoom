using System;

namespace Server
{
    public class Program 
    {
        #region Declarations
        public static Server service = null;
        #endregion

        public static void Main(string[] args) 
        {
            Server server = new Server(5566);
            server.Start();
            Console.WriteLine("WebSocket server started. Press any key to stop.");
            Console.ReadKey();
            server.Stop();
        }
    }
}
