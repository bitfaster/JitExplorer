using BitFaster.Caching.Lru;
using JitExplorer.Engine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace JitExplorer
{
    // http://www.blackwasp.co.uk/WPFCustomCommands_2.aspx
    public class JitCommand : ICommand
    {
        private readonly RuntimeDisassembler dissassembler;
        private readonly ClassicLru<JitKey, Disassembly> cache = new ClassicLru<JitKey, Disassembly>(100);

        private bool canExecute = true;

        public event EventHandler CanExecuteChanged;

        public JitCommand()
        {
            this.dissassembler = new RuntimeDisassembler("test.exe");
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            this.canExecute = false;

            var model = parameter as AppModel;

            var jitKey = new JitKey(model.SourceCode, model.GetConfig());

            model.Disassembly = this.cache.GetOrAdd(jitKey, k => this.dissassembler.CompileJitAndDisassemble(k.SourceCode, k.Config));
        }
    }
}
