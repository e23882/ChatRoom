using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Update
{
	public class Program
	{
		#region Declarations
		public static List<string> FileList = new List<string>();
		#endregion

		#region Memberfunction
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		static void Main (string[] args)
		{
			int retryTimes  = 3;
			while (retryTimes > 0)
			{
				try
				{
					//取得要更新的檔案清單
					GetFileList();
					bool downloadSuccess = Download();
					if (downloadSuccess)
					{
						StartChatUI();
					}
					break;
				}
				catch(Exception ex) 
				{
					retryTimes--;
				}
			}
		}
		
		/// <summary>
		/// 重新啟動聊天室 Client UI
		/// </summary>
		public static void StartChatUI ()
		{
			Process.Start("ChatUI.exe");
		}
		
		/// <summary>
		/// 從FTP下載檔案
		/// </summary>
		/// <returns></returns>
		public static bool Download ()
		{
			try
			{
				foreach (var item in FileList)
				{
					string ftpServer = $"ftp://10.93.9.117//Update/{item}"; // FTP 服务器地址
					WebClient client = new WebClient();
					client.Credentials = new NetworkCredential("anonymous", "anonymous@example.com");
					client.DownloadFile(ftpServer, item);
				}
				Console.WriteLine($"下載檔案成功");
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"下載檔案失敗 : {ex.Message}\r\n{ex.StackTrace}");
				Console.Read();
				return false;
			}
		}

		/// <summary>
		/// 取得FTP檔案清單
		/// </summary>
		private static void GetFileList ()
		{
			try
			{
				string ftpServer = "ftp://10.93.9.117/Update";
				FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(ftpServer));
				request.Method = WebRequestMethods.Ftp.ListDirectory;
				request.Credentials = new NetworkCredential("anonymous", "anonymous@example.com");
				using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
				{
					using (Stream responseStream = response.GetResponseStream())
					{
						using (StreamReader reader = new StreamReader(responseStream))
						{
							// 读取FTP服务器上的文件内容
							string fileContent = reader.ReadToEnd();
							var allFile = fileContent.Split(new string[] { "\r\n" }, StringSplitOptions.None);
							foreach (var item in allFile)
							{
								if (item.Length < 3)
									continue;
								if (string.IsNullOrEmpty(item))
									continue;
								FileList.Add(item);
							}
						}
					}
				}
				Console.WriteLine($"取得更新檔案清單成功");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"取得更新檔案清單發生例外 : {ex.Message}\r\n{ex.StackTrace}");
			}
		}
		#endregion
	}
}
