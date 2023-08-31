using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using WebSocketSharp.Server;

namespace ChatUI
{
    public class ShareService
    {
        #region Property
        public WebSocketServer SocketServer { get; set; }
        public bool Sharing { get; set; } = false;
        #endregion

        #region Memberfunction
        public ShareService (int port)
        {
            SocketServer = new WebSocketServer(port);
            SocketServer.AddWebSocketService<Connect>("/Connect");
            SocketServer.Start();

           
        }

        public static byte[] ImageToByteArray (System.Drawing.Image x)
        {
            ImageConverter _imageConverter = new ImageConverter();
            byte[] xByte = (byte[])_imageConverter.ConvertTo(x, typeof(byte[]));
            return xByte;
        }

		internal void Run ()
		{
            while (true)
            {
                if (Sharing)
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

                        System.IO.MemoryStream ms = new MemoryStream();
                        bmpScreenshot.Save(ms, ImageFormat.Jpeg);
                        byte[] byteImage = ms.ToArray();
                        var SigBase64 = Convert.ToBase64String(byteImage);

                        SocketServer.WebSocketServices["/Connect"].Sessions.Broadcast(SigBase64);
                    }
                    catch
                    {

                    }
                }
                Thread.Sleep(100);
            }
        }
		#endregion
	}
}
