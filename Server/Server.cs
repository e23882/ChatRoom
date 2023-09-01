using WebSocketSharp.Server;

namespace Server
{
    public class Server
    {
        #region Property
        public WebSocketServer SocketServer { get; set; }
        #endregion

        #region Memberfunction
        public Server(int port)
        {
            //設定服務的Port
            SocketServer = new WebSocketServer(port);
            //監聽連線
            SocketServer.AddWebSocketService<Connect>("/Connect");
        }
        
        /// <summary>
        /// 啟動服務
        /// </summary>
        public void Start() 
        {
            SocketServer.Start();
        }

        /// <summary>
        /// 停止服務
        /// </summary>
        public void Stop() 
        {
            SocketServer.Stop();
        }

        #endregion
    }
}
