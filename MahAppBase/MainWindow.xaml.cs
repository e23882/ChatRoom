using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ChatUI.ViewModel;
using MahApps.Metro.Controls;

namespace ChatUI
{
	/// <summary>
	/// MainWindow.xaml 的互動邏輯
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		MainComponent viewModel = null;
		public MainWindow()
		{
			InitializeComponent();
			viewModel = new MainComponent();
			viewModel.ChatTextBox = tbChat;
			this.DataContext = viewModel;
		}
		protected override void OnClosing(CancelEventArgs e)
		{
			viewModel.State = WindowState.Minimized;
			e.Cancel = true;
			//this.Hide();
			//this.Show();
			base.OnClosing(e);
		}

		private void MetroWindow_Closed(object sender, EventArgs e)
		{
			
			
			
		}

		private void TextBox_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Up)
			{
				viewModel.PressUpButtonCommandAction();
				return;
			}
			if (e.Key != System.Windows.Input.Key.Enter)
				return;


			viewModel.SendMessage();
			tbChat.ScrollToEnd();
		}
	}
}