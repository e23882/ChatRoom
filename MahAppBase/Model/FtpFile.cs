namespace ChatUI
{
    public class FtpFile : ViewModelBase
    {
        #region Declarations

        private string _FileName = string.Empty;

        #endregion

        #region Property

        /// <summary>
        /// FTP 檔案名稱
        /// </summary>
        public string FileName
        {
            get { return _FileName; }
            set
            {
                _FileName = value;
                OnPropertyChanged();
            }
        }

        #endregion
    }
}