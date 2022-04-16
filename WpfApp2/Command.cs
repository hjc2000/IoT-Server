using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfApp2
{
    public class Command : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public event Action<object> ExecuteChanged;
        public Command()
        {

        }
        public Command(Action<object> action)
        {
            ExecuteChanged = action;
        }

        public bool CanExecute(object parameter)
        {
            //用来控制命令是否允许执行a
            return true;
        }

        public void Execute(object parameter)
        {
            if (ExecuteChanged != null)
            {
                ExecuteChanged(parameter);
            }
        }
    }
}
