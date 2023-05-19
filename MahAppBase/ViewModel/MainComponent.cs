using System;
using System.Configuration;
using WebSocketSharp;
using System.Windows;
using System.Windows.Forms;
using MahAppBase.Command;
using MahAppBase.CustomerUserControl;
using System.Net;
using System.Net.Sockets;
using Notifications.Wpf;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using System.Threading;

namespace MahAppBase.ViewModel
{
	public class MainComponent : ViewModelBase
	{
		#region Declarations
		private NotifyIcon nIcon = new NotifyIcon();

		private ucDonate donate = new ucDonate();

		private WindowState _State;
		
		private Visibility _MainWindowVisibility;

		private bool _ShowInToolBar;

		private int _ShowMessageTime = 60;

		private string _ChatText;
		private string _InPut;
		private string _UserName;
		private string _ConnectStatus = string.Empty;

		private Brush _StatusBackGroundColor = new SolidColorBrush(Colors.Red);

		private System.Windows.Controls.TextBox _ChatTextBox;

		private ObservableCollection<string> _AllUser = new ObservableCollection<string>();
		#endregion

		#region Property
		/// <summary>
		/// WebSocket Client物件實例
		/// </summary>
		public WebSocket WebSocketClient 
		{
			get; 
			set; 
		}

		public void PressUpButtonCommandAction()
		{
			InPut = PreviousInput.Replace("\r", "").Replace("\n", "");
		}

