using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;

namespace JitExplorer.Controls
{
    // https://stackoverflow.com/questions/29717581/how-can-i-disable-customized-tabitem-close-button-for-some-specific-tabs/29717880#29717880
    public class PermanentTabItem : TabItem, INotifyPropertyChanged
    {
        public PermanentTabItem()
        {
            this.IsButtonEnabled = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private bool _isButtonEnabled;

        public bool IsButtonEnabled
        {
            get { return _isButtonEnabled; }

            set
            {
                if (value != _isButtonEnabled)
                {
                    _isButtonEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
