using System;
using System.Diagnostics;
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
            HoodieShared.Core.HoodieSuiteUpdater(args[0], args[1]);
        }
    }
}
