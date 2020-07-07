using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace JitExplorer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var splash = new Splash();
            splash.Show();
            InitializeHighlighting();
            var mainWindow = new MainWindow();
            mainWindow.Show();
            splash.Close();
        }

        private static void InitializeHighlighting()
        {
            HighlightingManager.Instance.RegisterHighlighting(
                "Asm", new string[] { ".s", ".asm" },
                delegate {
                    using (Stream s = typeof(MainWindow).Assembly.GetManifestResourceStream("JitExplorer.Controls.Asm-Mode.xshd"))
                    {
                        using (XmlTextReader reader = new XmlTextReader(s))
                        {
                            return HighlightingLoader.Load(reader, HighlightingManager.Instance);
                        }
                    }
                });

            HighlightingManager.Instance.RegisterHighlighting(
                "C#", new string[] { ".cs" },
                delegate
                {
                    using (Stream s = typeof(MainWindow).Assembly.GetManifestResourceStream("JitExplorer.Controls.CSharp-Mode.xshd"))
                    {
                        using (XmlTextReader reader = new XmlTextReader(s))
                        {
                            return HighlightingLoader.Load(reader, HighlightingManager.Instance);
                        }
                    }
                });
        }
    }
}
