using System;
using System.Windows;
using System.Windows.Input;
using MahAppBase.ViewModel;
using MahApps.Metro.Controls;

namespace MahAppBase
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

		private void MetroWindow_Closed(object sender, EventArgs e)
		{
			viewModel.State = WindowState.Minimized;
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