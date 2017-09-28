using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

using Synchronization.Data;

namespace Protocol
{
    public class CodeBuilder
    {
        public event Action<string, string, string> OnEventEvent;

        public event Action<string, string> OnProviderEvent;

        public event Action<string, string> OnGpiEvent;

        

        private static readonly string _GhostIdName = "_GhostIdName";

        public void Build(string protocol_name, Type[] types)
        {
            var codeGpis = new List<string>();
            var codeEvents = new List<string>();


            var addGhostType = new List<string>();
            var addEventType = new List<string>();

            var serializerTypes = new HashSet<Type>();

            var memberMapMethodBuilder = new List<string>();
            var memberMapEventBuilder = new List<string>();
            var memberMapPropertyBuilder = new List<string>();

            var memberMapInterfaceBuilder = new List<string>();

            foreach(var type in types)
            {
                serializerTypes.Add(type);

                if (!type.IsInterface)
                {
                    continue;
                }

                var ghostClassCode = _BuildGhostCode(type);

                var typeName = _GetTypeName(type);

                addGhostType.Add($"types.Add(typeof({typeName}) , typeof({_GetGhostType(type)}) );");

                codeGpis.Add(ghostClassCode);

                OnGpiEvent?.Invoke(typeName, ghostClassCode);

                // build event
                foreach (var eventInfo in type.GetEvents())
                {
                    addEventType.Add($"eventClosures.Add(new {_GetEventType(type, eventInfo.Name)}() );");

                    var eventCode = _BuildEventCode(type, eventInfo);

                    codeEvents.Add(eventCode);

                    OnEventEvent?.Invoke(typeName, eventInfo.Name, eventCode);

                    memberMapEventBuilder.Add($"typeof({type.FullName}).GetEvent(\"{eventInfo.Name}\")");
                }

                // build method
                var methodInfos = type.GetMethods();

                foreach (var methodInfo in methodInfos)
                {
                    if(methodInfo.IsPublic && methodInfo.IsSpecialName == false)
                    {
                        foreach(var parameterInfo in methodInfo.GetParameters())
                        {
                            serializerTypes.Add(parameterInfo.ParameterType);
                        }

                        memberMapMethodBuilder.Add($"typeof({type.FullName}).GetMethod(\"{methodInfo.Name}\")");
                    }
                }

                // build property
                var propertyInfos = type.GetProperties();

                foreach (var propertyInfo in propertyInfos)
                {
                    memberMapPropertyBuilder.Add($"typeof({type.FullName}).GetProperty(\"{propertyInfo.Name}\")");
                }

                memberMapInterfaceBuilder.Add($"typeof({type.FullName})");
            }

            var addMemberMapInterfaceCode = string.Join(",", memberMapInterfaceBuilder.ToArray());
            var addMemberMapPropertyCode = string.Join(",", memberMapPropertyBuilder.ToArray());
            var addMemberMapEventCode = string.Join(",", memberMapEventBuilder.ToArray());
            var addMemberMapMethodCode = string.Join(",", memberMapMethodBuilder.ToArray());
            var addTypeCode = string.Join("\n", addGhostType.ToArray());
            var addDescriberCode = string.Join(",", _GetSerializarType(serializerTypes));
            var addEventCode = string.Join("\n", addEventType.ToArray());
            var tokens = protocol_name.Split(new[] { '.' });
            var protocolName = tokens.Last();

            var providerNamespace = string.Join(".", tokens.Take(tokens.Count() - 1).ToArray());

            var providerNamespaceHead = string.Empty;

            var providerNamespaceTail = string.Empty;

            if (string.IsNullOrEmpty(providerNamespace) == false)
            {
                providerNamespaceHead = $"namespace {providerNamespace}{{ ";
                providerNamespaceTail = "}";
            }

            var builder = new StringBuilder();
            builder.Append(addTypeCode);
            builder.Append(addEventCode);
            builder.Append(addDescriberCode);

            var verificationCode = _BuildVerificationCode(builder);

            var providerCode =
                    $@"
            using System;  
            using System.Collections.Generic;
            
            {providerNamespaceHead}
                public class {protocolName} : Synchronization.PreGenerated.IProtocol
                {{
                    Synchronization.PreGenerated.InterfaceProvider _InterfaceProvider;
                    Synchronization.PreGenerated.EventProvider _EventProvider;
                    Synchronization.PreGenerated.MemberMap _MemberMap;
                    Library.Serialization.ISerializer _Serializer;
                    public {protocolName}()
                    {{
                        var types = new Dictionary<Type, Type>();
                        {addTypeCode}
                        _InterfaceProvider = new Synchronization.PreGenerated.InterfaceProvider(types);

                        var eventClosures = new List<Synchronization.Interface.IEventProxyCreator>();
                        {addEventCode}
                        _EventProvider = new  Synchronization.PreGenerated.EventProvider(eventClosures);

                        _Serializer = new Serialization.Serializer(new Serialization.DescriberBuilder({addDescriberCode}));


                        _MemberMap = new Synchronization.PreGenerated.MemberMap(new System.Reflection.MethodInfo[] {{{addMemberMapMethodCode}}} ,new System.Reflection.EventInfo[]{{ {addMemberMapEventCode} }}, new System.Reflection.PropertyInfo[] {{{addMemberMapPropertyCode} }}, new Type[] {{{addMemberMapInterfaceCode}}});
                    }}

                    byte[] Synchronization.PreGenerated.IProtocol.VerificationCode {{ get {{ return new byte[]{{{verificationCode}}};}} }}
                    Synchronization.PreGenerated.InterfaceProvider Synchronization.PreGenerated.IProtocol.GetInterfaceProvider()
                    {{
                        return _InterfaceProvider;
                    }}

                    Synchronization.PreGenerated.EventProvider Synchronization.PreGenerated.IProtocol.GetEventProvider()
                    {{
                        return _EventProvider;
                    }}

                    Library.Serialization.ISerializer Synchronization.PreGenerated.IProtocol.GetSerialize()
                    {{
                        return _Serializer;
                    }}

                    Synchronization.PreGenerated.MemberMap Synchronization.PreGenerated.IProtocol.GetMemberMap()
                    {{
                        return _MemberMap;
                    }}
                    
                }}
            {providerNamespaceTail}
            ";

            OnProviderEvent?.Invoke(protocol_name, providerCode);
        }

