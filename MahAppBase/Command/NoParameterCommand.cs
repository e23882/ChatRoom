using System;
using System.Windows.Input;

namespace ChatUI.Command
{
    /// <summary>
    /// 無參數共用Command
    /// </summary>
    public class NoParameterCommand : ICommand
    {
        #region Declarations
        public readonly Action _execute = null;
        public event EventHandler CanExecuteChanged;
        #endregion

        #region Memberfunction
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _execute.Invoke();
        }

        public NoParameterCommand(Action execute)
        {
            _execute = execute;
        }
        #endregion
    }
}