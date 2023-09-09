using System.Windows;

namespace ChatUI
{
	public class UserInfo : ViewModelBase
	{
		#region Declarations
		public string _UserIP = "";
		public Visibility _IsLive = Visibility.Collapsed;
		#endregion

		#region Property
		/// <summary>
		/// 使用者IP
		/// </summary>
		public string UserIP
		{
			get
			{
				return _UserIP;
			}
			set
			{
				_UserIP = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// 是否在分享畫面
		/// </summary>
		public Visibility IsLive
		{
			get
			{
				return _IsLive;
			}
			set
			{
				_IsLive = value;
				OnPropertyChanged();
			}
		}
		#endregion
	}
}
