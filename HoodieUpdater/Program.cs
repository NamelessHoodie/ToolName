using System;
using System.Diagnostics;
using System.Windows;
using HoodieShared;
namespace HoodieUpdater
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
#if DEBUG
                Debugger.Launch();
#endif
            HoodieShared.Core.ToolNameUpdater(args[0], args[1]);
        }
    }
}