        private string[] _GetSerializarType(HashSet<Type> serializer_types)
        {
            var types = new HashSet<Type>();

            serializer_types.Add(typeof(PackageProtocolSubmit));
            serializer_types.Add(typeof(RequestPackage));
            serializer_types.Add(typeof(ResponsePackage));
            serializer_types.Add(typeof(PackageUpdateProperty));
            serializer_types.Add(typeof(PackageInvokeEvent));
            serializer_types.Add(typeof(PackageErrorMethod));
            serializer_types.Add(typeof(PackageReturnValue));
            serializer_types.Add(typeof(PackageLoadSoulCompile));
            serializer_types.Add(typeof(PackageLoadSoul));
            serializer_types.Add(typeof(PackageUnloadSoul));
            serializer_types.Add(typeof(PackageCallMethod));
            serializer_types.Add(typeof(PackageRelease));

            foreach (var serializerType in serializer_types)
            {

                foreach (var type in new TypeDisintegrator(serializerType).Types)
                {
                    
                    types.Add(type);
                }
            }
            var typeCodes = (from type in types orderby type.FullName
                             select new { Code = "typeof(" + _GetTypeName(type) + ")" , Assembly = type.Assembly.Location }).ToArray();

            
        
            //            foreach (var type in types)
            //            {
            //                Regulus.Utility.Log.Instance.WriteInfo(type.FullName);
            //            }
            //            Regulus.Utility.Log.Instance.WriteInfo("Serializable object " + types.Count);

            return typeCodes.Select( t =>t.Code).ToArray();
        }

