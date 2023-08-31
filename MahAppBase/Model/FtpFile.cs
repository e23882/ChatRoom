using ChatUI.ViewModel;

namespace ChatUI
{
	public class FtpFile:ViewModelBase
	{
		public string _FileName = string.Empty;
		public string FileName 
		{
			get 
			{
				return _FileName;
			}
			set 
			{
				_FileName = value;
				OnPropertyChanged();
			}
		}
	}
}
