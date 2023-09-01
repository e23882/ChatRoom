
using System.Windows;

namespace ChatUI
{
	public class UserInfo:ViewModelBase
	{
		#region Declarations
		public string _UserIP = "";
		public Visibility _IsLive = Visibility.Collapsed;
		#endregion

		#region Property
		#endregion

		#region Memberfunction
		#endregion
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
	}
}
