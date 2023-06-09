﻿using System;
using System.Configuration;
using WebSocketSharp;
using System.Windows;
using System.Windows.Forms;
using ChatUI.Command;
using ChatUI.CustomerUserControl;
using System.Net;
using System.Net.Sockets;
using Notifications.Wpf;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using System.Threading;
using System.Drawing;

namespace ChatUI.ViewModel
{
	public class MainComponent : ViewModelBase
	{
		#region Declarations
		private NotifyIcon nIcon = new NotifyIcon();

		private ucDonate donate = new ucDonate();

		private WindowState _State;

		private Visibility _MainWindowVisibility;

		private bool _ShowInToolBar = true;

		private int _ShowMessageTime = 60;

		private string _ChatText;
		private string _InPut = "";
		private string _UserName;
		private string _ConnectStatus = string.Empty;

		private System.Windows.Media.Brush _StatusBackGroundColor;

		private System.Windows.Controls.TextBox _ChatTextBox;

		private ObservableCollection<string> _AllUser = new ObservableCollection<string>();

		private ObservableCollection<ReceiveMessage> _TextProperty = new ObservableCollection<ReceiveMessage>();
		public ObservableCollection<ReceiveMessage> TextProperty
		{
			get
			{
				return _TextProperty;
			}
			set
			{
				_TextProperty = value;
			}
		}
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

		/// <summary>
		/// 目前使用者IP(避免使用者mock其他使用者)
		/// </summary>
		public string CurrentIP { get; set; }

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

		private int _ConnectCount;
		public int ConnectCount
		{
			get
			{
				return _ConnectCount;
			}
			set
			{
				_ConnectCount = value;
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

		/// <summary>
		/// 前一個輸入的訊息(用於按下上顯示上一個訊息)
		/// </summary>
		public string PreviousInput { get; set; }

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

		public NoParameterCommand ButtonImageSendCommand { get; set; }

		/// <summary>
		/// Donate click
		/// </summary>
		public NoParameterCommand ButtonDonateClickCommand
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
		public System.Windows.Media.Brush StatusBackGroundColor
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

		private Visibility _MainWindowVisibly = Visibility.Visible;
		public Visibility MainWindowVisibly
		{
			get
			{
				return _MainWindowVisibly;
			}
			set
			{
				_MainWindowVisibly = value;
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
		/// 顯示Git main page
		/// </summary>
		public NoParameterCommand ButtonGitClickCommand { get; set; }

		public NoParameterCommand ButtonFileSendCommand { get; set; }

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
				try
				{
					switch (_State)
					{
						case WindowState.Minimized:
							nIcon.Visible = true;
							ShowInToolBar = false;
							MainWindowVisibly = Visibility.Hidden;
							return _State;
						case WindowState.Normal:
							nIcon.Visible = false;
							ShowInToolBar = true;
							MainWindowVisibly = Visibility.Visible;
							return _State;
					}
				}
				catch
				{
					nIcon.Visible = false;
					ShowInToolBar = true;
					MainWindowVisibly = Visibility.Visible;
					return _State;
				}
				return _State;
			}
			set
			{
				_State = value;

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
		/// 按 "上" 按鈕 Command action
		/// </summary>
		public void PressUpButtonCommandAction()
		{
			InPut = PreviousInput.Replace("\r", "").Replace("\n", "");
		}

		/// <summary>
		/// 初始化主程式工具列圖案、相關功能
		/// </summary>
		public void InitIcon()
		{
			nIcon.Icon = ChatUI.Properties.Resources.icon;
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
						CurrentIP = UserName;
						break;
					}
				}
				InitCommand();
				InitIcon();
				InitConnection();
				ReadSetting();
			}
			catch (Exception ex)
			{
				ShowMessage("錯誤", $"初始化發生例外 : {ex.Message}", NotificationType.Error);
			}
		}

		private void InitCommand()
		{
			ButtonDonateClickCommand = new NoParameterCommand(ButtonDonateClickAction);
			SendMessageCommand = new NoParameterCommand(SendMessageCommandAction);
			CloseCommand = new NoParameterCommand(CloseCommandAction);
			ButtonGitClickCommand = new NoParameterCommand(ButtonGitClickCommandAction);
			ButtonImageSendCommand = new NoParameterCommand(ButtonImageSendCommandAction);
			ButtonFileSendCommand = new NoParameterCommand(ButtonFileSendCommandAction);
		}

		private void ButtonFileSendCommandAction()
		{
			OpenFileDialog openFileDialog1 = new OpenFileDialog
			{
				InitialDirectory = @"D:\",
				Title = "選取要傳送的檔案",
				CheckFileExists = true,
				CheckPathExists = true,
				RestoreDirectory = true,
				ReadOnlyChecked = true,
				ShowReadOnly = true
			};

			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				try
				{
					var server = ConfigurationSettings.AppSettings["Server"];
					//using (Image image = Image.FromFile(openFileDialog1.FileName))
					using (var client = new WebClient())
					{
						client.Credentials = new NetworkCredential("anonymous", "janeDoe@contoso.com");
						client.UploadFile($"ftp://{server}/{openFileDialog1.SafeFileName}", WebRequestMethods.Ftp.UploadFile, openFileDialog1.FileName);
					}
					InPut = $"!File:ftp://{server}/{openFileDialog1.SafeFileName}";
					SendMessage();
				}
				catch (Exception ex)
				{
					ShowMessage("錯誤", $"傳送檔案時發生例外 : {ex.Message}", NotificationType.Error);
				}
			}
		}

