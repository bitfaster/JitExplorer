﻿using JitExplorer.Engine;
using System;
using System.Collections.Generic;
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
        }

        private void Jit_Click(object sender, RoutedEventArgs e)
        {
            var exp = new IsolatedJit();

            try
            {
                this.AssemblerView.Text = exp.CompileJitAndDisassemble(this.CodeEditor.Text);
            }
            catch (Exception ex)
            {
                this.AssemblerView.Text = ex.ToString();
            }
            
        }
    }
}
