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
using System.Windows.Shapes;

namespace ChatUI.CustomerUserControl
{
	/// <summary>
	/// ucShowImage.xaml 的互動邏輯
	/// </summary>
	public partial class ucShowImage : Window
	{
		public ucShowImage(ImageSource imageSource)
		{
			InitializeComponent();
			this.imgMain.Source = imageSource;
		}
	}
}
