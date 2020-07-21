using BitFaster.Caching.Lru;
using JitExplorer.Engine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace JitExplorer
{
    public class JitCommand : ICommand
    {
        //private readonly RuntimeDisassembler dissassembler;
        private readonly ClassicLru<JitKey, Disassembly> cache = new ClassicLru<JitKey, Disassembly>(100);

        private bool canExecute = true;

        public event EventHandler CanExecuteChanged;

        public JitCommand()
        {
            
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute;
        }

        public void Execute(object parameter)
        {
            this.canExecute = false;
            RaiseCanExecuteChanged();

            var model = parameter as AppModel;

            Task.Run(() => 
            {
                model.StatusModel.SetRunning();

                var dissassembler = new RuntimeDisassembler("test.exe");
                dissassembler.Progress += (object sender, ProgressEventArgs e) => { model.StatusModel.Status = e.StatusMessage; };

                var jitKey = new JitKey(model.SourceCode, model.GetConfig());

                var disassembly = this.cache.GetOrAdd(jitKey, k => dissassembler.CompileJitAndDisassemble(k.SourceCode, k.Config));

                model.Disassembly = disassembly;

                this.canExecute = true;
                Application.Current.Dispatcher.Invoke((() => { RaiseCanExecuteChanged(); }));
                model.StatusModel.SetReady();
            });
        }



        private void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
