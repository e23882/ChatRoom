using System;
using System.Collections.Generic;
using WebSocketSharp.Server;
using WebSocketSharp;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Server
{
	public class Connect : WebSocketBehavior
	{
		#region Property
		public static List<Connect> ClientList = new List<Connect>();
		#endregion

		#region Memberfunction
		public Connect() { }

		protected override void OnOpen()
		{
			//將目前使用者加入清單
			ClientList.Add(this);

			//if(!ClientList.Any(x=>x.Context.UserEndPoint.ToString().Split(':')[0] == this.Context.UserEndPoint.ToString().Split(':')[0])) 
			//通知所有以連線使用者目前登入的使用者
			foreach (var item in ClientList)
			{
				item.Send($"使用者 {this.Context.UserEndPoint} 加入聊天\n");
			}
			var current = ClientList.Where(x => x.Context.UserEndPoint == this.Context.UserEndPoint).FirstOrDefault();
			foreach (var item in ClientList)
			{
				current.Send($"[目前已連線使用者] {item.Context.UserEndPoint}");
			}
		}
		protected override void OnClose(CloseEventArgs e)
		{
			ClientList.Remove(this);

			foreach (var item in ClientList)
			{
				try
				{
					item.Send($"使用者 {this.Context.UserEndPoint} 離開聊天\n");
				}
				catch { }
			}
			List<string> existsUser = new List<string>();
			foreach(var item in ClientList)
			{
				var currentIP = item.Context.UserEndPoint.ToString().Split(':')[1];
				if (!existsUser.Contains(currentIP))
					existsUser.Add(currentIP);
			}
			Console.WriteLine($"目前使用者數量 {existsUser.Count()}");
			Console.WriteLine($"所有連線數量 {ClientList.Count()}");
		}
		protected override void OnMessage(MessageEventArgs e)
		{
			if (!e.IsPing)
			{
				string result;
				string CryptoKey = "54088";
				AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
				MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
				SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
				byte[] key = sha256.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));
				byte[] iv = md5.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));
				aes.Key = key;
				aes.IV = iv;
				var data = e.Data;
				byte[] dataByteArray = Convert.FromBase64String(data);
				using (MemoryStream ms = new MemoryStream())
				{
					using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
					{
						cs.Write(dataByteArray, 0, dataByteArray.Length);
						cs.FlushFinalBlock();
						result = Encoding.UTF8.GetString(ms.ToArray());
					}
				}

				foreach (var item in ClientList)
				{
					item.Send(result);
				}
			}
		}
		#endregion
	}
}
