using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Library.Utility
{
    public class CodeBuilder
    {
        public event Action<string, string, string> OnEventEvent;

        public event Action<string, string> GpiEvent;

        private static readonly string _GhostIdName = "_GhostIdName";

        public void Build(Type type)
        {
            if(!type.IsInterface)
            {
            }

            var ghostClassCode = _BuildGhostCode(type);

            var typeName = _GetTypeName(type);

            if(GpiEvent != null)
            {
                GpiEvent(typeName, ghostClassCode);
            }

            foreach(var eventInfo in type.GetEvents())
            {
                var eventCode = _BuildEventCode(type, eventInfo);

                OnEventEvent?.Invoke(typeName, eventInfo.Name, eventCode);
            }
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
            foreach(var type in types)
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

            foreach(var info in eventInfos)
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

            foreach(var info in propertyInfos)
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
            var id = 0;
            foreach(var methodInfo in methods)
            {
                if(methodInfo.IsSpecialName)
                {
                    continue;
                }

                var haveReturn = methodInfo.ReturnType != typeof(void);
                var returnTypeCode = "void";
                if(haveReturn)
                {
                    var returnType = methodInfo.ReturnType.GetGenericArguments()[0];

                    returnTypeCode = $"Library.Synchronize.Value<{_GetTypeName(returnType)}>";
                }

                var returnValue = string.Empty;

                var addReturn = "Library.Synchronize.IValue returnValue = null;";

                if(haveReturn)
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
                    _CallMethodEvent(info , new object[] {{{addParams}}} , returnValue);                    
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

            for(var i = 0; i < parameters.Length; i++)
            {
                addParams.Add("_" + (i + 1));
            }

            return string.Join(" ,", addParams.ToArray());
        }

        private string _GetTypes(Type[] generic_type_arguments)
        {
            var code = from t in generic_type_arguments
                       select $"{_GetTypeName(t)}";

            if(code.Any())
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
            return $"{type.Namespace}.Event.{type.Name}.{event_name}";
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
    
            namespace {nameSpace}.Event.{name} 
            {{ 
                public class {eventName} : Synchronization.IEventProxyCreator
                {{
                    Type _Type;
                    string _Name;
            
                    public {eventName}()
                    {{
                        _Name = ""{eventName}"";
                        _Type = typeof({type.FullName});                   
            
                    }}
    
                    Delegate Regulus.Synchronization.IEventProxyCreator.Create(Guid soul_id, int event_id, Synchronization.InvokeEventCallback invoke_event)
                    {{                
                        var closure = new Synchronization.GenericEventClosure{_GetTypes(argTypes)}(soul_id, event_id, invoke_event);                
                        return new Action{_GetTypes(argTypes)}(closure.Run);
                    }}
        
                    Type Synchronization.IEventProxyCreator.GetType()
                    {{
                        return _Type;
                    }}            

                    string Synchronization.IEventProxyCreator.GetName()
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
            // return string.Join(",", code.Select(val => val.ToString()).ToArray());
            return string.Empty;
        }
    }
}
