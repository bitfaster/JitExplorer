using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;

namespace JitExplorer.Controls
{
    // https://github.com/MahApps/MahApps.Metro/blob/0dffed2b9a98ed928d3ac0b5c8d5c7708f1aa413/src/MahApps.Metro/Styles/VS/TabControl.xaml
    // https://stackoverflow.com/questions/48110420/why-not-show-close-button-of-metro-tab-item
    // https://stackoverflow.com/questions/29717581/how-can-i-disable-customized-tabitem-close-button-for-some-specific-tabs/29717880#29717880
    public class PermanentTabItem : MetroTabItem
    {
        public PermanentTabItem()
        {
            this.CloseButtonEnabled = false;
        }

        
    }
}
