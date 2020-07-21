using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace JitExplorer.Commands
{
    public static class GlobalCommands
    {
        public static ICommand AboutCommand = new AboutCommand();
        public static ICommand ExitCommand = new ExitCommand();
    }
}
