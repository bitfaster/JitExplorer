using BitFaster.Caching.Lru;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using JitExplorer.Completion;
using JitExplorer.Controls;
using JitExplorer.Engine;
using JitExplorer.Engine.Compile;
using MahApps.Metro.Controls;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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
    public partial class MainWindow : MetroWindow 
    {
        private readonly RuntimeDisassembler dissassembler;
        private readonly RoslynCodeCompletion codeCompletion;
        private readonly ClassicLru<JitKey, Dissassembly> cache = new ClassicLru<JitKey, Dissassembly>(100);

        private Dissassembly dissassembly;

        public MainWindow()
        {
            InitializeComponent();

            this.dissassembler = new RuntimeDisassembler("test.exe");
            this.dissassembler.Progress += IsolatedJit_Progress;

            this.CodeEditor.Text = @"namespace JitExplorer
{
    using System;

    public class Test
    {
        [" + RuntimeDisassembler.AttributeName + @"]
        public static void Execute()
        {
            for (int i = 0; i < 100; i++)
            {
                i = i * 3;
            }
        }
    }
}";

            this.AssemblerView.MouseDoubleClick += AssemblerView_MouseDoubleClick;

            this.Loaded += a_Loaded;

            // in the constructor:
            this.CodeEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            this.CodeEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;

            this.codeCompletion = new RoslynCodeCompletion(Compiler.MetadataReferences);
            this.codeCompletion.Initialize();

            if (IntPtr.Size == 8)
            { 
                this.Title = "JitExplorer (x64)";
                var i = (ComboBoxItem)this.Platform.Items[1];
                i.IsEnabled = false;
            }
            else
            {
                this.Title = "JitExplorer (x86)";
                this.Platform.SelectedIndex = 1;
                var i = (ComboBoxItem)this.Platform.Items[0];
                i.IsEnabled = false;
            }

            this.AsmTab.IsButtonEnabled = false;
        }

        void a_Loaded(object sender, EventArgs e)
        {
            var s = (Window)sender;
            Matrix m = PresentationSource.FromVisual(s).CompositionTarget.TransformToDevice;
            double dpiFactor = 1 / m.M11;
            this.BorderThickness = new Thickness(dpiFactor);
        }
    
        // Scroll to line
        private void AssemblerView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // https://www.codeproject.com/Articles/42490/Using-AvalonEdit-WPF-Text-Editor?msg=4395053#xx4395053xx
            // https://github.com/icsharpcode/AvalonEdit/blob/master/ICSharpCode.AvalonEdit/Editing/TextArea.cs
            // https://github.com/aelij/RoslynPad/blob/f9cf2b3f14333d73210aa91329ec162324de2b70/src/RoslynPad.Editor.Shared/CodeTextEditor.cs - see OnMouseHover
            var t = e.Device.Target as ICSharpCode.AvalonEdit.Editing.TextArea;

            if (t != null)
            {
                var p = t.Caret.Position;
                var d = t.Document;

                var textLine = d.GetLineByNumber(p.Line);
                var str = d.Text.Substring(textLine.Offset, textLine.Length);

                if (this.dissassembly != null)
                {
                    if (this.dissassembly.AsmToSourceLineIndex.TryGetValue(p.Line, out var targetLine))
                    {
                        if (targetLine < this.CodeEditor.Document.LineCount)
                        {
                            var ceLine = this.CodeEditor.TextArea.Document.GetLineByNumber(targetLine);
                            this.CodeEditor.ScrollTo(targetLine, 0);
                            this.CodeEditor.TextArea.Selection = Selection.Create(this.CodeEditor.TextArea, ceLine.Offset, ceLine.EndOffset);
                        }
                    }

                    if (this.dissassembly.AsmLineToAsmLineIndex.TryGetValue(p.Line, out targetLine))
                    {
                        if (targetLine < this.AssemblerView.Document.LineCount)
                        {
                            var ceLine = this.AssemblerView.TextArea.Document.GetLineByNumber(targetLine);
                            this.AssemblerView.ScrollTo(targetLine, 0);
                            this.AssemblerView.TextArea.Selection = Selection.Create(this.AssemblerView.TextArea, ceLine.Offset, ceLine.EndOffset);
                        }
                    }

                    e.Handled = true;
                    return;
                }

                if (str.Contains('^'))
                {
                    var m = Regex.Match(str, @"((\d+))");

                    if (m.Success)
                    {
                        if (int.TryParse(m.Value, out int targetLine))
                        {
                            if (targetLine < this.CodeEditor.Document.LineCount)
                            {
                                var ceLine = this.CodeEditor.TextArea.Document.GetLineByNumber(targetLine);
                                this.CodeEditor.ScrollTo(targetLine, 0);
                                this.CodeEditor.TextArea.Selection = Selection.Create(this.CodeEditor.TextArea, ceLine.Offset, ceLine.EndOffset);
                            }
                        }
                    }
                }
            }
        }

        private void IsolatedJit_Progress(object sender, ProgressEventArgs e)
        {
            this.Dispatcher.Invoke(() => this.StatusText.Text = e.StatusMessage);
        }

        // Each time a key is pressed, it starts a timer.
        // After 100ms is elapsed, run Jit
        // If key is pressed again, dispose and restart timer.
        // If jit is already running, dispose and restart timer.
        // Track 'version' each time edit occurs. Compare jitted source to current source.

        private void Jit_Click(object sender, RoutedEventArgs e)
        {
            this.Jit.IsEnabled = false;
            this.ProgressIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.Cog;
            this.ProgressIcon.Spin = true;
            string source = this.CodeEditor.Text;

            var compilerOptions = new CompilerOptions()
            {
                OutputKind = OutputKind.ConsoleApplication,
                LanguageVersion = GetLanguageVersion(),
                Platform = GetPlatform(),
                OptimizationLevel = GetOptimizationLevel(),
                AllowUnsafe = GetAllowUnsafe(),
            };

            var config = new Config()
            {
                CompilerOptions = compilerOptions,
                JitMode = GetJitMode(),
            };

            Task.Run(() => this.JitIt(source, config));
        }

        private LanguageVersion GetLanguageVersion()
        {
            switch (this.LanguageVersion.SelectedIndex)
            {
                case 1:
                    return Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp1;
                case 2:
                    return Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp2;
                case 3:
                    return Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp3;
                case 4:
                    return Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp4;
                case 5:
                    return Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp5;
                case 6:
                    return Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp6;
                case 7:
                    return Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp7;
                case 8:
                    return Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp7_1;
                case 9:
                    return Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp7_2;
                case 10:
                    return Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp7_3;
                case 11:
                    return Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp8;
                case 12:
                    return Microsoft.CodeAnalysis.CSharp.LanguageVersion.Latest;
                default:
                    return Microsoft.CodeAnalysis.CSharp.LanguageVersion.Default;
            }
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

        private bool GetAllowUnsafe ()
        {
            return this.Unsafe.IsChecked.Value;
        }

        private JitMode GetJitMode()
        {
            JitMode jitMode = JitMode.Default;

            if (this.TieredCompilation.IsChecked.Value)
            {
                jitMode = JitMode.Tiered;
            }

            if (this.QuickJit.IsChecked.Value)
            {
                jitMode = jitMode | JitMode.Quick;
            }

            if (this.LoopJit.IsChecked.Value)
            {
                jitMode = jitMode | JitMode.QuickLoop;
            }

            if (this.Legacy.IsChecked.Value)
            {
                jitMode = jitMode | JitMode.Legacy;
            }

            return jitMode;
        }

        private void JitIt(string source, Config config)
        {
            try
            {
                var jitKey = new JitKey(source, config);

                this.dissassembly = this.cache.GetOrAdd(jitKey, k => this.dissassembler.CompileJitAndDisassemble(k.SourceCode, k.Config));

                // Free some memory?
                // System.Diagnostics.Process.GetCurrentProcess().MinWorkingSet = System.Diagnostics.Process.GetCurrentProcess().MinWorkingSet;

                this.Dispatcher.Invoke(
                    () => 
                    this.AssemblerView.Update(this.dissassembly.AsmText, new LineAddressResolver(this.dissassembly.AsmLineAddressIndex)));

                this.Dispatcher.Invoke(
                    () =>
                    this.OutputEditor.Text = this.dissassembly.OutputText);

                this.Dispatcher.Invoke(
                    () =>
                    OutputTab.SelectedIndex = this.dissassembly.IsSuccess ? 0 : 1);
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke(() => this.AssemblerView.Text = ex.ToString());
            }
            finally
            {
                this.Dispatcher.Invoke(() => 
                { 
                    this.Jit.IsEnabled = true; 
                    this.StatusText.Text = "Ready";
                    this.ProgressIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.Stop;
                    this.ProgressIcon.Spin = false;
                });
            }
        }

        private async void OpenFile(object sender, RoutedEventArgs e)
        {
            var d = new OpenFileDialog();
            
            if (d.ShowDialog().Value)
            {
                var text = await File.ReadAllTextAsync(d.FileName);
                this.CodeEditor.Text = text;
                this.AssemblerView.Text = string.Empty;
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

                // TODO: how to correctly style this?
                completionWindow.CompletionList.Background = SystemColors.ControlDarkDarkBrush;

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

        private void ShowLineNumbersHandler(object sender, RoutedEventArgs e)
        {
            this.CodeEditor.ShowLineNumbers = !this.CodeEditor.ShowLineNumbers;
        }

        private void ShowMemoryAddressesHandler(object sender, RoutedEventArgs e)
        {
            this.AssemblerView.ShowMemoryAddresses = !this.AssemblerView.ShowMemoryAddresses;
        }

        private void UndoHandler(object sender, RoutedEventArgs e)
        {
            this.CodeEditor.Undo();
        }

        private void RedoHandler(object sender, RoutedEventArgs e)
        {
            this.CodeEditor.Redo();
        }

        private void CutHandler(object sender, RoutedEventArgs e)
        {
            this.CodeEditor.Cut();
        }

        private void CopyHandler(object sender, RoutedEventArgs e)
        {
            this.CodeEditor.Copy();
        }

        private void PasteHandler(object sender, RoutedEventArgs e)
        {
            this.CodeEditor.Paste();
        }

        private void AboutHandler(object sender, RoutedEventArgs e)
        {
            string url = @"https://github.com/bitfaster/JitExplorer";

            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
        }

        private void AllowUnsafe_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Legacy_Click(object sender, RoutedEventArgs e)
        {
            if (this.Legacy.IsChecked.Value)
            {
                this.TieredCompilation.IsChecked = false;
                this.LoopJit.IsChecked = false;
                this.QuickJit.IsChecked = false;
            }    
        }

        private void ModernJit_Click(object sender, RoutedEventArgs e)
        {
            if (this.TieredCompilation.IsChecked.Value || this.LoopJit.IsChecked.Value || this.QuickJit.IsChecked.Value)
            {
                this.Legacy.IsChecked = false;
            }
        }
    }
}
