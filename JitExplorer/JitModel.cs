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
                this.OnPropertyChanged();
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
                this.OnPropertyChanged();
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
                this.OnPropertyChanged();
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
                this.OnPropertyChanged();
            }
        }

        private void LegacyChanged()
        {
            if (this.legacyJit)
            {
                this.tieredCompilation = false;
                this.quick = false;
                this.quickLoop = false;

                // fire the events afterwards, else we trigger ModernChanged while with this.quick == true or this.quickLoop == true
                // before they are reset
                this.OnPropertyChanged(nameof(TieredCompilation));
                this.OnPropertyChanged(nameof(Quick));
                this.OnPropertyChanged(nameof(QuickLoop));
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

            if (this.tieredCompilation)
            {
                jitMode = JitMode.Tiered;
            }

            if (this.quick)
            {
                jitMode = jitMode | JitMode.Quick;
            }

            if (this.quickLoop)
            {
                jitMode = jitMode | JitMode.QuickLoop;
            }

            if (this.legacyJit)
            {
                jitMode = jitMode | JitMode.Legacy;
            }

            return jitMode;
        }
    }
}
