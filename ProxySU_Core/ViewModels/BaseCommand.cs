using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ProxySU_Core.ViewModels
{
    public class BaseCommand : ICommand
    {
        private readonly Action<object> _execution;
        private readonly Func<object, bool> _canExecute;

        public BaseCommand(Action<object> execution, Func<object, bool> canExecute = null)
        {
            _execution = execution;
            _canExecute = canExecute;
        }



        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }

            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            if (_execution != null && CanExecute(parameter))
            {
                _execution.Invoke(parameter);
            }
        }
    }
}
