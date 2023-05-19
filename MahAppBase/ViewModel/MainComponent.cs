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

namespace MahAppBase.ViewModel
{
    public class MainComponent : ViewModelBase
    {
        #region Declarations
        private NotifyIcon nIcon = new NotifyIcon();
        ucDonate donate = new ucDonate();
        private WindowState _State;
        private Visibility _MainWindowVisibility;
        private bool _ShowInToolBar;
        private string _ChatText;
        private string _InPut;

        #endregion

        #region Property
        private ObservableCollection<string> _AllUser = new ObservableCollection<string>();
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

        private int _ShowMessageTime = 60;
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
        private string _UserName;
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
        public NoParameterCommand ButtonDonateClick { get; set; }

        private System.Windows.Media.Brush _StatusBackGroundColor = new SolidColorBrush(Colors.Red);
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

        public NoParameterCommand SendMessageCommand 
        {
            get;set;
        }

        public NoParameterCommand CloseCommand 
        {
            get;set;
        }
        public void SendMessage() 
       {
			
            if (!string.IsNullOrEmpty(InPut)) 
            {
                //render message
                //ChatText += $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {UserName} : {InPut}";
                //send message
                ws.Send($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {UserName} : {InPut}\n");
                //clear input
                InPut = "";
                

            }
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
        public bool DonateIsOpen { get; set; }
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
                    Environment.Exit(0);
                    break;
            }
        }
		private void NIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
		{
            State = WindowState.Normal;
        }
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
            }
            catch(Exception ex) 
            {
                System.Windows.Forms.MessageBox.Show($"初始化發生例外 : {ex.Message}\r\n{ex.StackTrace}");
            }

        }

		private void CloseCommandAction()
		{
            State = WindowState.Minimized;
        }

		private void SendMessageCommandAction()
		{
            SendMessage();
		}

		public WebSocket ws { get; set; }
        public string _ConnectStatus = string.Empty;

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

		private void InitConnection()
		{
			try 
            {
                var server = ConfigurationSettings.AppSettings["Server"];
                ws = new WebSocket($"ws://{server}:5566/Connect");
                ws.OnMessage += Ws_OnMessage;
                ws.OnOpen += Ws_OnOpen;
                ws.OnClose += Ws_OnClose;
                ws.Connect();
                ConnectStatus = "伺服器連線成功!";
                StatusBackGroundColor = Brushes.LightGreen;
            }
            catch(Exception ex) 
            {
				System.Windows.Forms.MessageBox.Show($"初始化連線發生例外 : {ex.Message}\r\n{ex.StackTrace}");
                StatusBackGroundColor = Brushes.Red;
            }
            
        }

		private void Ws_OnClose(object sender, CloseEventArgs e)
		{
            ConnectStatus = "伺服器中斷連線...嘗試重連....";
            StatusBackGroundColor = Brushes.Orange;
            InitialClient();
        }

		private void InitialClient()
		{
            var server = ConfigurationSettings.AppSettings["Server"];
            ws = new WebSocket($"ws://{server}:5566/Connect");
            ws.OnMessage += Ws_OnMessage;
            ws.OnOpen += Ws_OnOpen;
            ws.OnClose += Ws_OnClose;
            ws.Connect();
            ConnectStatus = "伺服器連線成功!";
        }

		private void Ws_OnOpen(object sender, EventArgs e)
		{
            ConnectStatus = "伺服器連線成功!";
        }

		private void Ws_OnMessage(object sender, MessageEventArgs e)
		{
            var ws = (sender as WebSocket);

            if (ws is null)
            {
                return;
            }

            string receiveData = e.Data;
			if (receiveData.Length == 41) 
            {
                var allLloginMessage = receiveData.Split(' ');
				if (allLloginMessage.Length > 2) 
                {
                    AllUser.Add(allLloginMessage[1]);
                }
            }
            switch (receiveData)
            {
                case "CLOSE":
                    Console.WriteLine("hit");
                    break;
                default:
                    ChatText += receiveData;
                    if(!receiveData.Contains(UserName))
                        ShowMessage("通知", receiveData, NotificationType.Success);
                    break;
            }
        }


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
        private void Donate_Closed(object sender, EventArgs e)
        {
            DonateIsOpen = false;
        }

        #endregion
    }
}