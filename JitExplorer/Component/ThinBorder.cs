using System;
using System.Collections.Generic;

using System.Text;
using System.Windows;
using System.Windows.Media;

namespace JitExplorer.Component
{
    public static class ThinBorder
    {
        public static void SetThinBorder(object sender, EventArgs e)
        {
            var s = (Window)sender;
            Matrix m = PresentationSource.FromVisual(s).CompositionTarget.TransformToDevice;
            double dpiFactor = 1 / m.M11;
            s.BorderThickness = new Thickness(dpiFactor);
        }
    }
}
