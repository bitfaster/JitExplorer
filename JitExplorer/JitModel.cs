using JitExplorer.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace JitExplorer
{
    public class JitModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool legacyJit;
        private bool tieredCompilation;
        private bool quick;
        private bool quickLoop;

        public bool LegacyJit
        {
            get { return this.legacyJit; }
            set
            {
                if (value == this.legacyJit)
                {
                    return;
                }

                this.legacyJit = value;
                this.LegacyChanged();
                OnPropertyChanged();
            }
        }

        public bool TieredCompilation
        {
            get { return this.tieredCompilation; }
            set
            {
                if (value == this.tieredCompilation)
                {
                    return;
                }

                this.tieredCompilation = value;
                this.ModernChanged();
                OnPropertyChanged();
            }
        }

        public bool Quick
        {
            get { return this.quick; }
            set
            {
                if (value == this.quick)
                {
                    return;
                }

                this.quick = value;
                this.ModernChanged();
                OnPropertyChanged();
            }
        }

        public bool QuickLoop
        {
            get { return this.quickLoop; }
            set
            {
                if (value == this.quickLoop)
                {
                    return;
                }

                this.quickLoop = value;
                this.ModernChanged();
                OnPropertyChanged();
            }
        }

        private void LegacyChanged()
        {
            if (this.legacyJit)
            {
                this.TieredCompilation = false;
                this.Quick = false;
                this.QuickLoop = false;
            }
        }

        private void ModernChanged()
        {
            if (this.tieredCompilation || this.quick || this.quickLoop)
            {
                this.LegacyJit = false;
            }
        }

        public JitMode GetJitMode()
        {
            JitMode jitMode = JitMode.Default;

            if (tieredCompilation)
            {
                jitMode = JitMode.Tiered;
            }

            if (quick)
            {
                jitMode = jitMode | JitMode.Quick;
            }

            if (quickLoop)
            {
                jitMode = jitMode | JitMode.QuickLoop;
            }

            if (legacyJit)
            {
                jitMode = jitMode | JitMode.Legacy;
            }

            return jitMode;
        }
    }
}
