using JitExplorer.Engine;
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
        public MainWindow()
        {
            InitializeComponent();

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

        private void Jit_Click(object sender, RoutedEventArgs e)
        {
            this.Jit.IsEnabled = false;
            this.StatusText.Text = "Compiling...";
            string source = this.CodeEditor.Text;
            Task.Run(() => this.JitIt(source));
        }

        private void JitIt(string source)
        {
            var exp = new IsolatedJit();

            try
            {
                var result = exp.CompileJitAndDisassemble(source);
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
