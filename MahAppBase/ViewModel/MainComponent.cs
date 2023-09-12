using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Net.Sockets;
using System.Diagnostics;
using System.Windows.Forms;
using System.Configuration;
using System.Windows.Interop;
using System.Security.Cryptography;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

using WebSocketSharp;
using Notifications.Wpf;
using Newtonsoft.Json;

namespace ChatUI
{
	public class MainComponent: ViewModelBase
	{
		#region Declarations
		private Thread _ShareThread = null;
		
		private ShareService _server = null;

		private FtpFile _SelectedFile = new FtpFile();

		private NotifyIcon nIcon = new NotifyIcon();

		private WindowState _State = WindowState.Normal;

		private Visibility _MainWindowVisibly = Visibility.Visible;

		private bool _ShowInToolBar = true;
		private bool _FlyOutSettingIsOpen = false;
		private bool _ShareScreen = false;
		private bool _Notify = true;
		private bool _CanSendMessage = true;
		private int sendAllCount = 0;
		private int _ShowMessageTime = 3;
		private int _ConnectCount = 0;

		private double _Opacity = 20;

		private string _ChatText = string.Empty;
		private string _InPut = string.Empty;
		private string _UserName = string.Empty;
		private string _ConnectStatus = string.Empty;
		private string _UpdateBadgeText = "Update";

		private System.Windows.Media.Brush _StatusBackGroundColor;

		private ObservableCollection<UserInfo> _AllUser = new ObservableCollection<UserInfo>();
		private ObservableCollection<FtpFile> _FileList = new ObservableCollection<FtpFile>();
		#endregion

		#region Property
		private Thread ConutSendAllThread { get; set; }
		public ChooseImage ChooseImageWindow{get;set;}

		/// <summary>
		/// 彈幕圖片透明度
		/// </summary>
		public double Opacity 
		{
			get 
			{
				return _Opacity;
			}
			set 
			{
				_Opacity = value;
				Barrage.Opacity = _Opacity/100;
				OnPropertyChanged();
			}
		}
		
		/// <summary>
		/// 彈幕視窗物件實例(用於設定彈幕圖片透明度)
		/// </summary>
		public Barrage1 Barrage { get; set; } = null;

		/// <summary>
		/// Chat Service IP
		/// </summary>
		public string ServerIP { get; set; } = "localhost";

