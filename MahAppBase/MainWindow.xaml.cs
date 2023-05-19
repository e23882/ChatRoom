using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
            this.DataContext = viewModel;
        }

		private void MetroWindow_Closed(object sender, EventArgs e)
		{
            viewModel.State = WindowState.Minimized;
        }

		private void TextBox_KeyUp(object sender, KeyEventArgs e)
		{
            if (e.Key != System.Windows.Input.Key.Enter) return;
            viewModel.SendMessage();
            tbChat.ScrollToEnd();
            //viewModel
        }
	}
}