using JitExplorer.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace JitExplorer.Commands
{
    public class SaveAsmCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            var model = parameter as AppModel;

            return !string.IsNullOrEmpty(model.Disassembly?.AsmText ?? string.Empty);
        }

        public void Execute(object parameter)
        {
            var model = parameter as AppModel;

            var d = new SaveFileDialog();
            d.Filter = @"Asm files (*.asm)|*.asm|All files (*.*)|*.*";

            if (d.ShowDialog().Value)
            {
                Task.Run(() =>
                {
                    File.WriteAllText(d.FileName, model.Disassembly.AsmText);
                });
            }
        }

        public void RaiseCanExecuteChanged()
        {
            Application.Current.Dispatcher.Invoke((() => { CanExecuteChanged?.Invoke(this, EventArgs.Empty); }));
        }
    }
}
