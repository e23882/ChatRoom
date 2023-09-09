using System;

namespace Server
{
    public class Program 
    {
        #region Declarations
        /// <summary>
        /// Server物件實例
        /// </summary>
        public static Server service = null;
        #endregion

        #region MemberFunction
        /// <summary>
        /// 啟動服務主程式
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            Server server = new Server(5566);
            server.Start();
            Console.WriteLine("WebSocket server started. Press any key to stop.");
            Console.ReadKey();
            server.Stop();
        }
        #endregion
    }
}
