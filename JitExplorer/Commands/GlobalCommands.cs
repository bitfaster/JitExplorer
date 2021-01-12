using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace JitExplorer.Commands
{
    public static class GlobalCommands
    {
        public static readonly ICommand AboutCommand = new AboutCommand();
        public static readonly ICommand ExitCommand = new ExitCommand();
        public static readonly ICommand NewCommand = new NewCommand();
        public static readonly ICommand OpenCommand = new OpenCommand();
        public static readonly SaveAsmCommand SaveAsmCommand = new SaveAsmCommand();
        public static readonly ICommand SaveSourceCommand = new SaveSourceCommand();
    }
}