		private void ButtonImageSendCommandAction()
		{
			OpenFileDialog openFileDialog1 = new OpenFileDialog
			{
				InitialDirectory = @"D:\",
				Title = "選取圖片",
				Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;...",
				CheckFileExists = true,
				CheckPathExists = true,
				RestoreDirectory = true,
				ReadOnlyChecked = true,
				ShowReadOnly = true
			};

			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				try 
				{
					using (Image image = Image.FromFile(openFileDialog1.FileName))
					{
						using (MemoryStream m = new MemoryStream())
						{
							image.Save(m, image.RawFormat);
							byte[] imageBytes = m.ToArray();

							// Convert byte[] to Base64 String
							string base64String = Convert.ToBase64String(imageBytes);
							InPut = $"!Image:{base64String}";
						}
					}
					
					SendMessage();
				}
				catch(Exception ex) 
				{
					ShowMessage("錯誤", $"傳送圖片時發生例外 : {ex.Message}", NotificationType.Error);
				}
			}
		}

		private void ButtonGitClickCommandAction()
		{
			var git = new ucGit();


			git.Show();
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
				string defaultSetting = "{\"UserName\":\"" + localUserName + "\", \"ShowTime\":30}";
				File.WriteAllText("UserSetting.ini", defaultSetting);
			}
		}

		/// <summary>
		/// Kill所有殘留應用程式instance
		/// </summary>
		private void KillAllProcess()
		{
			foreach (var process in Process.GetProcessesByName("ChatUI"))
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
				WebSocketClient = new WebSocket($"ws://{server}:5577/Connect");
				WebSocketClient.OnMessage += Ws_OnMessage;
				WebSocketClient.OnOpen += Ws_OnOpen;
				WebSocketClient.OnClose += Ws_OnClose;
				WebSocketClient.Connect();
				ConnectStatus = "伺服器連線成功!";
				StatusBackGroundColor = System.Windows.Media.Brushes.LightGreen;
			}
			catch (Exception ex)
			{
				ShowMessage("錯誤", $"初始化連線發生例外 : {ex.Message}", NotificationType.Error);
				StatusBackGroundColor = System.Windows.Media.Brushes.Red;
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
			StatusBackGroundColor = System.Windows.Media.Brushes.Orange;
			InitialClient();
		}

		/// <summary>
		/// 初始化WebSocketClient
		/// </summary>
		private void InitialClient()
		{
			var server = ConfigurationSettings.AppSettings["Server"];
			WebSocketClient = new WebSocket($"ws://{server}:5566/Connect");
			WebSocketClient.OnMessage += Ws_OnMessage;
			WebSocketClient.OnOpen += Ws_OnOpen;
			WebSocketClient.OnClose += Ws_OnClose;
			while (ConnectStatus != "伺服器連線成功")
			{
				try
				{
					WebSocketClient.Connect();
					ConnectStatus = "伺服器連線成功!";
					StatusBackGroundColor = System.Windows.Media.Brushes.Green;
				}
				catch (Exception ex)
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

			if (receiveData.Contains("目前已連線使用者"))
			{
				var CurrentAddUserID = receiveData.Split(' ')[1];
				App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
				{
					if (!AllUser.Contains(CurrentAddUserID.Split(':')[0])) 
					{
						AllUser.Add(CurrentAddUserID.Split(':')[0]);
						ConnectCount++;
					}
				});
				return;
			}
			
			if (receiveData.Contains("使用者") && receiveData.Contains("加入聊天"))
			{
				ShowMessage("通知", receiveData, NotificationType.Success);
				var allLloginMessage = receiveData.Split(' ');
				if (allLloginMessage.Length > 2)
				{
					App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
					{
						if (!AllUser.Contains(allLloginMessage[1].Split(':')[0])) 
						{
							AllUser.Add(allLloginMessage[1].Split(':')[0]);
							ConnectCount++;
						}
					});
					return;
				}
			}
			if (receiveData.Contains("使用者") && receiveData.Contains("離開聊天"))
			{
				ShowMessage("通知", receiveData, NotificationType.Success);
				var allLloginMessage = receiveData.Split(' ');
				if (allLloginMessage.Length > 2)
				{
					App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
					{
						if (!AllUser.Contains(allLloginMessage[1].Split(':')[0])) 
						{
							AllUser.Remove(allLloginMessage[1].Split(':')[0]);
							ConnectCount++;
						}
					});
					return;
				}
			}
			ReceiveMessage formatData = JsonConvert.DeserializeObject<ReceiveMessage>(receiveData);
			switch (formatData.Type) 
			{
				case "Image":
					if (formatData.UserName !=UserName)
					{
						ShowMessage("通知", $"{formatData.UserName} 傳送圖片", NotificationType.Success);
					}
					formatData.Message = formatData.Message.Replace("!Image:", "");
					byte[] bytes = Convert.FromBase64String(formatData.Message);
					Image image;
					using (MemoryStream ms = new MemoryStream(bytes))
					{
						image = Image.FromStream(ms);
					}
					var currentPath = $"{Guid.NewGuid().ToString()}.jpg";
					image.Save(currentPath);
					formatData.Message = $"{System.AppDomain.CurrentDomain.BaseDirectory}{currentPath}";
					App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
					{
						TextProperty.Add(formatData);
					});
					
					break;
				case "Text":
					if (formatData.UserName != UserName)
					{
						ShowMessage("通知", $"{formatData.UserName} : {formatData.Message}", NotificationType.Success);
					}
					App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
					{
						TextProperty.Add(formatData);
					});
					break;
				case "File":
					if (formatData.UserName != UserName)
					{
						ShowMessage("通知", $"{formatData.UserName} 傳送檔案", NotificationType.Success);
					}
					formatData.Message = "下載 "+formatData.Message.Replace("!File:", "");
					App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
					{
						TextProperty.Add(formatData);
					});
					break;
			}

			

		}

		/// <summary>
		/// 傳送訊息
		/// </summary>
		public void SendMessage()
		{
			if (!string.IsNullOrEmpty(InPut.Replace("\r", "").Replace("\n", "")))
			{
				string output = "";
				ReceiveMessage text = new ReceiveMessage();
				text.SentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
				text.UserName = UserName;
				text.Message = InPut;
				if (InPut.Contains("!Image"))
				{
					text.Type = "Image";
				}
				else if (InPut.Contains("!File"))
				{
					text.Type = "File";
				}
				else 
				{
					text.Type = "Text";
				}
				output = JsonConvert.SerializeObject(text);
				PreviousInput = InPut;
				InPut = "";
				WebSocketClient.Send(output);
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
			}, "", ts, ()=> State = WindowState.Normal);

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