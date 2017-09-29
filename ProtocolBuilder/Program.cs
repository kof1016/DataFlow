using System;
using System.IO;
using System.Reflection;

using Gateway.Utility;

using Protocol;

namespace ProtocolBuilder
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, a) =>
            {
                // Regulus.Utility.CrashDump.Write();
                Environment.Exit(0);
            };

            // Regulus.Utility.Log.Instance.RecordEvent += _WriteLog;
            if(args.Length == 0)
            {
                Console.WriteLine("Need to build parameters.");
                Console.WriteLine("ex . ProtocolBuilder.exe build.ini");
                return;
            }

            var path = args[0];
            if(File.Exists(path) == false)
            {
                Console.WriteLine($"Non-existent path {path}.");
                return;
            }

            try
            {
                var ini = new Ini(File.ReadAllText(path));

                if(Program._TryRead(ini, "Build", "SourcePath", out string sourcePath) == false)
                {
                    return;
                }

                if(Program._TryRead(ini, "Build", "ProtocolName", out string protocolName) == false)
                {
                    return;
                }

                if(Program._TryRead(ini, "Build", "OutputPath", out string outputPath) == false)
                {
                    return;
                }

                var sourceFullPath = Path.GetFullPath(sourcePath);
                var outputFullPath = Path.GetFullPath(outputPath);

                Console.WriteLine($"Source {sourceFullPath}");
                Console.WriteLine($"Output {outputFullPath}");

                var sourceAsm = Assembly.LoadFile(sourceFullPath);

                var assemblyBuilder = new AssemblyBuilder();

                assemblyBuilder.BuildFile(sourceAsm, protocolName, outputFullPath);

                Console.WriteLine("Build success.");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("Build failure.");
            }
        }

        private static bool _TryRead(Ini ini, string section, string key, out string value)
        {
            if(ini.TryRead(section, key, out value) == false)
            {
                Program._ShowBuildIni();
                return false;
            }

            return true;
        }

        private static void _ShowBuildIni()
        {
            Console.WriteLine("Wrong Ini format.");
            var iniSample = @"
            [Build]
            SourcePath = YourProjectPath/YourAssemblyCommon.dll
            ProtocolName = YourProjectNamespace.ProtocolClassName
            OutputPath = YourProjectPath/YourAssemblyOutput.dll
            ";

            Console.WriteLine("ex.");
            Console.WriteLine(iniSample);

            Console.WriteLine("Do you create sample.ini file? (Y/N)");
            var ans = Console.ReadLine();
            if(ans == "Y" || ans == "y")
            {
                File.WriteAllText("sample.ini", iniSample);
            }
        }

        private static void _WriteLog(string message)
        {
            Console.WriteLine(message);
        }
    }
}
