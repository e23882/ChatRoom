using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using WebSocketSharp;

namespace ChatUI.CustomerUserControl
{
	/// <summary>
	/// Barrage.xaml 的互動邏輯
	/// </summary>
	public partial class Barrage: Window
	{
        public WebSocket WebSocketClient { get; set; }

        public Barrage ()
		{
			InitializeComponent();
			this.Deactivated += MainWindow_Deactivated;
			this.StateChanged += MainWindow_StateChanged;
            var server = ConfigurationSettings.AppSettings["Server"];
            WebSocketClient = new WebSocket($"ws://{server}:5566/Connect");
            WebSocketClient.OnMessage += Ws_OnMessage;
            WebSocketClient.Connect();
        }

        [STAThread]
		private void Ws_OnMessage (object sender, MessageEventArgs e)
		{
			try
			{
				var ws = (sender as WebSocket);

				if (ws is null)
				{
					return;
				}

				string receiveData = e.Data;

				if (receiveData.Contains("@All") 
                    || receiveData.Contains("@ALL")
                    || receiveData.Contains("@all"))
				{
                    List<string> list = new List<string>();
                    var allData = receiveData.Split(' ');
                    string message = "";
                    for (int i = 5;i < allData.Length; i++)
                    {
                        message += allData[i];
                    }
                    list.Add(allData[2] + " " + message);
                    BarrageMessage(list);
                }
			}
			catch (Exception ex)
			{
			}
		}

		void MainWindow_StateChanged (object sender, EventArgs e)
		{
			this.WindowState = WindowState.Maximized;
		}
		void MainWindow_Deactivated (object sender, EventArgs e)
		{
			this.Topmost = true;
		}

        /// <summary>
        /// 在Window界面上显示弹幕信息,速度和位置随机产生
        /// </summary>
        /// <param name="contentlist"></param>
        public void BarrageMessage (IEnumerable<string> contentlist)
        {
            Random random = new Random();
            foreach (var item in contentlist)
            {   //获取位置随机数
                double randomtop = random.NextDouble();
                double inittop = canvas.ActualHeight * randomtop;
                //获取速度随机数
                double randomspeed = random.NextDouble();
                double initspeed = 10 * randomspeed;
                
                //实例化动画
                Application.Current.Dispatcher.Invoke(() => {
                    //实例化TextBlock和设置基本属性,并添加到Canvas中
                    TextBlock textblock = new TextBlock();
                    textblock.Text = item;
                    textblock.FontSize = 50;
                    textblock.Foreground = System.Windows.Media.Brushes.Red;
                    Canvas.SetTop(textblock, inittop);
                    canvas.Children.Add(textblock);
                    DoubleAnimation animation = new DoubleAnimation();
                    Timeline.SetDesiredFrameRate(animation, 60);  //如果有性能问题,这里可以设置帧数
                    animation.From = 0;
                    animation.To = canvas.ActualWidth;
                    animation.Duration = TimeSpan.FromSeconds(10);
                    animation.Completed += (object sender, EventArgs e) =>
                    {
                        canvas.Children.Remove(textblock);
                    };
                    //启动动画
                    textblock.BeginAnimation(Canvas.LeftProperty, animation);
                });
                
            }
          
            

        }
    }
}
