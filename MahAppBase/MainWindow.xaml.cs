using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace ChatUI
{
	/// <summary>
	/// MainWindow.xaml 的互動邏輯
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		#region Declarations
		MainComponent viewModel = null;
		#endregion

		#region Memberfunction
		/// <summary>
		/// 主視窗建構子
		/// </summary>
		public MainWindow ()
		{
			InitializeComponent();
			viewModel = new MainComponent();
			this.DataContext = viewModel;
		}

		/// <summary>
		/// 關閉視窗事件
		/// </summary>
		/// <param name="e"></param>
		protected override void OnClosing (CancelEventArgs e)
		{
			viewModel.State = WindowState.Minimized;
			e.Cancel = true;
			base.OnClosing(e);
		}

		bool isChoosingWord = false;
		string PreviousText = "";
		/// <summary>
		/// 輸入訊息元件按下上事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TextBox_KeyUp (object sender, System.Windows.Input.KeyEventArgs e)
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
		#endregion
		int wordLength = 0;
		[DllImport("kernel32.dll")]
		static extern bool AttachConsole (uint dwProcessId);

		const uint ATTACH_PARENT_PROCESS = 0x0ffffffff;
		private void TextBox_TextChanged (object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			
			AttachConsole(ATTACH_PARENT_PROCESS);
			Console.WriteLine($"textChange");

			Console.WriteLine($"Original : {wordLength}");
			Console.WriteLine($"Current : {(sender as System.Windows.Controls.TextBox).Text.Length}");
			//選字
			if(wordLength == (sender as System.Windows.Controls.TextBox).Text.Length)
			{
				isChoosingWord = true;
			}
			else
			{
				isChoosingWord = false;
			}
			wordLength = (sender as System.Windows.Controls.TextBox).Text.Length;
			//前一次的字數比目前多 : 輸入法在選字
			//if (wordLength > (sender as System.Windows.Controls.TextBox).Text.Length)
			//{
			//	Console.WriteLine($"Original : {wordLength}");
			//	Console.WriteLine($"Current : {(sender as System.Windows.Controls.TextBox).Text.Length}");
			//	isChoosingWord = true;
			//}
			//else
			//{
			//	isChoosingWord = false;
			//}
			////Console.WriteLine($"Original : {wordLength}");
			////Console.WriteLine($"Current : {(sender as System.Windows.Controls.TextBox).Text.Length}");
			//wordLength = (sender as System.Windows.Controls.TextBox).Text.Length;
		}

		private void tbChat_TextChanged (object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			tbChat.ScrollToEnd();
		}
	}
}