using JitExplorer.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JitExplorer.Commands
{
    public class SaveSourceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged = delegate { };

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var model = parameter as AppModel;

            var d = new SaveFileDialog();
            d.Filter = @"Visual C# files (*.cs)|*.cs|All files (*.*)|*.*";

            if (d.ShowDialog().Value)
            {
                Task.Run(() =>
                {
                    File.WriteAllText(d.FileName, model.SourceCode);
                });
            }
        }
    }
}
