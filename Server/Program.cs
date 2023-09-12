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

        #region Property
        public static int ServicePort { get; set; } = 5566;
		#endregion

		#region MemberFunction
		/// <summary>
		/// 啟動服務主程式
		/// </summary>
		/// <param name="args"></param>
		public static void Main(string[] args)
        {
			try
			{
                Server server = new Server(ServicePort);
                server.Start();
                Console.WriteLine($"WebSocket server started through {ServicePort} port. Press any key to stop.");
                Console.ReadKey();
                server.Stop();
            }
            catch(Exception ex) 
            {
                Console.WriteLine($"Initial WebSocket Server Exception : {ex.Message}\r\n{ex.StackTrace}");
            }
        }
        #endregion
    }
}