		/// <summary>
		/// 所有以連線的使用者清單
		/// </summary>
		public ObservableCollection<string> AllUser
		{
			get
			{
				return _AllUser;
			}
			set
			{
				_AllUser = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// 訊息彈出視窗顯示時間
		/// </summary>
		public int ShowMessageTime
		{
			get
			{
				return _ShowMessageTime;
			}
			set
			{
				_ShowMessageTime = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// 聊天的訊息
		/// </summary>
		public string ChatText
		{
			get
			{
				return _ChatText;
			}
			set
			{
				_ChatText = value;
				OnPropertyChanged();
			}
		}

		public string PreviousInput { get;  set; }

		/// <summary>
		/// 使用者輸入的訊息
		/// </summary>
		public string InPut
		{
			get
			{
				return _InPut;
			}
			set
			{
				_InPut = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// 使用者名稱
		/// </summary>
		public string UserName
		{
			get
			{
				return _UserName;
			}
			set
			{
				_UserName = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Donate click
		/// </summary>
		public NoParameterCommand ButtonDonateClick 
		{
			get; 
			set; 
		}

		/// <summary>
		/// 主畫面顯示對話textbox物件實例(用在Scroll to end)
		/// </summary>
		public System.Windows.Controls.TextBox ChatTextBox
		{
			get
			{
				return _ChatTextBox;
			}
			set
			{
				_ChatTextBox = value;
			}
		}

		/// <summary>
		/// 與WebSocket Server狀態背景顏色
		/// </summary>
		public Brush StatusBackGroundColor
		{
			get
			{
				return _StatusBackGroundColor;
			}
			set
			{
				_StatusBackGroundColor = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// 傳送訊息Command
		/// </summary>
		public NoParameterCommand SendMessageCommand
		{
			get; 
			set;
		}

		/// <summary>
		/// 按鈕 上command
		/// </summary>
		public NoParameterCommand PressUpButtonCommand
		{
			get; 
			set;
		}

		/// <summary>
		/// 關閉視窗Command
		/// </summary>
		public NoParameterCommand CloseCommand
		{
			get; 
			set;
		}
	
		/// <summary>
		/// 主程式執行時，ICON是否在window toolbar(最小化不顯示)
		/// </summary>
		public bool ShowInToolBar
		{
			get
			{
				return _ShowInToolBar;
			}
			set
			{
				_ShowInToolBar = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Donate視窗是否開啟
		/// </summary>
		public bool DonateIsOpen 
		{ 
			get; 
			set; 
		}
		
		/// <summary>
		/// 主程式是否顯示(最小化時不顯示)
		/// </summary>
		public Visibility MainWindowVisibility
		{
			get
			{
				return _MainWindowVisibility;
			}
			set
			{
				_MainWindowVisibility = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// 主視窗視窗狀態
		/// </summary>
		public WindowState State
		{
			get
			{
				return _State;
			}
			set
			{
				_State = value;
				switch (_State)
				{
					case WindowState.Minimized:
						MainWindowVisibility = Visibility.Hidden;
						nIcon.Visible = true;
						ShowInToolBar = false;
						break;
					case WindowState.Normal:
						MainWindowVisibility = Visibility.Visible;
						nIcon.Visible = false;
						ShowInToolBar = true;
						break;
					case WindowState.Maximized:
						MainWindowVisibility = Visibility.Visible;
						nIcon.Visible = false;
						ShowInToolBar = true;
						break;
				}
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// 目前與WebSocket Server連線狀態
		/// </summary>
		public string ConnectStatus
		{
			get
			{
				return _ConnectStatus;
			}
			set
			{
				_ConnectStatus = value;
				OnPropertyChanged();
			}
		}
		#endregion

		#region MemberFunction
		/// <summary>
		/// 初始化主程式工具列圖案、相關功能
		/// </summary>
		public void InitIcon()
		{
			nIcon.Icon = MahAppBase.Properties.Resources.icon;
			nIcon.Visible = false;
			nIcon.MouseDoubleClick += NIcon_MouseDoubleClick;
			#region Init Contextmenu
			var cm = new System.Windows.Forms.ContextMenu();

			var miMax = new System.Windows.Forms.MenuItem();
			miMax.Text = "放大";
			miMax.Click += Mi_Click;
			cm.MenuItems.Add(miMax);

			var miClose = new System.Windows.Forms.MenuItem();
			miClose.Text = "關閉";
			miClose.Click += Mi_Click;
			cm.MenuItems.Add(miClose);
			#endregion
			nIcon.ContextMenu = cm;
		}

		/// <summary>
		/// 右下角工具icon選單項目click事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Mi_Click(object sender, EventArgs e)
		{
			if ((sender as System.Windows.Forms.MenuItem) is null)
				return;

			switch ((sender as System.Windows.Forms.MenuItem).Text)
			{
				case "放大":
					State = WindowState.Normal;
					break;
				case "關閉":
					UserSetting currentSetting = new UserSetting();
					currentSetting.UserName = UserName;
					currentSetting.ShowTime = ShowMessageTime;
					string currentSettingString = JsonConvert.SerializeObject(currentSetting, Formatting.Indented);
					if (File.Exists("UserSetting.ini")) 
						File.Delete("UserSetting.ini");
					File.WriteAllText("UserSetting.ini", currentSettingString);
					KillAllProcess();
					Environment.Exit(0);
					break;
			}
		}

		/// <summary>
		/// 右下角工具icon double click事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			State = WindowState.Normal;
		}
		
		/// <summary>
		/// 建構子
		/// </summary>
		public MainComponent()
		{
			try
			{
				
				var host = Dns.GetHostEntry(Dns.GetHostName());
				foreach (var ip in host.AddressList)
				{
					if (ip.AddressFamily == AddressFamily.InterNetwork)
					{
						UserName = ip.ToString();
						break;
					}
				}
				ButtonDonateClick = new NoParameterCommand(ButtonDonateClickAction);
				SendMessageCommand = new NoParameterCommand(SendMessageCommandAction);
				CloseCommand = new NoParameterCommand(CloseCommandAction);
				InitIcon();
				InitConnection();
				ReadSetting();
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show($"初始化發生例外 : {ex.Message}\r\n{ex.StackTrace}");
			}
		}

		/// <summary>
		/// 檢查設定是否存在
		/// </summary>
		private void ReadSetting()
		{
			//如果設定存在讀取設定
			if (File.Exists("UserSetting.ini")) 
			{
				var setting = File.ReadAllText("UserSetting.ini");
				UserSetting convertSetting = JsonConvert.DeserializeObject<UserSetting>(setting);
				UserName = convertSetting.UserName;
				ShowMessageTime = convertSetting.ShowTime;
			}
			//如果設定不存在建立設定
			else
			{
				string localUserName = "";
				var host = Dns.GetHostEntry(Dns.GetHostName());
				foreach (var ip in host.AddressList)
				{
					if (ip.AddressFamily == AddressFamily.InterNetwork)
					{
						localUserName = ip.ToString();
						break;
					}
				}
				string defaultSetting = "{\"UserName\":\""+ localUserName + "\", \"ShowTime\":30}";
				File.WriteAllText("UserSetting.ini", defaultSetting);
			}
		}

		/// <summary>
		/// Kill所有殘留應用程式instance
		/// </summary>
		private void KillAllProcess()
		{
			foreach (var process in Process.GetProcessesByName("MahAppBase"))
			{
				process.Kill();
			}
		}

		/// <summary>
		/// 關閉視窗按鈕觸發Command
		/// </summary>
		private void CloseCommandAction()
		{
			State = WindowState.Minimized;
		}

		/// <summary>
		/// 傳送訊息
		/// </summary>
		private void SendMessageCommandAction()
		{
			SendMessage();
		}

		/// <summary>
		/// 初始化Icon相關
		/// </summary>
		private void InitConnection()
		{
			try
			{
				var server = ConfigurationSettings.AppSettings["Server"];
				WebSocketClient = new WebSocket($"ws://{server}:5566/Connect");
				WebSocketClient.OnMessage += Ws_OnMessage;
				WebSocketClient.OnOpen += Ws_OnOpen;
				WebSocketClient.OnClose += Ws_OnClose;
				WebSocketClient.Connect();
				ConnectStatus = "伺服器連線成功!";
				StatusBackGroundColor = Brushes.LightGreen;
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show($"初始化連線發生例外 : {ex.Message}\r\n{ex.StackTrace}");
				StatusBackGroundColor = Brushes.Red;
			}
		}
		
		/// <summary>
		/// WebSocket Server 關閉事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Ws_OnClose(object sender, CloseEventArgs e)
		{
			ConnectStatus = "伺服器中斷連線...嘗試重連....";
			StatusBackGroundColor = Brushes.Orange;
			InitialClient();
		}
		
		/// <summary>
		/// 初始化WebSocketClient
		/// </summary>
		private void InitialClient()
		{
			var server = ConfigurationSettings.AppSettings["Server"];
			WebSocketClient = new WebSocket($"WebSocketClient://{server}:5566/Connect");
			WebSocketClient.OnMessage += Ws_OnMessage;
			WebSocketClient.OnOpen += Ws_OnOpen;
			WebSocketClient.OnClose += Ws_OnClose;
			while (ConnectStatus != "伺服器連線成功") 
			{
				try 
				{
					WebSocketClient.Connect();
					ConnectStatus = "伺服器連線成功!";
					StatusBackGroundColor = Brushes.Green;
				}
				catch(Exception ex) 
				{
					Thread.Sleep(3000);
				}
			}
		}

		/// <summary>
		/// WebSocket Server開啟事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Ws_OnOpen(object sender, EventArgs e)
		{
			ConnectStatus = "伺服器連線成功!";
		}
		
		/// <summary>
		/// WebSocket Server發送訊息事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Ws_OnMessage(object sender, MessageEventArgs e)
		{
			var ws = (sender as WebSocket);

			if (ws is null)
			{
				return;
			}

			string receiveData = e.Data;
			if (receiveData.Length > 40)
			{
				if (receiveData.Contains("使用者"))
				{
					var allLloginMessage = receiveData.Split(' ');
					if (allLloginMessage.Length > 2)
					{
						AllUser.Add(allLloginMessage[1]);
					}
				}

			}

			ChatText += receiveData;
			if (!receiveData.Contains(UserName))
			{
				ShowMessage("通知", receiveData, NotificationType.Success);
				if (this.ChatTextBox != null) 
					this.ChatTextBox.ScrollToEnd();
			}
		}
		
		/// <summary>
		/// 傳送訊息
		/// </summary>
		public void SendMessage()
		{
			if (!string.IsNullOrEmpty(InPut))
			{
				//render message
				//ChatText += $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {UserName} : {InPut}";
				//send message
				WebSocketClient.Send($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {UserName} : {InPut}\n");
				//clear input
				PreviousInput = InPut;
				InPut = "";
			}
		}

		/// <summary>
		/// 顯示訊息
		/// </summary>
		/// <param name="title"></param>
		/// <param name="message"></param>
		/// <param name="type"></param>
		public void ShowMessage(string title, string message, NotificationType type)
		{
			var notificationManager = new NotificationManager();
			var ts = TimeSpan.FromSeconds(ShowMessageTime);
			notificationManager.Show(new NotificationContent
			{
				Title = title,
				Message = message,
				Type = type,
			}, "", ts);

		}
		
		/// <summary>
		/// Donate功能Click Command
		/// </summary>
		public void ButtonDonateClickAction()
		{
			if (!DonateIsOpen)
			{
				donate = new ucDonate
				{
					Topmost = true
				};
				donate.Closed += Donate_Closed;
				DonateIsOpen = true;
				donate.Show();
			}
			else
			{

			}
		}
		
		/// <summary>
		/// Donate功能關閉事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Donate_Closed(object sender, EventArgs e)
		{
			DonateIsOpen = false;
		}

		#endregion
	}
}