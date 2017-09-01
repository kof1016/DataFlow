using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;

using Library.Utility;

using Microsoft.CSharp;

namespace Library.TypeHelper
{
    public class AssemblyBuilder
    {
        // public void BuildFile(Assembly assembly, string library_name, string[] name_spaces, string output_path)
        // {
        // Dictionary<string, string> optionsDic = new Dictionary<string, string>
        // {
        // {"CompilerVersion", "v3.5"}
        // };
        // var provider = new CSharpCodeProvider(optionsDic);
        // var options = new CompilerParameters
        // {
        // OutputAssembly = output_path,
        // GenerateExecutable = false,
        // ReferencedAssemblies =
        // {
        // "System.Core.dll",
        // "System.xml.dll",
        // "RegulusLibrary.dll",
        // "RegulusRemoting.dll",
        // "Regulus.Serialization.dll",
        // assembly.Location
        // }
        // };
        // var types = assembly.GetExportedTypes();
        // var codes = new List<string>();
        // var codeBuilder = new CodeBuilder();
        // codeBuilder.GpiEvent += (name, code) => codes.Add(code);
        // codeBuilder.OnEventEvent += (type_name, event_name, code) => codes.Add(code);
        // codeBuilder.GpiEvent += (type_name, code) => codes.Add(code);
        // codeBuilder.Build(types);
        // var result = provider.CompileAssemblyFromSource(options, codes.ToArray());
        // if (result.Errors.Count > 0)
        // {
        // throw new Exception("Gpi compile error");
        // }
        // }
        public Type Build(Type base_type)
        {
            var optionsDic = new Dictionary<string, string>
                                 {
                                     {
                                         "CompilerVersion", "v3.5"
                                     }
                                 };
            var provider = new CSharpCodeProvider(optionsDic);
            var options = new CompilerParameters
                              {
                                  GenerateInMemory = true,
                                  GenerateExecutable = false,

                                  ReferencedAssemblies =
                                      {
                                          "System.Core.dll",
                                          "System.xml.dll",
                                          "Library.dll",
                                          base_type.Assembly.Location
                                      },
                                  TempFiles = new TempFileCollection()
                              };

            var codes = new List<string>();
            var codeBuilder = new CodeBuilder();
            codeBuilder.OnEventEvent += (type_name, event_name, code) => codes.Add(code);
            codeBuilder.GpiEvent += (type_name, code) => { codes.Add(code); };
            codeBuilder.Build(base_type);

            var result = provider.CompileAssemblyFromSource(options, codes.ToArray());
            if(result.Errors.Count > 0)
            {
                File.WriteAllLines("dump.cs", codes.ToArray());

                throw new Exception("Protocol compile error");
            }

            var ghostTypeName = "DataDefine.Ghost.C" + base_type.Name;
            return result.CompiledAssembly.GetType(ghostTypeName);
        }
    }
}
