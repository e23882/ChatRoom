using WebSocketSharp.Server;

namespace Server
{
    public class Server
    {
        #region Property
        /// <summary>
        /// Socket Server IP
        /// </summary>
        public WebSocketServer SocketServer { get; set; }
        #endregion

        #region Memberfunction
        /// <summary>
        /// Server建構子
        /// </summary>
        /// <param name="port"></param>
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
