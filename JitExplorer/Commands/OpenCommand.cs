using JitExplorer.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Input;

namespace JitExplorer.Commands
{
    public class OpenCommand : ICommand
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

            var d = new OpenFileDialog();
            d.Filter = @"Visual C# files (*.cs)|*.cs|All files (*.*)|*.*";

            if (d.ShowDialog().Value)
            {
                var text = File.ReadAllText(d.FileName);
                model.SourceCode = text;
                model.Disassembly = null;
            }
        }
    }
}
