using System;
using System.ComponentModel;
using System.IO;
using System.Net;
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
		#region Declarations
		MainComponent viewModel = null;
		#endregion

		#region Memberfunction
		public MainWindow ()
		{
			InitializeComponent();
			viewModel = new MainComponent();
			this.DataContext = viewModel;
		}
		protected override void OnClosing (CancelEventArgs e)
		{
			viewModel.State = WindowState.Minimized;
			e.Cancel = true;
			base.OnClosing(e);
		}
		private void MetroWindow_Closed (object sender, EventArgs e){}
		private void TextBox_KeyUp (object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Up)
			{
				viewModel.PressUpButtonCommandAction();
				return;
			}
			if (e.Key != Key.Enter)
				return;

			viewModel.SendMessage();
		}
		#endregion
	}
}