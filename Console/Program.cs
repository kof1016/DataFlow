using System;
using System.Runtime.Hosting;

using Regulus.Utility.WindowConsoleAppliction;

namespace Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var app = new Application();
            app.Run();

            // IUpdatable center = new Center();

            // center.Update();
        }
    }

    
}
