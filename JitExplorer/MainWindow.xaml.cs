using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using JitExplorer.Completion;
using JitExplorer.Component;
using JitExplorer.Engine.Compile;
using JitExplorer.Model;
using MahApps.Metro.Controls;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JitExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow 
    {
        private readonly RoslynCodeCompletion codeCompletion;

        public AppModel AppModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            this.AppModel = new AppModel();
            DataContext = this.AppModel;

            this.AssemblerView.MouseDoubleClick += AssemblerView_MouseDoubleClick;

            this.Loaded += ThinBorder.SetThinBorder;

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

                if (this.AppModel.Disassembly != null)
                {
                    if (this.AppModel.Disassembly.AsmToSourceLineIndex.TryGetValue(p.Line, out var targetLine))
                    {
                        if (targetLine < this.CodeEditor.Document.LineCount)
                        {
                            var ceLine = this.CodeEditor.TextArea.Document.GetLineByNumber(targetLine);
                            this.CodeEditor.ScrollTo(targetLine, 0);
                            this.CodeEditor.TextArea.Selection = Selection.Create(this.CodeEditor.TextArea, ceLine.Offset, ceLine.EndOffset);
                        }
                    }

                    if (this.AppModel.Disassembly.AsmLineToAsmLineIndex.TryGetValue(p.Line, out targetLine))
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
            }
        }

        private async void OpenFile(object sender, RoutedEventArgs e)
        {
            var d = new OpenFileDialog();
            
            if (d.ShowDialog().Value)
            {
                var text = await File.ReadAllTextAsync(d.FileName);
                this.AppModel.SourceCode = text;
                this.AppModel.Disassembly = null;
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
    }
}
