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
            var exp = new IsolatedJit();

            try
            {
                this.lblCursorPosition.Text = "Compiling...";
                this.AssemblerView.Text = exp.CompileJitAndDisassemble(this.CodeEditor.Text);
            }
            catch (Exception ex)
            {
                this.AssemblerView.Text = ex.ToString();
            }
            finally
            {
                this.lblCursorPosition.Text = "Ready";
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
