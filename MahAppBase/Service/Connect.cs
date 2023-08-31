using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace ChatUI
{
    public class Connect: WebSocketBehavior
    {
        #region Property
        public List<string> ClientList { get; set; }
        #endregion

        #region Memberfunction
        public Connect ()
        {
            ClientList = new List<string>();
        }
        protected override void OnClose (CloseEventArgs e)
        {

            //ClientList.Add(e..Data);
        }
        protected override void OnMessage (MessageEventArgs e)
        {
            if (e.IsPing)
            {
                Info($"Receive Ping {e.Data}");
            }
            else
            {
                var data = e.Data;
                Info(data);
            }
            //Send(msg);
        }

        public void Info (string message)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(message);
        }
        public void Warning (string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
        }
        public void Error (string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
        }
        #endregion
    }
}