        private string _BuildGhostCode(Type type)
        {
            var nameSpace = type.Namespace;

            var name = type.Name;

            var types = type.GetInterfaces()
                            .Concat(
                                    new[]
                                        {
                                            type
                                        });

            var implementCode = _BuildGhostCode(types);

            var codeHeader = $@"   
    using System;  
    
    using System.Collections.Generic;
    
    namespace {nameSpace}.Ghost 
    {{ 
        public class C{name} : {_GetTypeName(type)}, Library.Synchronize.IGhost
        {{
            private event Library.Synchronize.CallMethodCallback _OnCallMethodEvent;
            
            event Library.Synchronize.CallMethodCallback Library.Synchronize.IGhost.CallMethodEvent
            {{
                add {{ this._OnCallMethodEvent += value; }}
                remove {{ this._OnCallMethodEvent -= value; }}
            }}

            readonly bool _HaveReturn ;
            
            readonly Guid {CodeBuilder._GhostIdName};

            readonly Type _GhostType;

            public C{name}(Guid id, Type ghost_type, bool have_return )
            {{
                _HaveReturn = have_return ;
                {CodeBuilder._GhostIdName} = id;            
                _GhostType = ghost_type;
            }}
            
            Guid Library.Synchronize.IGhost.GetID()
            {{
                return {CodeBuilder._GhostIdName};
            }}

            object Library.Synchronize.IGhost.GetInstance()
            {{
                return this;
            }}

            bool Library.Synchronize.IGhost.IsReturnType()
            {{
                return _HaveReturn;
            }}

            Type Library.Synchronize.IGhost.GetType()
            {{
                return _GhostType;
            }}
            
            {implementCode}
        }}
    }}";
            return codeHeader;
        }

        private string _BuildGhostCode(IEnumerable<Type> types)
        {
            var codes = new List<string>();
            foreach (var type in types)
            {
                var events = _BuildEvents(type);
                var properties = _BuildProperties(type);
                var methods = _BuildMethods(type);

                codes.Add(methods);
                codes.Add(properties);
                codes.Add(events);
            }

            return string.Join("\n", codes.ToArray());
        }

        private string _BuildEvents(Type type)
        {
            var eventInfos = type.GetEvents();
            var codes = new List<string>();

            foreach (var info in eventInfos)
            {
                var code = $@"
                System.Action{_GetTypes(info.EventHandlerType.GetGenericArguments())} _{info.Name};
                
                event System.Action{_GetTypes(info.EventHandlerType.GetGenericArguments())} {_GetTypeName(type)}.{info.Name}
                {{
                    add {{ _{info.Name} += value;}}
                    remove {{ _{info.Name} -= value;}}
                }}";
                codes.Add(code);
            }

            return string.Join("\n", codes.ToArray());
        }

        private string _BuildProperties(Type type)
        {
            var propertyInfos = type.GetProperties();
            var propertyCodes = new List<string>();

            foreach (var info in propertyInfos)
            {
                var propertyCode = $@"
                {_GetTypeName(info.PropertyType)} _{info.Name};

                {_GetTypeName(info.PropertyType)} {_GetTypeName(type)}.{info.Name} {{ get{{ return _{info.Name};}} }}";
                propertyCodes.Add(propertyCode);
            }

            return string.Join("\n", propertyCodes.ToArray());
        }

