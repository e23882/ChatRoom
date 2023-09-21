using System;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.InteropServices;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Notifications.Wpf;

namespace ChatUI
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        #region Declarations

        private MainComponent _viewModel = null;
        private const uint ATTACH_PARENT_PROCESS = 0x0ffffffff;

        private bool _closeMe;
        private bool _isFirstTime = true;
        private bool _isChoosingWord = false;

        private int _wordLength = 0;

        #endregion

        #region Memberfunction

        /// <summary>
        /// 主視窗建構子
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            ShowMessage("通知", "開始初始化相關設定", NotificationType.Information);
            _viewModel = new MainComponent
            {
                Window = window
            };
            this.DataContext = _viewModel;

            var win = new Barrage1();
            win.Show();
            _viewModel.Barrage = win;
        }

        private void ShowMessage(string title, string message, NotificationType type)
        {
            try
            {
                var notificationManager = new NotificationManager();
                var ts = TimeSpan.FromSeconds(10 * 1000);
                notificationManager.Show(new NotificationContent
                {
                    Title = title,
                    Message = message,
                    Type = type,
                });
            }
            catch (Exception ex)
            {
                ShowMessage("顯示訊息時發生例外", $"{ex.Message}\r\n{ex.StackTrace}", NotificationType.Error);
            }
        }

        /// <summary>
        /// 輸入訊息元件按下上事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                _viewModel.PressUpButtonCommandAction();
                return;
            }

            if (e.Key != Key.Enter)
                return;

            //按下Enter且在選字模式 > 不送訊息出去
            if ((sender as System.Windows.Controls.TextBox) is null) return;
            if (_wordLength == (sender as System.Windows.Controls.TextBox).Text.Length && _isChoosingWord)
            {
                return;
            }

            _viewModel.SendMessage();
        }

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(uint dwProcessId);

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if ((sender as System.Windows.Controls.TextBox) is null) return;
            AttachConsole(ATTACH_PARENT_PROCESS);
            Console.WriteLine($"textChange");

            Console.WriteLine($"Original : {_wordLength}");
            Console.WriteLine($"Current : {(sender as System.Windows.Controls.TextBox).Text.Length}");
            //選字
            _isChoosingWord = _wordLength == (sender as System.Windows.Controls.TextBox).Text.Length;
            _wordLength = (sender as System.Windows.Controls.TextBox).Text.Length;
        }

        private void tbChat_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            tbChat.ScrollToEnd();
        }

        private async void window_Closing(object sender, CancelEventArgs e)
        {
            if (e.Cancel)
                return;
            e.Cancel = !this._closeMe;
            if (this._closeMe)
                return;

            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "離開",
                NegativeButtonText = "縮小",
                AnimateShow = true,
                AnimateHide = false
            };
            var result = await this.ShowMessageAsync(
                "關閉應用程式 ?",
                "是否要關閉應用程式 ?",
                MessageDialogStyle.AffirmativeAndNegative, mySettings);

            this._closeMe = result == MessageDialogResult.Affirmative;

            if (this._closeMe)
            {
                Environment.Exit(0);
            }
            else
            {
                _viewModel.State = WindowState.Minimized;
            }
        }

        private void window_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_isFirstTime)
            {
                _isFirstTime = false;
            }

            foreach (var item in this._viewModel.AllUser)
            {
                Thread th = new Thread(() =>
                {
                    try
                    {
                        System.Net.Sockets.Socket sock = new System.Net.Sockets.Socket(
                            System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream,
                            System.Net.Sockets.ProtocolType.Tcp);
                        sock.Connect(item.UserIP, 6842);
                        if (sock.Connected == true)
                        {
                            if (this._viewModel.AllUser.FirstOrDefault(x => x.UserIP == item.UserIP) is null) return;
                            this._viewModel.AllUser.FirstOrDefault(x => x.UserIP == item.UserIP).IsLive =
                                Visibility.Visible;
                        }

                        sock.Close();
                    }
                    catch (System.Net.Sockets.SocketException ex)
                    {
                    }
                });
                th.Start();
            }
        }

        #endregion
    }
}