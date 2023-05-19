using System;
using System.Threading;
using WebSocketSharp.Server;
using WebSocketSharp;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;

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
            SocketServer = new WebSocketServer(port);
            //監聽連線
            SocketServer.AddWebSocketService<Connect>("/Connect");
        }
        public void Start() 
        {
            SocketServer.Start();
        }

        public void Stop() 
        {
            SocketServer.Stop();
        }

        #endregion
    }
}
