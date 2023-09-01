using System;
using System.IO;
using System.Windows.Media.Imaging;
using WebSocketSharp;

namespace ChatUI
{
	public class LiveViewModel: ViewModelBase
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

		/// <summary>
		/// 分享畫面的使用者IP
		/// </summary>
		public string ServerIP { get; set; }

		/// <summary>
		/// WebClient物件實例
		/// </summary>
		public WebSocket webSocketClient { get; set; }

		#endregion

		#region Memberfunction
		/// <summary>
		/// 觀看分享畫面視窗ViewModel
		/// </summary>
		/// <param name="selectedListBoxItem"></param>
		public LiveViewModel (string selectedListBoxItem)
		{
			ServerIP = selectedListBoxItem;
			InitialClient();
		}

		/// <summary>
		/// 初始化WebSocket Client
		/// </summary>
		public void InitialClient ()
		{
			try
			{
				webSocketClient = new WebSocket($"ws://{ServerIP}:6842/Connect");
				webSocketClient.OnMessage += Ws_OnMessage;
				webSocketClient.Connect();
			}
			catch { }
		}

		/// <summary>
		/// WebSocket Clinet收到訊息
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Ws_OnMessage (object sender, MessageEventArgs e)
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
		#endregion
	}
}
