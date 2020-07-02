using BitFaster.Caching.Lru;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using JitExplorer.Completion;
using JitExplorer.Engine;
using JitExplorer.Engine.Compile;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
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
using System.Xml;

namespace JitExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IsolatedJit isolatedJit;
        private readonly RoslynCodeCompletion codeCompletion;
        private readonly ClassicLru<JitKey, string> cache = new ClassicLru<JitKey, string>(100);

        public MainWindow()
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

            // in the constructor:
            this.CodeEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            this.CodeEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;

            this.codeCompletion = new RoslynCodeCompletion(Compiler.MetadataReferences);
            this.codeCompletion.Initialize();
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
                    return Microsoft.CodeAnalysis.Platform.X86;
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
                var jitKey = new JitKey(source, config.OptimizationLevel, config.Platform, config.UseTieredCompilation);

                var result = this.cache.GetOrAdd(jitKey, k => this.isolatedJit.CompileJitAndDisassemble(source, config));

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

        CompletionWindow completionWindow;

        void textEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                var result = codeCompletion.CompleteAsync(this.CodeEditor.Text, this.CodeEditor.CaretOffset, '.').Result;

                // Open code completion after the user has pressed dot:
                completionWindow = new CompletionWindow(this.CodeEditor.TextArea);
                completionWindow.CompletionList.CompletionData.AddRange(result);

                completionWindow.Show();
                completionWindow.Closed += delegate {
                    completionWindow = null;
                };
            }
        }

        void textEditor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }
    }
}
