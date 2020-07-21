using JitExplorer.Commands;
using JitExplorer.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace JitExplorer.Model
{
    // TODO:
    // - Select output tab on error, asm for non error
    //  - NavigateToOutputCommand
    //  - NavigateToAsmCommand
    // - Asm
    //  - Double click support depends on disassembly objects in mainWindow.cs. These should be changed to commands.
    // - Migrate from legacy nuget to Microsoft.Xaml.Behaviors.Wpf, see this:
    // https://stackoverflow.com/questions/8360209/how-to-add-system-windows-interactivity-to-project
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
            this.StatusModel = new StatusModel();
            this.sourceCode = GetDefaultSource();
            this.jitCommand = new JitCommand();
        }

        private string sourceCode;
        private Disassembly disassembly;
        private CompilerModel compilerModel;
        private JitModel jitModel;
        private StatusModel statusModel;

        private JitCommand jitCommand;

        public JitCommand JitCommand => this.jitCommand;

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

        public StatusModel StatusModel
        {
            get { return this.statusModel; }
            set
            {
                if (value == this.statusModel)
                {
                    return;
                }

                this.statusModel = value;
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
