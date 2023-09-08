using System;
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
	public partial class ChooseImage: Window
	{
		public WebSocket WebSocketClient { get; set; }

		public ChooseImage ()
		{
			InitializeComponent();
			RenderImage(Properties.Resources.EatYourShit, "1");
			RenderImage(Properties.Resources.AllGarbege, "2");
			RenderImage(Properties.Resources.wut, "3");
			RenderImage(Properties.Resources.shock, "4");

		}

		private void RenderImage (System.Drawing.Bitmap data, string index)
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


		public string SaultMessage (string imageIndex) 
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
		private void Img_MouseLeftButtonDown (object sender, MouseButtonEventArgs e)
		{
			var element = (sender as Image);
			if (element is null)
				return;
			var server = ConfigurationSettings.AppSettings["Server"];
			WebSocketClient = new WebSocket($"ws://{server}:5566/Connect");
			WebSocketClient.Connect();
			WebSocketClient.Send(SaultMessage(element.Tag.ToString()));
			this.Close();
		}
	}
}
