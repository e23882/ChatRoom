using System;
using System.ComponentModel;
using System.Configuration;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WebSocketSharp;

namespace ChatUI
{
	/// <summary>
	/// ChooseImage.xaml 的互動邏輯
	/// </summary>
	public partial class ChooseImage : Window
	{
		#region Property
		/// <summary>
		/// 送圖片WebSocket Client
		/// </summary>
		public WebSocket WebSocketClient { get; set; }
		#endregion

		#region Memberfunction
		/// <summary>
		/// 建構子
		/// </summary>
		public ChooseImage()
		{
			InitializeComponent();
			InitSendImageConnection();

			RenderImage(Properties.Resources.EatYourShit, "1");
			RenderImage(Properties.Resources.AllGarbege, "2");
			RenderImage(Properties.Resources.wut, "3");
			RenderImage(Properties.Resources.shock, "4");
			RenderImage(Properties.Resources.EatPie, "5");
			RenderImage(Properties.Resources.fool, "6");
			RenderImage(Properties.Resources.isback, "7");
			RenderImage(Properties.Resources.haha, "8");
			RenderImage(Properties.Resources.fuckoff, "9");
			RenderImage(Properties.Resources.disgusting, "10");

		}

		/// <summary>
		/// 初始化送圖片的連線
		/// </summary>
		private void InitSendImageConnection()
		{
			var server = ConfigurationSettings.AppSettings["Server"];
			WebSocketClient = new WebSocket($"ws://{server}:5566/Connect");
			WebSocketClient.Connect();
		}

		/// <summary>
		/// 從專案中Resource渲染圖片到介面上，讓使用者選
		/// </summary>
		/// <param name="data"></param>
		/// <param name="index"></param>
		private void RenderImage(System.Drawing.Bitmap data, string index)
		{
			BitmapImage bitmapImage;
			using (var memory = new MemoryStream())
			{
				data.Save(memory, ImageFormat.Png);
				memory.Position = 0;

				bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.StreamSource = memory;
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.EndInit();
				bitmapImage.Freeze();
			}
			Image img = new Image();
			img.Tag = index;
			img.Width = 250;
			img.Height = 250;
			img.Source = bitmapImage;
			img.MouseLeftButtonDown += Img_MouseLeftButtonDown;
			panel.Children.Add(img);
		}

		/// <summary>
		/// 加密圖片訊息
		/// </summary>
		/// <param name="imageIndex"></param>
		/// <returns></returns>
		public string SaultMessage(string imageIndex)
		{
			string CryptoKey = "54088";
			string result;
			AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
			MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
			SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
			byte[] key = sha256.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));
			byte[] iv = md5.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));
			aes.Key = key;
			aes.IV = iv;

			byte[] dataByteArray = Encoding.UTF8.GetBytes($"[img] {imageIndex}");
			using (MemoryStream ms = new MemoryStream())
			using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
			{
				cs.Write(dataByteArray, 0, dataByteArray.Length);
				cs.FlushFinalBlock();
				result = Convert.ToBase64String(ms.ToArray());
			}
			return result;
		}

		/// <summary>
		/// 選取圖片，送出圖片
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var element = (sender as Image);
			if (element is null)
				return;

			WebSocketClient.Send(SaultMessage(element.Tag.ToString()));
			this.Hide();
		}

		/// <summary>
		/// 關閉視窗事件，把視窗隱藏起來不要關掉(節省資源)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_Closed(object sender, CancelEventArgs e)
		{
			this.Hide();
			e.Cancel = true;
		}
		#endregion
	}
}
