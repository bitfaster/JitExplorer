using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace JitExplorer.Model
{
    public class StatusModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public StatusModel()
        {
            this.SetReady();
        }

        private string status;
        private FontAwesome.WPF.FontAwesomeIcon icon;
        private bool spinIcon;

        public void SetReady()
        {
            this.Status = "Ready";
            this.Icon = FontAwesome.WPF.FontAwesomeIcon.Stop;
            this.SpinIcon = false;
        }

        public void SetRunning()
        {
            this.Status = "Running...";
            this.Icon = FontAwesome.WPF.FontAwesomeIcon.Cog;
            this.SpinIcon = true;
        }

        public string Status
        {
            get { return this.status; }
            set
            {
                if (value == this.status)
                {
                    return;
                }

                this.status = value;

                this.OnPropertyChanged();
            }
        }

        public FontAwesome.WPF.FontAwesomeIcon Icon
        {
            get { return this.icon; }
            set
            {
                if (value == this.icon)
                {
                    return;
                }

                this.icon = value;
                this.OnPropertyChanged();
            }
        }

        public bool SpinIcon
        {
            get { return this.spinIcon; }
            set
            {
                if (value == this.spinIcon)
                {
                    return;
                }

                this.spinIcon = value;
                this.OnPropertyChanged();
            }
        }
    }
}
