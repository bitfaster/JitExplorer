using JitExplorer.Engine;
using Microsoft.CodeAnalysis;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IsolatedJit isolatedJit;

        public MainWindow()
        {
            InitializeComponent();

            this.isolatedJit = new IsolatedJit();
            this.isolatedJit.Progress += IsolatedJit_Progress;

            this.CodeEditor.Text = @"namespace Testing
{
    using System;

    public class Program
    {
        public static void Main(string[] args)
        {
            JitExplorer.Signal.__Jit();
        }
    }
}";
        }

        private void IsolatedJit_Progress(object sender, ProgressEventArgs e)
        {
            this.Dispatcher.Invoke(() => this.StatusText.Text = e.StatusMessage);
        }

        private void Jit_Click(object sender, RoutedEventArgs e)
        {
            this.Jit.IsEnabled = false;
            string source = this.CodeEditor.Text;

            var config = new Config()
            {
                Platform = GetPlatform(),
                OptimizationLevel = GetOptimizationLevel(),
                UseTieredCompilation = this.TieredCompilation.IsChecked.Value,
            };

            Task.Run(() => this.JitIt(source, config));
        }

        private Platform GetPlatform()
        {
            switch (this.Platform.SelectedIndex)
            {
                case 0:
                    return Microsoft.CodeAnalysis.Platform.X64;
                case 1:
                    return Microsoft.CodeAnalysis.Platform.X64;
            }

            return Microsoft.CodeAnalysis.Platform.AnyCpu;
        }

        private OptimizationLevel GetOptimizationLevel()
        {
            return this.BuildConfig.SelectedIndex == 0 ? OptimizationLevel.Release : OptimizationLevel.Debug;
        }

        private void JitIt(string source, Config config)
        {
            try
            {
                var result = this.isolatedJit.CompileJitAndDisassemble(source, config);
                this.Dispatcher.Invoke(() => this.AssemblerView.Text = result);
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke(() => this.AssemblerView.Text = ex.ToString());
            }
            finally
            {
                this.Dispatcher.Invoke(() => { this.Jit.IsEnabled = true; this.StatusText.Text = "Ready"; });
            }
        }

        private async void OpenFile(object sender, RoutedEventArgs e)
        {
            var d = new OpenFileDialog();
            
            if (d.ShowDialog().Value)
            {
                var text = await File.ReadAllTextAsync(d.FileName);
                this.CodeEditor.Text = text;
            }
        }

        private async void SaveCode(object sender, RoutedEventArgs e)
        {
            var d = new SaveFileDialog();
            if (d.ShowDialog().Value)
            {
                await File.WriteAllTextAsync(d.FileName, this.CodeEditor.Text);
            }
        }

        private async void SaveDissassemble(object sender, RoutedEventArgs e)
        {
            var d = new SaveFileDialog();
            if (d.ShowDialog().Value)
            {
                await File.WriteAllTextAsync(d.FileName, this.AssemblerView.Text);
            }
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