        private string _BuildMethods(Type type)
        {
            var methodCodes = new List<string>();
            var methods = type.GetMethods();
            //var id = 0;
            foreach (var methodInfo in methods)
            {
                if (methodInfo.IsSpecialName)
                {
                    continue;
                }

                var haveReturn = methodInfo.ReturnType != typeof(void);
                var returnTypeCode = "void";
                if (haveReturn)
                {
                    var returnType = methodInfo.ReturnType.GetGenericArguments()[0];

                    returnTypeCode = $"Library.Synchronize.Value<{_GetTypeName(returnType)}>";
                }

                var returnValue = string.Empty;

                var addReturn = "Library.Synchronize.IValue returnValue = null;";

                if (haveReturn)
                {
                    addReturn = $@" var returnValue = new {returnTypeCode}();";
                    returnValue = "return returnValue;";
                }

                var addParams = _BuildAddParams(methodInfo);
                var paramId = 0;
                var paramCode = string.Join(
                                            ",", (from p in methodInfo.GetParameters()
                                                  select $"{_GetTypeName(p.ParameterType)} {"_" + ++paramId}").ToArray());
                var methodCode = $@"
                {returnTypeCode} {type.Namespace}.{type.Name}.{methodInfo.Name}({paramCode})
                {{                    
                    {addReturn}
                    var info = typeof({methodInfo.DeclaringType}).GetMethod(""{methodInfo.Name}"");
                    _OnCallMethodEvent(info , new object[] {{{addParams}}} , returnValue);                    
                    {returnValue}
                }}";

                methodCodes.Add(methodCode);
            }

            return string.Join(" \n", methodCodes.ToArray());
        }

        private string _GetTypeName(Type type)
        {
            return type.FullName.Replace("+", ".");
        }

        private string _BuildAddParams(MethodInfo method_info)
        {
            var parameters = method_info.GetParameters();

            var addParams = new List<string>();

            for (var i = 0; i < parameters.Length; i++)
            {
                addParams.Add("_" + (i + 1));
            }

            return string.Join(" ,", addParams.ToArray());
        }

        private string _GetTypes(IEnumerable<Type> generic_type_arguments)
        {
            var code = from t in generic_type_arguments
                       select $"{_GetTypeName(t)}";

            if (code.Any())
            {
                return "<" + string.Join(",", code.ToArray()) + ">";
            }

            return string.Empty;
        }

        private string _GetGhostType(Type type)
        {
            return $"{type.Namespace}.Ghost.C{type.Name}";
        }

        private string _GetEventType(Type type, string event_name)
        {
            return $"{type.Namespace}.EventId.{type.Name}.{event_name}";
        }

        private string _BuildEventCode(Type type, EventInfo info)
        {
            var nameSpace = type.Namespace;
            var name = type.Name;

            var argTypes = info.EventHandlerType.GetGenericArguments();
            var eventName = info.Name;
            return $@"
            using System;  
            using System.Collections.Generic;
    
            namespace {nameSpace}.EventId.{name} 
            {{ 
                public class {eventName} : Synchronization.Interface.IEventProxyCreator
                {{
                    Type _Type;
                    string _Name;
            
                    public {eventName}()
                    {{
                        _Name = ""{eventName}"";
                        _Type = typeof({type.FullName});                   
            
                    }}
    
                    Delegate Synchronization.Interface.IEventProxyCreator.Create(Guid soul_id, int event_id, Synchronization.PreGenerated.InvokeEventCallback invoke_event)
                    {{                
                        var closure = new Synchronization.PreGenerated.GenericEventClosure{_GetTypes(argTypes)}(soul_id, event_id, invoke_event);                
                        return new Action{_GetTypes(argTypes)}(closure.Run);
                    }}
        
                    Type Synchronization.Interface.IEventProxyCreator.GetType()
                    {{
                        return _Type;
                    }}            

                    string Synchronization.Interface.IEventProxyCreator.GetName()
                    {{
                        return _Name;
                    }}            
                }}
            }}";
        }

        private string _BuildVerificationCode(StringBuilder builder)
        {
            var md5 = MD5.Create();
            var code = md5.ComputeHash(Encoding.Default.GetBytes(builder.ToString()));

            // Regulus.Utility.Log.Instance.WriteInfo("Verification Code " + Convert.ToBase64String(code));
             return string.Join(",", code.Select(val => val.ToString()).ToArray());
            //return string.Empty;
        }
    }
}
