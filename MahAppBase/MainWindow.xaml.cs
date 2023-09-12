using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Notifications.Wpf;

namespace ChatUI
{
	/// <summary>
	/// MainWindow.xaml 的互動邏輯
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		#region Declarations
		MainComponent viewModel = null;
		const uint ATTACH_PARENT_PROCESS = 0x0ffffffff;
		private bool closeMe;
		bool isFirstTime = true;

		bool isChoosingWord = false;
		string PreviousText = "";
		int wordLength = 0;

		#endregion

		#region Memberfunction
		/// <summary>
		/// 主視窗建構子
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();
			ShowMessage("通知", "開始初始化相關設定", NotificationType.Information);
			viewModel = new MainComponent();
			viewModel.Window = window;
			this.DataContext = viewModel;

			Barrage1 win = new Barrage1();
			win.Show();
			viewModel.Barrage = win;

		}
		public void ShowMessage(string title, string message, NotificationType type)
		{
			try
			{
				var notificationManager = new NotificationManager();
				var ts = TimeSpan.FromSeconds(10 * 1000);
				notificationManager.Show(new NotificationContent
				{
					Title = title,
					Message = message,
					Type = type,
				});

			}
			catch (Exception ex)
			{
				ShowMessage("顯示訊息時發生例外", $"{ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
			}
		}

		/// <summary>
		/// 輸入訊息元件按下上事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{

			if (e.Key == Key.Up)
			{
				viewModel.PressUpButtonCommandAction();
				return;
			}
			if (e.Key != Key.Enter)
				return;
			AttachConsole(ATTACH_PARENT_PROCESS);
			Console.WriteLine($"InputEnter");
			//按下Enter且在選字模式 > 不送訊息出去
			if (wordLength == (sender as System.Windows.Controls.TextBox).Text.Length && isChoosingWord)
			{
				return;

			}
			viewModel.SendMessage();
			PreviousText = "";

		}

		[DllImport("kernel32.dll")]
		static extern bool AttachConsole(uint dwProcessId);

		private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{

			AttachConsole(ATTACH_PARENT_PROCESS);
			Console.WriteLine($"textChange");

			Console.WriteLine($"Original : {wordLength}");
			Console.WriteLine($"Current : {(sender as System.Windows.Controls.TextBox).Text.Length}");
			//選字
			if (wordLength == (sender as System.Windows.Controls.TextBox).Text.Length)
			{
				isChoosingWord = true;
			}
			else
			{
				isChoosingWord = false;
			}
			wordLength = (sender as System.Windows.Controls.TextBox).Text.Length;
		}

		private void tbChat_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			tbChat.ScrollToEnd();
		}
		private async void window_Closing(object sender, CancelEventArgs e)
		{
			if (e.Cancel)
				return;

			// we want manage the closing itself!
			e.Cancel = !this.closeMe;
			// yes we want now really close the window
			if (this.closeMe)
				return;

			var mySettings = new MetroDialogSettings()
			{
				AffirmativeButtonText = "Quit",
				NegativeButtonText = "Cancel",
				AnimateShow = true,
				AnimateHide = false
			};
			var result = await this.ShowMessageAsync(
				"關閉應用程式 ?",
				"是否要關閉應用程式 ?",
				MessageDialogStyle.AffirmativeAndNegative, mySettings);

			this.closeMe = result == MessageDialogResult.Affirmative;

			if (this.closeMe)
			{
				Environment.Exit(0);
			}
			else
			{
				viewModel.State = WindowState.Minimized;
			}
		}
		private void window_GotFocus(object sender, RoutedEventArgs e)
		{
			if (isFirstTime)
			{
				isFirstTime = false;
			}
			foreach (var item in this.viewModel.AllUser)
			{
				Thread th = new Thread(() =>
				{
					try
					{
						System.Net.Sockets.Socket sock = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
						sock.Connect(item.UserIP, 6842);
						if (sock.Connected == true)
						{
							this.viewModel.AllUser.Where(x => x.UserIP == item.UserIP).FirstOrDefault().IsLive = Visibility.Visible;
						}
						sock.Close();
					}
					catch (System.Net.Sockets.SocketException ex)
					{

					}
				});
				th.Start();
			}
		}
		#endregion
	}
}