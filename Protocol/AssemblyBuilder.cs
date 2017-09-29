using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.CSharp;

namespace Protocol
{
    public class AssemblyBuilder
    {
        public void BuildFile(Assembly assembly, string protocol_name, string output_path)
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
                GenerateInMemory = false,
                GenerateExecutable = false,
                OutputAssembly = output_path,
                ReferencedAssemblies =
                {
                    "System.Core.dll",
                    "System.xml.dll",
                    "Gateway.dll",
                    "Synchronization.dll",
                    "Serialization.dll",
                    assembly.Location
                },
                TempFiles = new TempFileCollection()
            };

            var codes = AssemblyBuilder._BuildCode(assembly, protocol_name);
            File.WriteAllLines("dump.cs", codes.ToArray());
            var result = provider.CompileAssemblyFromSource(options, codes.ToArray());
            if(result.Errors.Count <= 0)
            {
                return;
            }

            File.WriteAllLines("error.log", _GetErrors(result.Errors).ToArray());
            

            throw new Exception("Protocol compile error");
        }

        // static build in memory
        public Assembly Build(Assembly assembly, string protocol_name)
        {
            var optionsDic = new Dictionary<string, string>
            {
                {
                    "CompilerVersion", "v4.6"
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
                    "Gateway.dll",
                    "Synchronization.dll",
                    "Serialization.dll",
                    assembly.Location
                },
                TempFiles = new TempFileCollection()
            };

            var codes = AssemblyBuilder._BuildCode(assembly, protocol_name);
            File.WriteAllLines("dump.cs", codes.ToArray());
            var result = provider.CompileAssemblyFromSource(options, codes.ToArray());
            if(result.Errors.Count <= 0)
            {
                return result.CompiledAssembly;
            }
            File.WriteAllLines("error.log", _GetErrors(result.Errors).ToArray());
            
            

            throw new Exception("static Protocol compile error in memory version");
        }

        private static List<string> _BuildCode(Assembly assembly, string protocol_name)
        {
            var codes = new List<string>();
            var codeBuilder = new CodeBuilder();
            codeBuilder.OnProviderEvent += (name, code) => codes.Add(code);
            codeBuilder.OnEventEvent += (type_name, event_name, code) => codes.Add(code);
            codeBuilder.OnGpiEvent += (type_name, code) => codes.Add(code);
            codeBuilder.Build(protocol_name, assembly.GetExportedTypes());
            return codes;
        }

        // dynamic build in memory
//        public Type Build(Type base_type, string library_path, string synchronization_path)
//        {
//            var optionsDic = new Dictionary<string, string>
//            {
//                {
//                    "CompilerVersion", "v4.5"
//                }
//            };
//            var provider = new CSharpCodeProvider(optionsDic);
//            var options = new CompilerParameters
//            {
//                GenerateInMemory = true,
//                GenerateExecutable = false,
//
//                ReferencedAssemblies =
//                {
//                    "System.Core.dll",
//                    "System.xml.dll",
//                    library_path,
//                    synchronization_path,
//                    base_type.Assembly.Location
//                },
//                TempFiles = new TempFileCollection()
//            };
//
//            var codes = new List<string>();
//            var codeBuilder = new Gateway.Utility.CodeBuilder();
//            codeBuilder.OnEventEvent += (type_name, event_name, code) => codes.Add(code);
//            codeBuilder.GpiEvent += (type_name, code) => { codes.Add(code); };
//            codeBuilder.Build(base_type);
//
//            var result = provider.CompileAssemblyFromSource(options, codes.ToArray());
//            if(result.Errors.Count > 0)
//            {
//                File.WriteAllLines("dump.cs", codes.ToArray());
//
//                var total = string.Join("\n", _GetErrors(result.Errors).ToArray());
//
//                throw new Exception(total);
//            }
//
//            var ghostTypeName = "DataDefine.Ghost.C" + base_type.Name;
//            return result.CompiledAssembly.GetType(ghostTypeName);
//        }

        private IEnumerable<string> _GetErrors(IEnumerable errors)
        {
            foreach(var error in errors)
            {
                yield return error.ToString();
            }
        }
    }
}
