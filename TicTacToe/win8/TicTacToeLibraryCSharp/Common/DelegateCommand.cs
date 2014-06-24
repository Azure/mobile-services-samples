using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToeLibraryCSharp.Common
{
    public delegate void ExecuteDelegate(object parameter);
    public delegate bool CanExecuteDelegate(object parameter);

    // The DelegateCommand class implements commands. It is used by XAML button controls.
    [Windows.Foundation.Metadata.WebHostHidden]
    public sealed class DelegateCommand : System.Windows.Input.ICommand
    {
        public DelegateCommand(ExecuteDelegate execute, CanExecuteDelegate canExecute)
        { 
            executeDelegate = execute;
            canExecuteDelegate = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            //assert(m_executeDelegate != null);
            if (null != executeDelegate)
            {
                executeDelegate(parameter);
            }
        }

        public bool CanExecute(object parameter)
        {
            if (canExecuteDelegate == null)
            {
                return true;
            }

            bool temp = canExecuteDelegate(parameter);
            if (canExecute != temp)
            {
                canExecute = temp;
                CanExecuteChanged(this, null);
            }
            return canExecute;
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }


        private ExecuteDelegate executeDelegate;
        private CanExecuteDelegate canExecuteDelegate;
        private bool canExecute;
    };
}
