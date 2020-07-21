using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using JitExplorer.Model;

namespace JitExplorer.Commands
{
    public class NewCommand : ICommand
    {
#pragma warning disable 67
        public event EventHandler CanExecuteChanged;
#pragma warning restore 67

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var model = parameter as AppModel;

            model.SetDefaultSource();
        }
    }
}
