using System;
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
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ChatUI.ViewModel
{
	public class MainComponent: ViewModelBase
	{
		#region Declarations
		private FtpFile _SelectedFile = new FtpFile();

		private NotifyIcon nIcon = new NotifyIcon();

		private WindowState _State;

		private Visibility _MainWindowVisibility;
		private Visibility _MainWindowVisibly = Visibility.Visible;

		private bool _ShowInToolBar = true;
		private bool _FlyOutSettingIsOpen = false;

		private int _ShowMessageTime = 60;
		private int _ConnectCount;

		private string _ChatText;
		private string _InPut;
		private string _UserName;
		private string _ConnectStatus = string.Empty;

		private System.Windows.Media.Brush _StatusBackGroundColor;

		private System.Windows.Controls.TextBox _ChatTextBox;

		private ObservableCollection<string> _AllUser = new ObservableCollection<string>();

		private ObservableCollection<FtpFile> _FileList = new ObservableCollection<FtpFile>();
		#endregion

		#region Property
		public ObservableCollection<FtpFile> FileList
		{
			get
			{
				return _FileList;
			}
			set
			{
				_FileList = value;
				OnPropertyChanged();
			}
		}
		public bool FlyOutSettingIsOpen
		{
			get
			{
				return _FlyOutSettingIsOpen;
			}
			set
			{
				_FlyOutSettingIsOpen = value;
				OnPropertyChanged();
			}
		}
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

		public FtpFile SelectedFile
		{
			get
			{
				return _SelectedFile;
			}
			set
			{
				_SelectedFile = value;
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

		public NoParameterCommand ClearTextCommand
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

		public NoParameterCommand SendDataCommand
		{
			get; set;
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
		public NoParameterCommand DownloadFileCommand
		{
			get; set;
		}
		public NoParameterCommand DeleteFileCommand
		{
			get; set;
		}


		/// <summary>
		/// 關閉視窗Command
		/// </summary>
		public NoParameterCommand CloseCommand
		{
			get;
			set;
		}

		public NoParameterCommand ShowSettingCommand
		{
			get; set;
		}

		/// <summary>
		/// 顯示Git main page
		/// </summary>
		public NoParameterCommand ButtonGitClickCommand { get; set; }

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
		public void PressUpButtonCommandAction ()
		{
			InPut = PreviousInput.Replace("\r", "").Replace("\n", "");
		}

		/// <summary>
		/// 初始化主程式工具列圖案、相關功能
		/// </summary>
		public void InitIcon ()
		{
			nIcon.Icon = ChatUI.Properties.Resources.icon;
			nIcon.Visible = false;
			nIcon.MouseDoubleClick += NIcon_MouseDoubleClick;
			#region Init Contextmenu
			var cm = new System.Windows.Forms.ContextMenu();
			try
			{
				nIcon.Icon = ChatUI.Properties.Resources.icon;
				nIcon.Visible = false;
				nIcon.MouseDoubleClick += NIcon_MouseDoubleClick;
				#region Init Contextmenu

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
				#endregion
				nIcon.ContextMenu = cm;
			}
			catch (Exception ex)
			{
				ShowMessage("通知", $"初始化ICon發生例外 {ex.Message}", NotificationType.Error);
			}
		}

		/// <summary>
		/// 右下角工具icon選單項目click事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Mi_Click (object sender, EventArgs e)
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
		private void NIcon_MouseDoubleClick (object sender, System.Windows.Forms.MouseEventArgs e)
		{
			State = WindowState.Normal;
		}

		/// <summary>
		/// 建構子
		/// </summary>
		public MainComponent ()
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
				ShowMessage("通知", $"初始化命令完成", NotificationType.Success);
				InitIcon();
				ShowMessage("通知", $"初始化ICON完成", NotificationType.Success);
				InitConnection();
				ShowMessage("通知", $"初始化連線完成", NotificationType.Success);
				ReadSetting();
				ShowMessage("通知", $"初始化設定", NotificationType.Success);
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show($"初始化發生例外 : {ex.Message}\r\n{ex.StackTrace}");
			}
		}

		private void InitCommand ()
		{
			try
			{
				SendDataCommand = new NoParameterCommand(SendDataCommandAction);
				SendMessageCommand = new NoParameterCommand(SendMessageCommandAction);
				CloseCommand = new NoParameterCommand(CloseCommandAction);
				ButtonGitClickCommand = new NoParameterCommand(ButtonGitClickCommandAction);
				ShowSettingCommand = new NoParameterCommand(ShowSettingCommandAction);
				ClearTextCommand = new NoParameterCommand(ClearTextCommandAction);
				DownloadFileCommand = new NoParameterCommand(DownloadFileCommandAction);
				DeleteFileCommand = new NoParameterCommand(DeleteFileCommandAction);
			}
			catch (Exception ex)
			{
				ShowMessage("通知", $"初始化命令發生例外 {ex.Message}", NotificationType.Error);
			}
		}

		public bool HasChinese (string str)
		{
			return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
		}

		private void DeleteFileCommandAction ()
		{
			try
			{
				FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://10.93.9.117//{SelectedFile.FileName}");
				request.Credentials = new NetworkCredential("anonymous", "anonymous@example.com");
				request.Method = WebRequestMethods.Ftp.DeleteFile;
				FtpWebResponse response = (FtpWebResponse)request.GetResponse();
				response.Close();
				ShowSettingCommandAction();
				ShowSettingCommandAction();
			}
			catch (Exception ex)
			{
				ShowMessage("通知", $"刪除檔案時發生例外 : {ex.Message}", NotificationType.Error);
			}

		}

		private void DownloadFileCommandAction ()
		{
			using (var fbd = new FolderBrowserDialog())
			{
				DialogResult result = fbd.ShowDialog();
				if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
				{
					try
					{
						string ftpServer = $"ftp://10.93.9.117//{SelectedFile.FileName}"; // FTP 服务器地址
						WebClient client = new WebClient();
						client.Credentials = new NetworkCredential("anonymous", "anonymous@example.com");
						client.DownloadFile(ftpServer, $"{fbd.SelectedPath}//{SelectedFile.FileName}");
						ShowMessage("通知", $"下載成功", NotificationType.Success);
					}
					catch (Exception ex)
					{
						ShowMessage("通知", $"下載失敗 : {ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
					}
				}
			}
		}

		private void SendDataCommandAction ()
		{
			try
			{
				OpenFileDialog dialog = new OpenFileDialog();
				if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					if (HasChinese(dialog.SafeFileName))
					{
						ShowMessage("通知", $"不能上傳包含中文的檔案", NotificationType.Warning);
						return;
					}
					string ftpServer = $"ftp://10.93.9.117/{dialog.SafeFileName}"; // FTP 服务器地址

					string tempFolder = System.IO.Path.GetTempPath();
					//檢查是不是網路磁碟機，是的話先抓回來
					if (dialog.FileName.Substring(0, 2).Equals("\\\\"))
					{
						File.Copy(dialog.FileName, tempFolder + "\\" + dialog.SafeFileName, true);


						using (var client = new WebClient())
						{
							client.Credentials = new NetworkCredential("anonymous", "anonymous@example.com");
							client.UploadFile(ftpServer, WebRequestMethods.Ftp.UploadFile, tempFolder + "\\" + dialog.SafeFileName);
						}
						ShowMessage("通知", $"上傳成功", NotificationType.Success);




						FlyOutSettingIsOpen = true;
						WebSocketClient.Send($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {UserName}({CurrentIP}) : {ftpServer}\n");
					}
					else
					{
						// 创建FTP请求对象
						FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(ftpServer));
						request.Method = WebRequestMethods.Ftp.UploadFile;
						request.Credentials = new NetworkCredential("anonymous", "anonymous@example.com"); // 匿名登录凭据
						using (Stream fileStream = File.OpenRead(dialog.FileName))
						using (Stream ftpStream = request.GetRequestStream())
						{
							byte[] buffer = new byte[10240];
							int read;
							while ((read = fileStream.Read(buffer, 0, buffer.Length)) > 0)
							{
								ftpStream.Write(buffer, 0, read);
							}
						}
						ShowMessage("通知", $"上傳成功", NotificationType.Success);
						InPut = $"傳送檔案 : {ftpServer}";
						SendMessage();
						ShowSettingCommandAction();
					}
				}
			}
			catch (Exception ex)
			{
				ShowMessage("通知", $"上傳檔案時發生例外 {ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
			}
		}

		private void ClearTextCommandAction ()
		{
			ChatText = "";
			FlyOutSettingIsOpen = false;

		}

		private void ShowSettingCommandAction ()
		{
			FlyOutSettingIsOpen = !FlyOutSettingIsOpen;
			getFTPFileList();
		}

		private void getFTPFileList ()
		{
			FileList.Clear();
			string ftpServer = "ftp://10.93.9.117"; // FTP 服务器地址

			// 创建FTP请求对象
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(ftpServer));
			request.Method = WebRequestMethods.Ftp.ListDirectory;
			request.Credentials = new NetworkCredential("anonymous", "anonymous@example.com"); // 匿名登录凭据

			// 获取FTP响应对象
			using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
			{
				using (Stream responseStream = response.GetResponseStream())
				{
					using (StreamReader reader = new StreamReader(responseStream))
					{
						// 读取FTP服务器上的文件内容
						string fileContent = reader.ReadToEnd();
						var allFile = fileContent.Split(new string[] { "\r\n" }, StringSplitOptions.None);
						foreach (var item in allFile)
						{
							if (item.Length < 3)
								continue;
							if (string.IsNullOrEmpty(item))
								continue;
							FileList.Add(new FtpFile() { FileName = item });
						}
					}
				}
			}
		}

		private void ButtonGitClickCommandAction ()
		{
			var git = new ucGit();


			git.Show();
		}

		/// <summary>
		/// 檢查設定是否存在
		/// </summary>
		private void ReadSetting ()
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
		private void KillAllProcess ()
		{
			foreach (var process in Process.GetProcessesByName("ChatUI"))
			{
				process.Kill();
			}
		}

		/// <summary>
		/// 關閉視窗按鈕觸發Command
		/// </summary>
		private void CloseCommandAction ()
		{
			State = WindowState.Minimized;
		}

		/// <summary>
		/// 傳送訊息
		/// </summary>
		private void SendMessageCommandAction ()
		{
			SendMessage();
		}

		/// <summary>
		/// 初始化Icon相關
		/// </summary>
		private void InitConnection ()
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
				StatusBackGroundColor = System.Windows.Media.Brushes.LightGreen;
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show($"初始化連線發生例外 : {ex.Message}\r\n{ex.StackTrace}");
				ShowMessage("通知", $"初始化連線發生例外 : {ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
				StatusBackGroundColor = System.Windows.Media.Brushes.Red;
			}
		}

		/// <summary>
		/// WebSocket Server 關閉事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Ws_OnClose (object sender, CloseEventArgs e)
		{
			ConnectStatus = "伺服器中斷連線...嘗試重連....";
			StatusBackGroundColor = System.Windows.Media.Brushes.Orange;
			InitialClient();
		}

		/// <summary>
		/// 初始化WebSocketClient
		/// </summary>
		private void InitialClient ()
		{
			var server = ConfigurationSettings.AppSettings["Server"];
			WebSocketClient = new WebSocket($"ws://{server}:5566/Connect");
			WebSocketClient = new WebSocket($"ws://{server}:1234/Connect");
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
		private void Ws_OnOpen (object sender, EventArgs e)
		{
			ConnectStatus = "伺服器連線成功!";
		}

		/// <summary>
		/// WebSocket Server發送訊息事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Ws_OnMessage (object sender, MessageEventArgs e)
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
								ConnectCount--;
							}
					});
					return;
				}
			}

			ChatText += receiveData;

			if (!receiveData.Contains(UserName))
			{
				ShowMessage("通知", receiveData, NotificationType.Success);

			}
			if (this.ChatTextBox != null)
			{
				App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
				{
					this.ChatTextBox.ScrollToEnd();
				});
			}
		}

		/// <summary>
		/// 傳送訊息
		/// </summary>
		public void SendMessage ()
		{
			if (string.IsNullOrEmpty(InPut))
			{
				return;
			}
			if (!string.IsNullOrEmpty(InPut.Replace("\r", "").Replace("\n", "")))
			{
				string CryptoKey = "54088";
				string result;
				AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
				MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
				SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
				byte[] key = sha256.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));
				byte[] iv = md5.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));
				aes.Key = key;
				aes.IV = iv;

				byte[] dataByteArray = Encoding.UTF8.GetBytes($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {UserName}({CurrentIP}) : {InPut}\n");
				using (MemoryStream ms = new MemoryStream())
				using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
				{
					cs.Write(dataByteArray, 0, dataByteArray.Length);
					cs.FlushFinalBlock();
					result = Convert.ToBase64String(ms.ToArray());
				}
				WebSocketClient.Send(result);
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
		public void ShowMessage (string title, string message, NotificationType type)
		{
			var notificationManager = new NotificationManager();
			var ts = TimeSpan.FromSeconds(ShowMessageTime);
			notificationManager.Show(new NotificationContent
			{
				Title = title,
				Message = message,
				Type = type,
			}, "", ts, () => State = WindowState.Normal);
		}
		#endregion
	}
}
