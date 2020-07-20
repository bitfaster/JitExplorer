using JitExplorer.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace JitExplorer
{
    // TODO:
    // - Status Bar
    // - Asm
    // - Output
    // - Jit Command
    public class AppModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public AppModel()
        {
            this.CompilerModel = new CompilerModel();
            this.JitModel = new JitModel();
            this.sourceCode = GetDefaultSource();
        }

        private string sourceCode;
        private Disassembly disassembly;
        private CompilerModel compilerModel;
        private JitModel jitModel;

        public CompilerModel CompilerModel
        {
            get { return this.compilerModel; }
            set
            {
                if (value == this.compilerModel)
                {
                    return;
                }

                this.compilerModel = value;
                this.OnPropertyChanged();
            }
        }

        public JitModel JitModel
        {
            get { return this.jitModel; }
            set
            {
                if (value == this.jitModel)
                {
                    return;
                }

                this.jitModel = value;
                this.OnPropertyChanged();
            }
        }

        // https://stackoverflow.com/questions/18964176/two-way-binding-to-avalonedit-document-text-using-mvvm
        public string SourceCode
        {
            get { return this.sourceCode; }
            set
            {
                if (value == this.sourceCode)
                {
                    return;
                }

                this.sourceCode = value;
                this.OnPropertyChanged();
            }
        }

        public Disassembly Disassembly
        {
            get { return this.disassembly; }
            set
            {
                if (value == this.disassembly)
                {
                    return;
                }

                this.disassembly = value;
                this.OnPropertyChanged();
            }
        }

        public Config GetConfig()
        {
            var config = new Config()
            {
                CompilerOptions = this.CompilerModel.GetCompilerConfig(),
                JitMode = this.JitModel.GetJitMode(),
            };

            return config;
        }

        private static string GetDefaultSource()
        {
            return @"namespace JitExplorer
{
    using System;

    public class Test
    {
        [" + RuntimeDisassembler.AttributeName + @"]
        public static void Execute()
        {
            for (int i = 0; i < 100; i++)
            {
                i = i * 3;
            }
        }
    }
}";
        }
    }
}
