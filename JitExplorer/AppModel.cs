using JitExplorer.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace JitExplorer
{
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
        }

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

        public Config GetConfig()
        {
            var config = new Config()
            {
                CompilerOptions = this.CompilerModel.GetCompilerConfig(),
                JitMode = this.JitModel.GetJitMode(),
            };

            return config;
        }
    }
}
