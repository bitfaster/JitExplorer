using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JitExplorer
{
    /// <summary>
    /// Interaction logic for Splash.xaml
    /// </summary>
    public partial class Splash : MetroWindow
    {
        public Splash()
        {
            InitializeComponent();
            this.Loaded += a_Loaded;
        }

        void a_Loaded(object sender, EventArgs e)
        {
            var s = (Window)sender;

            Matrix m = PresentationSource.FromVisual(s).CompositionTarget.TransformToDevice;
            double dpiFactor = 1 / m.M11;

            this.BorderThickness = new Thickness(dpiFactor);
        }
    }
}
