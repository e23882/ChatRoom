using System;
using System.IO;
using System.Windows.Media.Imaging;
using WebSocketSharp;

namespace ChatUI
{
	public class LiveViewModel : ViewModelBase
	{
		#region Declarations
		private BitmapImage _Source = new BitmapImage();
		#endregion

		#region Property
		/// <summary>
		/// 分享畫面的圖片
		/// </summary>
		public BitmapImage Source
		{
			get
			{
				return _Source;
			}
			set
			{
				_Source = value;
				OnPropertyChanged();
			}
		}

		public string SerevrPort { get; set; } = "6842";

		/// <summary>
		/// 分享畫面的使用者IP
		/// </summary>
		public string ServerIP { get; set; }

		/// <summary>
		/// WebClient物件實例
		/// </summary>
		public WebSocket WebSocketClient { get; set; }

		#endregion

		#region Memberfunction
		/// <summary>
		/// 觀看分享畫面視窗ViewModel
		/// </summary>
		/// <param name="selectedListBoxItem"></param>
		public LiveViewModel(string selectedListBoxItem)
		{
			try
			{
				ServerIP = selectedListBoxItem;
				InitialClient();
			}
			catch (Exception ex)
			{
				//TODO : 通知、寫LOG
			}
		}

		/// <summary>
		/// 初始化WebSocket Client
		/// </summary>
		public void InitialClient()
		{
			try
			{
				WebSocketClient = new WebSocket($"ws://{ServerIP}:{SerevrPort}/Connect");
				WebSocketClient.OnMessage += Ws_OnMessage;
				WebSocketClient.Connect();
			}
			catch
			{
				//TODO : 通知、寫LOG
			}
		}

		/// <summary>
		/// WebSocket Clinet收到訊息
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Ws_OnMessage(object sender, MessageEventArgs e)
		{
			try
			{
				var ws = (sender as WebSocket);
				if (ws is null)
				{
					return;
				}

				string receiveData = e.Data;
				switch (receiveData)
				{
					case "CLOSE":
						Console.WriteLine("hit");
						break;
					default:
						byte[] binaryData = Convert.FromBase64String(receiveData);

						BitmapImage bi = new BitmapImage();
						bi.BeginInit();
						bi.StreamSource = new MemoryStream(binaryData);
						bi.EndInit();
						bi.Freeze();
						Source = bi;
						break;
				}
			}
			catch (Exception ex)
			{
				//TODO : 通知、寫LOG
			}
		}
		#endregion
	}
}
