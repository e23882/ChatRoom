using System;
using System.Collections.Generic;
using WebSocketSharp.Server;
using WebSocketSharp;
using System.Linq;

namespace Server
{
    public class Connect : WebSocketBehavior
    {
        #region Property
        public static List<Connect> ClientList = new List<Connect>();
        #endregion

        #region Memberfunction
        public Connect()
        {
        }

        protected override void OnOpen() 
        {
            //將目前使用者加入清單
            ClientList.Add(this);

            //通知所有以連線使用者目前登入的使用者
            foreach (var item in ClientList)
            {
                item.Send($"使用者 {this.ID} 加入聊天\n");
            }

            var current = ClientList.Where(x => x.ID == this.ID).FirstOrDefault();
            foreach (var item in ClientList)
            {
                current.Send($"[目前已連線使用者] {item.ID}");
            }
        }
        protected override void OnClose(CloseEventArgs e)
        {
            foreach (var item in ClientList)
            {
                item.Send($"使用者 {this.ID} 離開聊天\n");
            }
            ClientList.Remove(this);
            
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            if (e.IsPing)
            {
                Info($"Receive Ping {e.Data}");
            }
            else
            {
                var data = e.Data;
                foreach (var item in ClientList) 
                {
                    item.Send(e.Data);
                 
                }
            }
        }

        public void Info(string message)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(message);
        }
        public void Warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
        }
        public void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
        }
        #endregion
    }
}
