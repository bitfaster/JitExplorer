using JitExplorer.Engine.Compile;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace JitExplorer
{
    public class CompilerModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int languageSelection;
        private int buildConfig;
        private int plaform;
        private bool allowUnsafe;

        public int LanguageSelection
        {
            get { return this.languageSelection; }
            set
            {
                if (value == this.languageSelection) 
                { 
                    return; 
                }

                this.languageSelection = value;
                this.OnPropertyChanged();
            }
        }

        public int BuildConfig
        {
            get { return this.buildConfig; }
            set
            {
                if (value == this.buildConfig)
                {
                    return;
                }

                this.buildConfig = value;
                this.OnPropertyChanged();
            }
        }

        public int Platform
        {
            get { return this.plaform; }
            set
            {
                if (value == this.plaform)
                {
                    return;
                }

                this.plaform = value;
                this.OnPropertyChanged();
            }
        }

        public bool AllowUnsafe
        {
            get { return this.allowUnsafe; }
            set
            {
                if (value == this.allowUnsafe)
                {
                    return;
                }

                this.allowUnsafe = value;
                this.OnPropertyChanged();
            }
        }

        public CompilerOptions GetCompilerConfig()
        {
            var compilerOptions = new CompilerOptions()
            {
                OutputKind = OutputKind.ConsoleApplication,
                LanguageVersion = GetLanguageVersion(),
                Platform = GetPlatform(),
                OptimizationLevel = GetOptimizationLevel(),
                AllowUnsafe = this.allowUnsafe,
            };

            return compilerOptions;
        }

        private  LanguageVersion GetLanguageVersion()
        {
            switch (this.languageSelection)
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
            switch (this.plaform)
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
            return this.buildConfig == 0 ? OptimizationLevel.Release : OptimizationLevel.Debug;
        }
    }
}
