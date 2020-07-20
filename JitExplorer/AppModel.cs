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
        }

        private CompilerModel compilerModel;

        public CompilerModel CompilerModel
        {
            get { return this.compilerModel; }
            set
            {
                if (value == this.compilerModel)
                {
                    return;
                }

                compilerModel = value;
                OnPropertyChanged();
            }
        }
    }
}
