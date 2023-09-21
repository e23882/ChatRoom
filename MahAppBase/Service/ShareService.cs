using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Imaging;
using WebSocketSharp.Server;

namespace ChatUI
{
    public class ShareService : IDisposable
    {
        #region Property

        /// <summary>
        /// WebSocket 服務物件實例
        /// </summary>
        public WebSocketServer SocketServer { get; private set; }

        /// <summary>
        /// 是否分享畫面
        /// </summary>
        public bool Sharing { get; set; } = false;

        #endregion

        #region Memberfunction

        /// <summary>
        /// ShareService物件建構子
        /// </summary>
        /// <param name="port"></param>
        public ShareService(int port)
        {
            SocketServer = new WebSocketServer(port);
            SocketServer.AddWebSocketService<Connect>("/Connect");
            SocketServer.Start();
        }

        /// <summary>
        /// 執行ShareService
        /// </summary>
        internal void Run()
        {
            while (true)
            {
                try
                {
                    var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                        Screen.PrimaryScreen.Bounds.Height,
                        PixelFormat.Format32bppArgb);
                    var gfxScreenshot = Graphics.FromImage(bmpScreenshot);

                    gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                        Screen.PrimaryScreen.Bounds.Y,
                        0,
                        0,
                        Screen.PrimaryScreen.Bounds.Size,
                        CopyPixelOperation.SourceCopy);

                    var ms = new MemoryStream();
                    bmpScreenshot.Save(ms, ImageFormat.Jpeg);
                    byte[] byteImage = ms.ToArray();
                    var sigBase64 = Convert.ToBase64String(byteImage);
                    if (SocketServer.IsListening)
                    {
                        SocketServer.WebSocketServices["/Connect"].Sessions.Broadcast(sigBase64);
                    }
                }
                catch
                {
                }

                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 釋放資源
        /// </summary>
        public void Dispose()
        {
            SocketServer = null;
            this.Dispose();
        }

        #endregion
    }
}