		/// <summary>
		/// 是否可以傳送訊息(用在有人一直發彈幕，鎖住他的介面)
		/// </summary>
		public bool CanSendMessage
		{
			get
			{
				return _CanSendMessage;
			}
			set
			{
				_CanSendMessage = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// 更新按鈕提示文字(Update)
		/// </summary>
		public string UpdateBadgeText
		{
			get
			{
				return _UpdateBadgeText;
			}
			set
			{
				_UpdateBadgeText = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// 是否分享畫面
		/// </summary>
		public bool ShareScreen
		{
			get
			{
				return _ShareScreen;
			}
			set
			{
				_ShareScreen = value;
				try
				{
					//設定分享畫面服務 分享/不分享畫面
					if (_server != null)
					{
						_server.Sharing = value;
					}

					if (_ShareScreen)
					{
						InitShareService();
						InPut = "使用者開始分享畫面";
						SendMessage();
					}
					else
					{
						if (_server != null)
						{
							_server.Sharing = false;
							_server.SocketServer.Stop();
						}
						InPut = "使用者停止分享畫面";
						SendMessage();
					}
				}
				catch (Exception ex)
				{
					ShowMessage("設定是否分享畫面時發生例外", $"{ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
				}

				OnPropertyChanged();
			}
		}

		/// <summary>
		/// FTP Server檔案清單
		/// </summary>
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

		/// <summary>
		/// 設定FlyOut是否打開
		/// </summary>
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
		public WebSocket WebSocketClient { get; set; }

		/// <summary>
		/// 目前使用者IP(避免使用者mock其他使用者)
		/// </summary>
		public string CurrentIP { get; set; }

		/// <summary>
		/// 所有以連線的使用者清單
		/// </summary>
		public ObservableCollection<UserInfo> AllUser
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
		/// 連線使用者數量
		/// </summary>
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
		/// 是否啟用通知
		/// </summary>
		public bool Notify
		{
			get
			{
				return _Notify;
			}
			set
			{
				_Notify = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// 目前選取到的Ftp檔按項目
		/// </summary>
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

		/// <summary>
		/// 清除聊天紀錄
		/// </summary>
		public NoParameterCommand ClearTextCommand { get; set; }

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

		/// <summary>
		/// 主視窗是否顯示
		/// </summary>
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
		/// 看其他人Live Click Command
		/// </summary>
		public RelayCommand WatchLiveCommand { get; set; }

		/// <summary>
		/// 送資料Command
		/// </summary>
		public NoParameterCommand SendDataCommand { get; set; }

		/// <summary>
		/// 傳送訊息Command
		/// </summary>
		public NoParameterCommand SendMessageCommand { get; set; }

		/// <summary>
		/// 按鈕 上command
		/// </summary>
		public NoParameterCommand PressUpButtonCommand { get; set; }

		/// <summary>
		/// 下載FTP檔案Command
		/// </summary>
		public NoParameterCommand DownloadFileCommand { get; set; }

		/// <summary>
		/// 刪除FTP檔案Command
		/// </summary>
		public NoParameterCommand DeleteFileCommand { get; set; }

		/// <summary>
		/// 更新Client Command
		/// </summary>
		public NoParameterCommand UpgradeCommand { get; set; }

		/// <summary>
		/// 關閉視窗Command
		/// </summary>
		public NoParameterCommand CloseCommand { get; set; }

		/// <summary>
		/// 顯示設定視窗Command
		/// </summary>
		public NoParameterCommand ShowSettingCommand { get; set; }

		public NoParameterCommand SendImageCommand { get; set; }

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
							//nIcon.Visible = true;
							ShowInToolBar = true;
							//MainWindowVisibly = Visibility.Hidden;
							return _State;
						case WindowState.Normal:
							//nIcon.Visible = false;
							ShowInToolBar = true;
							//MainWindowVisibly = Visibility.Visible;
							return _State;
					}
				}
				catch (Exception ex)
				{
					//nIcon.Visible = false;
					ShowInToolBar = true;
					MainWindowVisibly = Visibility.Visible;
					ShowMessage("設定主視窗時發生例外", $"{ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
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

		public object Window { get; internal set; }
		#endregion

		#region MemberFunction
		/// <summary>
		/// 按 "上" 按鈕 Command action
		/// </summary>
		public void PressUpButtonCommandAction()
		{
			try
			{
				if (PreviousInput != null)
					InPut = PreviousInput.Replace("\r", "").Replace("\n", "");
			}
			catch (Exception ex)
			{
				ShowMessage("按下上時發生例外", $"{ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
			}
		}

		/// <summary>
		/// 初始化主程式工具列圖案、相關功能
		/// </summary>
		public void InitIcon()
		{
			var cm = new ContextMenu();
			try
			{
				nIcon.Icon = Properties.Resources.icon;
				nIcon.Visible = false;
				nIcon.MouseDoubleClick += NIcon_MouseDoubleClick;

				var miMax = new MenuItem();
				miMax.Text = "放大";
				miMax.Click += Mi_Click;
				cm.MenuItems.Add(miMax);

				var miClose = new MenuItem();
				miClose.Text = "關閉";
				miClose.Click += Mi_Click;
				cm.MenuItems.Add(miClose);

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
		private void Mi_Click(object sender, EventArgs e)
		{
			try
			{
				if ((sender as MenuItem) is null)
					return;

				switch ((sender as MenuItem).Text)
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
			catch (Exception ex)
			{
				ShowMessage("設定工具列選單時發生例外", $"{ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
			}

		}

		/// <summary>
		/// 右下角工具icon double click事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NIcon_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			try
			{
				State = WindowState.Normal;
			}
			catch (Exception ex)
			{
				ShowMessage("工具列ICON Click時發生例外", $"{ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
			}
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
				ShowMessage("通知", $"初始化命令完成", NotificationType.Success);
				InitConnection();
				ShowMessage("通知", $"初始化連線完成", NotificationType.Success);
				ReadSetting();
				ShowMessage("通知", $"初始化設定", NotificationType.Success);
				ChooseImageWindow = new ChooseImage();
				ConutSendAllThread = new Thread(() =>
				{
					
					while (true)
					{
						if (sendAllCount > 0)
						{
							sendAllCount--;
						}
						Thread.Sleep(10 * 1000);
					}
				});
				ConutSendAllThread.Start();


			}
			catch (Exception ex)
			{
				ShowMessage("通知", $"初始化設定發生例外 : {ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
			}
		}

		/// <summary>
		/// 初始化分享畫面服務方法
		/// </summary>
		private void InitShareService()
		{
			try
			{
				_server = new ShareService(6842);
				_ShareThread = new Thread(() =>
				{
					_server.Run();
				});
				_ShareThread.Start();
			}
			catch (Exception ex)
			{
				ShowMessage("通知", $"初始化分享畫面服務發生例外 : {ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
			}
		}

		/// <summary>
		/// 初始化Command
		/// </summary>
		private void InitCommand()
		{
			try
			{
				SendDataCommand = new NoParameterCommand(SendDataCommandAction);
				SendMessageCommand = new NoParameterCommand(SendMessageCommandAction);
				CloseCommand = new NoParameterCommand(CloseCommandAction);
				ShowSettingCommand = new NoParameterCommand(ShowSettingCommandAction);
				ClearTextCommand = new NoParameterCommand(ClearTextCommandAction);
				DownloadFileCommand = new NoParameterCommand(DownloadFileCommandAction);
				DeleteFileCommand = new NoParameterCommand(DeleteFileCommandAction);
				UpgradeCommand = new NoParameterCommand(UpgradeCommandAction);
				WatchLiveCommand = new RelayCommand(WatchLiveCommandAction);
				SendImageCommand = new NoParameterCommand(SendImageCommandAction);
			}
			catch (Exception ex)
			{
				ShowMessage("通知", $"初始化命令發生例外 {ex.Message}", NotificationType.Error);
			}
		}

		/// <summary>
		/// 傳送圖片
		/// </summary>
		private void SendImageCommandAction()
		{
			ChooseImageWindow.Show();
		}

		/// <summary>
		/// 觀看分享畫面
		/// </summary>
		private void WatchLiveCommandAction(object parameter)
		{
			try
			{
				Live live = new Live();
				LiveViewModel viewModel = new LiveViewModel(parameter.ToString());
				if (viewModel.webSocketClient.IsAlive)
				{
					live.DataContext = viewModel;
					live.Show();
				}
			}
			catch (Exception ex)
			{
				ShowMessage("通知", $"使用者沒有在分享畫面", NotificationType.Warning);
			}
		}

		/// <summary>
		/// 更新Client
		/// </summary>
		private void UpgradeCommandAction()
		{
			try
			{
				//將目前設定儲存在UserSetting.ini中
				UserSetting currentSetting = new UserSetting();
				currentSetting.UserName = UserName;
				currentSetting.ShowTime = ShowMessageTime;
				string currentSettingString = JsonConvert.SerializeObject(currentSetting, Formatting.Indented);
				if (File.Exists("UserSetting.ini"))
					File.Delete("UserSetting.ini");
				File.WriteAllText("UserSetting.ini", currentSettingString);

				//啟動更新程式
				Process.Start("Update.exe");
				Environment.Exit(0);
			}
			catch (Exception ex)
			{
				ShowMessage("更新Client時發生例外", $"{ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
			}

		}

		/// <summary>
		/// 判斷有沒有中文字
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public bool HasChinese(string str)
		{
			try
			{
				return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
			}
			catch (Exception ex)
			{
				ShowMessage("判斷是否有中文字時發生例外", $"{ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
				return false;
			}
		}

		/// <summary>
		/// 刪除檔案
		/// </summary>
		private void DeleteFileCommandAction()
		{
			try
			{
				FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{ServerIP}//{SelectedFile.FileName}");
				request.Credentials = new NetworkCredential("anonymous", "anonymous@example.com");
				request.Method = WebRequestMethods.Ftp.DeleteFile;
				FtpWebResponse response = (FtpWebResponse)request.GetResponse();
				response.Close();

				//呼叫兩次觸發設定FlyOut重新刷新取得最新的檔案清單
				ShowSettingCommandAction();
				ShowSettingCommandAction();
			}
			catch (Exception ex)
			{
				ShowMessage("通知", $"刪除檔案時發生例外 : {ex.Message}", NotificationType.Error);
			}
		}

		/// <summary>
		/// 下載FTP檔案
		/// </summary>
		private void DownloadFileCommandAction()
		{
			using (var dialog = new FolderBrowserDialog())
			{
				DialogResult result = dialog.ShowDialog();
				if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
				{
					try
					{
						string ftpServer = $"ftp://{ServerIP}//{SelectedFile.FileName}"; // FTP 服务器地址
						WebClient client = new WebClient();
						client.Credentials = new NetworkCredential("anonymous", "anonymous@example.com");
						client.DownloadFile(ftpServer, $"{dialog.SelectedPath}//{SelectedFile.FileName}");
						ShowMessage("通知", $"下載成功", NotificationType.Success);
					}
					catch (Exception ex)
					{
						ShowMessage("通知", $"下載失敗 : {ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
					}
				}
			}
		}

		/// <summary>
		/// 傳送訊息
		/// </summary>
		private void SendDataCommandAction()
		{
			try
			{
				OpenFileDialog dialog = new OpenFileDialog();
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					if (HasChinese(dialog.SafeFileName))
					{
						ShowMessage("通知", $"不能上傳包含中文的檔案", NotificationType.Warning);
						return;
					}
					string ftpServer = $"ftp://{ServerIP}/{dialog.SafeFileName}";

					string tempFolder = Path.GetTempPath();
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
						FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(ftpServer));
						request.Method = WebRequestMethods.Ftp.UploadFile;
						request.Credentials = new NetworkCredential("anonymous", "anonymous@example.com");
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

		/// <summary>
		/// 清除目前聊天室文字
		/// </summary>
		private void ClearTextCommandAction()
		{
			try
			{
				ChatText = "";
				FlyOutSettingIsOpen = false;
			}
			catch (Exception ex)
			{
				ShowMessage("清除目前聊天室文字時發生例外", $"{ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
			}
		}

		/// <summary>
		/// 顯示設定FlyOut畫面
		/// </summary>
		private void ShowSettingCommandAction()
		{
			try
			{
				FlyOutSettingIsOpen = !FlyOutSettingIsOpen;
				GetFTPFileList();
			}
			catch (Exception ex)
			{
				ShowMessage("設定顯示FlyOut時發生例外", $"{ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
			}
		}

		/// <summary>
		/// 取得FTP Server檔案清單
		/// </summary>
		private void GetFTPFileList()
		{
			try
			{
				FileList.Clear();
				string ftpServer = $"ftp://{ServerIP}/";
				FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(ftpServer));
				request.Method = WebRequestMethods.Ftp.ListDirectory;
				request.Credentials = new NetworkCredential("anonymous", "anonymous@example.com");
				using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
				{
					using (Stream responseStream = response.GetResponseStream())
					{
						using (StreamReader reader = new StreamReader(responseStream))
						{
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
			catch (Exception ex)
			{
				ShowMessage("通知", $"取得FTP檔案清單時發生例外 : {ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
			}
		}

		/// <summary>
		/// 檢查設定是否存在
		/// </summary>
		private void ReadSetting()
		{
			try
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
			catch (Exception ex)
			{
				ShowMessage("讀取使用者設定時發生例外", $"{ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
			}
		}

		/// <summary>
		/// Kill所有殘留應用程式instance
		/// </summary>
		private void KillAllProcess()
		{
			try
			{
				foreach (var process in Process.GetProcessesByName("ChatUI"))
				{
					process.Kill();
				}
			}
			catch (Exception ex)
			{
				ShowMessage("刪除殘留Process時發生例外", $"{ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
			}

		}

		/// <summary>
		/// 關閉視窗按鈕觸發Command
		/// </summary>
		private void CloseCommandAction()
		{
			try
			{
				State = WindowState.Minimized;
			}
			catch (Exception ex)
			{
				ShowMessage("最小化視窗時發生例外", $"{ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
			}
		}

		/// <summary>
		/// 傳送訊息
		/// </summary>
		private void SendMessageCommandAction()
		{
			try
			{
				SendMessage();
			}
			catch (Exception ex)
			{
				ShowMessage("傳送訊息時發生例外", $"{ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
			}
		}

		/// <summary>
		/// 初始化Icon相關
		/// </summary>
		private void InitConnection()
		{
			try
			{
				ServerIP = ConfigurationSettings.AppSettings["Server"];
				WebSocketClient = new WebSocket($"ws://{ServerIP}:5566/Connect");
				WebSocketClient.OnMessage += Ws_OnMessage;
				WebSocketClient.OnOpen += Ws_OnOpen;
				WebSocketClient.OnClose += Ws_OnClose;
				WebSocketClient.Connect();
				ConnectStatus = "伺服器連線成功!";
				StatusBackGroundColor = System.Windows.Media.Brushes.LightGreen;
			}
			catch (Exception ex)
			{
				ShowMessage("通知", $"初始化連線發生例外 : {ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
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
			try
			{
				ConnectStatus = "伺服器中斷連線...嘗試重連....";
				StatusBackGroundColor = System.Windows.Media.Brushes.Orange;
				InitialClient();
			}
			catch (Exception ex)
			{
				ShowMessage("出發WebSocket OnClose事件時發生例外", $"{ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
			}
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
			try
			{
				ConnectStatus = "伺服器連線成功!";
			}
			catch (Exception ex)
			{
				ShowMessage("觸發WebSocket OnOpen事件時發生例外", $"{ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
			}
		}

		/// <summary>
		/// WebSocket Server發送訊息事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[DllImport("user32")] public static extern int FlashWindow(IntPtr hwnd, bool bInvert);
		private void Ws_OnMessage(object sender, MessageEventArgs e)
		{
			try
			{
				var ws = (sender as WebSocket);

				if (ws is null)
				{
					return;
				}
				//接收到的訊息
				string receiveData = e.Data;

				var sendMessage = FilterMessage(receiveData);
				if (!sendMessage)
				{
					return;
				}


				if (receiveData.Contains("目前已連線使用者"))
				{
					var CurrentAddUserID = receiveData.Split(' ')[1];

					App.Current.Dispatcher.Invoke((Action)delegate
					{
						var userIP = CurrentAddUserID.Split(':')[0];
						if (!AllUser.Any(x => x.UserIP == userIP))
						{
							ConnectCount++;
							AllUser.Add(new UserInfo()
							{
								UserIP = userIP,
								IsLive = Visibility.Collapsed
							});
						}
					});
					return;
				}

				if (receiveData.Contains("開始分享畫面"))
				{
					var CurrentLivingUserIP = receiveData.Split(' ')[2].Split('(')[1].Replace(")", "");
					App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
					{

						if (AllUser.Any(x => x.UserIP == CurrentLivingUserIP))
						{
							AllUser.Where(x => x.UserIP == CurrentLivingUserIP).FirstOrDefault().IsLive = Visibility.Visible;
						}
					});
					return;
				}

				if (receiveData.Contains("停止分享畫面"))
				{
					var CurrentLivingUserIP = receiveData.Split(' ')[2].Split('(')[1].Replace(")", "");
					App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
					{

						if (AllUser.Any(x => x.UserIP == CurrentLivingUserIP))
						{
							AllUser.Where(x => x.UserIP == CurrentLivingUserIP).FirstOrDefault().IsLive = Visibility.Collapsed;
						}
					});
					return;
				}


				if (receiveData.Contains("使用者") && receiveData.Contains("加入聊天"))
				{
					bool alreadyJoin = false;
					foreach (var item in AllUser)
					{
						if (receiveData.Contains(item.UserIP))
						{
							alreadyJoin = true;
							break;
						}
					}
					if (!alreadyJoin)
					{
						ShowMessage("通知", receiveData, NotificationType.Success);
						var allLloginMessage = receiveData.Split(' ');
						if (allLloginMessage.Length > 2)
						{
							App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
							{
								var userIP = allLloginMessage[1].Split(':')[0];
								if (!AllUser.Any(x => x.UserIP == userIP))
								{
									AllUser.Add(new UserInfo()
									{
										UserIP = userIP,
										IsLive = Visibility.Collapsed
									});
									ConnectCount++;
								}
							});
							return;
						}
					}

				}

				if (receiveData.Contains("使用者") && receiveData.Contains("離開聊天"))
				{
					ShowMessage("通知", receiveData, NotificationType.Success);
					var allLloginMessage = receiveData.Split(' ');
					if (allLloginMessage.Length > 2)
					{
						App.Current.Dispatcher.Invoke((Action)delegate
						{
							var userIP = allLloginMessage[1].Split(':')[0];
							if (AllUser.Any(x => x.UserIP == userIP))
							{
								AllUser.Remove(AllUser.Where(x => x.UserIP == userIP).FirstOrDefault());
								ConnectCount--;
							}
						});
						return;
					}
				}

				ChatText += receiveData.Replace("\n\n", "\n");
				if (receiveData.Contains($"@{UserName}"))
				{
					switch (State)
					{
						case WindowState.Maximized:
						case WindowState.Normal:
							ShowMessage("通知", receiveData, NotificationType.Success);
							App.Current.Dispatcher.Invoke((Action)delegate
							{
								WindowInteropHelper wih = new WindowInteropHelper((Window)Window);
								FlashWindow(wih.Handle, true);

							});
							break;
						case WindowState.Minimized:
							ShowMessage("通知", receiveData, NotificationType.Success);
							State = WindowState.Minimized;

							ShowInToolBar = true;
							MainWindowVisibly = Visibility.Visible;
							Thread th = new Thread(() =>
							{
								while (State != WindowState.Normal)
								{
									App.Current.Dispatcher.Invoke((Action)delegate
									{
										WindowInteropHelper wih = new WindowInteropHelper((Window)Window);
										FlashWindow(wih.Handle, true);
									});
									Thread.Sleep(1000);
								}
							});
							th.Start();
							break;
					}
				}

				if (!receiveData.Contains(UserName))
				{
					ShowMessage("通知", receiveData, NotificationType.Success);
				}
			}
			catch (Exception ex)
			{
				ShowMessage("觸發WebSocket OnMessage事件時發生例外", $"{ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
			}
		}

		/// <summary>
		/// 篩選訊息(篩選彈幕圖片訊息)
		/// </summary>
		/// <param name="receiveData"></param>
		/// <returns></returns>
		private bool FilterMessage(string receiveData)
		{
			if (receiveData.Contains("[img]"))
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// 傳送訊息
		/// </summary>
		public void SendMessage()
		{
			try
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
					if (InPut.Contains("ALL") || InPut.Contains("All") || InPut.Contains("all"))
					{
						sendAllCount++;
					}
					else
					{
						sendAllCount = 0;
					}

					if (sendAllCount > 2)
					{
						ShowMessage("傳太多次彈幕了，不讓你傳", $"不讓你傳，鎖定15秒", NotificationType.Error);

						CanSendMessage = false;
						App.Current.Dispatcher.Invoke((Action)delegate
						{
							Thread th = new Thread(() =>
							{
								//鎖定十秒
								Thread.Sleep(15 * 1000);
								CanSendMessage = true;
								sendAllCount = 0;
							});
							th.Start();
						});
					}
					else
					{
						WebSocketClient.Send(result);
						PreviousInput = InPut;
						InPut = "";
					}

				}
			}
			catch (Exception ex)
			{
				ShowMessage("傳送訊息時發生例外", $"{ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
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
			try
			{
				if (!Notify && (type == NotificationType.Success || type == NotificationType.Information))
				{
					return;
				}

				var notificationManager = new NotificationManager();
				if (ShowMessageTime != 0)
				{
					var ts = TimeSpan.FromSeconds(ShowMessageTime);
					notificationManager.Show(new NotificationContent
					{
						Title = title,
						Message = message,
						Type = type,
					}, "", ts, () => State = WindowState.Normal);
				}
			}
			catch (Exception ex)
			{
				ShowMessage("顯示訊息時發生例外", $"{ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
			}
		}
		#endregion
	}
}
