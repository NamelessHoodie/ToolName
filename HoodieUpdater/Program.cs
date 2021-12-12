using System;
using HoodieShared;
namespace HoodieUpdater
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            HoodieShared.Core.HoodieSuiteUpdater(args[0]);
        }
    }
}
