using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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

            var codes = AssemblyBuilder._BuildCode(assembly, protocol_name);
            File.WriteAllLines("dump.cs", codes.ToArray());

            var options = new CompilerParameters
            {
                IncludeDebugInformation = true,
                GenerateInMemory = false,
                GenerateExecutable = false,
                OutputAssembly = output_path,
                //TempFiles = new TempFileCollection(@"D:\VOH_SVN\common\VoH.Game\VoH.Define\bin\Debug\out", true)
            };

            _FindAllAssemblies(options.ReferencedAssemblies, assembly);
            options.ReferencedAssemblies.Add("Gateway.dll");
            options.ReferencedAssemblies.Add("Synchronization.dll");
            options.ReferencedAssemblies.Add("Serialization.dll");

            var result = provider.CompileAssemblyFromSource(options, codes.ToArray());
            if(result.Errors.Count <= 0)
            {
                return;
            }

            File.WriteAllLines("error.log", _GetErrors(result.Errors).ToArray());

            throw new Exception("Protocol compile error");
        }

        private void _FindAllAssemblies(StringCollection options_referenced_assemblies, Assembly assembly)
        {
            var locations = new HashSet<string>();
            var checkedTypes = new HashSet<Type>();
            var types = assembly.GetExportedTypes();

            foreach(var type in types)
            {
                _Record(type, locations, checkedTypes);
            }

            foreach(var location in locations)
            {
                if(location.Contains("mscorlib") == false)
                {
                    options_referenced_assemblies.Add(location);
                }
            }
        }

        private void _Record(Type type, HashSet<string> locations, HashSet<Type> checked_types)
        {
            if(type == null || checked_types.Contains(type))
            {
                return;
            }

            checked_types.Add(type);

            if(type == typeof(object))
            {
                return;
            }

            locations.Add(type.Assembly.Location);
            _Record(type.BaseType, locations, checked_types);

            var methods = type.GetMethods();

            foreach(var methodInfo in methods)
            {
                if(methodInfo.IsGenericMethod || methodInfo.IsPrivate)
                {
                    continue;
                }

                foreach(var parameterInfo in methodInfo.GetParameters())
                {
                    _Record(parameterInfo.ParameterType, locations, checked_types);
                }

                var returnParam = methodInfo.ReturnParameter;
                if(returnParam == null)
                {
                    continue;
                }

                var returnType = returnParam.ParameterType;
                if(returnType.IsGenericType
                   && returnType.GetGenericTypeDefinition() == typeof(Gateway.Synchronize.Value<>))
                {
                    foreach(var argType in returnType.GetGenericArguments())
                    {
                        _Record(argType, locations, checked_types);
                    }
                }
            }

            var properties = type.GetProperties(BindingFlags.Public);

            foreach(var propertyInfo in properties)
            {
                _Record(propertyInfo.PropertyType, locations, checked_types);
            }

            var events = type.GetEvents(BindingFlags.Public);

            foreach(var e in events)
            {
                if(e.IsSpecialName)
                {
                    continue;
                }

                _Record(e.EventHandlerType, locations, checked_types);
            }
        }

        // static build in memory
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
        // public Type Build(Type base_type, string library_path, string synchronization_path)
        // {
        // var optionsDic = new Dictionary<string, string>
        // {
        // {
        // "CompilerVersion", "v4.5"
        // }
        // };
        // var provider = new CSharpCodeProvider(optionsDic);
        // var options = new CompilerParameters
        // {
        // GenerateInMemory = true,
        // GenerateExecutable = false,
        // ReferencedAssemblies =
        // {
        // "System.Core.dll",
        // "System.xml.dll",
        // library_path,
        // synchronization_path,
        // base_type.Assembly.Location
        // },
        // TempFiles = new TempFileCollection()
        // };
        // var codes = new List<string>();
        // var codeBuilder = new Library.Utility.CodeBuilder();
        // codeBuilder.OnEventEvent += (type_name, event_name, code) => codes.Add(code);
        // codeBuilder.GpiEvent += (type_name, code) => { codes.Add(code); };
        // codeBuilder.Build(base_type);
        // var result = provider.CompileAssemblyFromSource(options, codes.ToArray());
        // if(result.Errors.Count > 0)
        // {
        // File.WriteAllLines("dump.cs", codes.ToArray());
        // var total = string.Join("\n", _GetErrors(result.Errors).ToArray());
        // throw new Exception(total);
        // }
        // var ghostTypeName = "DataDefine.Ghost.C" + base_type.Name;
        // return result.CompiledAssembly.GetType(ghostTypeName);
        // }
        private IEnumerable<string> _GetErrors(IEnumerable errors)
        {
            foreach(var error in errors)
            {
                yield return error.ToString();
            }
        }
    }
}
