using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using ChatUI.CustomerUserControl;
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

		private void TextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Up)
			{
				viewModel.PressUpButtonCommandAction();
				return;
			}
			if (e.Key != System.Windows.Input.Key.Enter)
				return;


			viewModel.SendMessage();
		}

		private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var currentImage = (sender as Image);
			if (currentImage != null)
			{
				ucShowImage uc = new ucShowImage(currentImage.Source);
				uc.Show();
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var currentButton = (sender as System.Windows.Controls.Button);
			var ftpPath = currentButton.Content.ToString().Split(' ')[1];
			var allString = currentButton.Content.ToString().Split(' ')[1].Split('/');
			using (var fbd = new FolderBrowserDialog())
			{
				DialogResult result = fbd.ShowDialog();

				if (!string.IsNullOrEmpty(fbd.SelectedPath))
				{


					using (WebClient request = new WebClient())
					{
						request.Credentials = new NetworkCredential("anonymous", "janeDoe@contoso.com");
						byte[] fileData = request.DownloadData(ftpPath);

						using (FileStream file = File.Create($"{fbd.SelectedPath}\\{allString[allString.Length-1]}"))
						{
							file.Write(fileData, 0, fileData.Length);
							file.Close();
						}
						System.Windows.Forms.MessageBox.Show("Download Complete");
					}
				}
			}
		}
	}
}