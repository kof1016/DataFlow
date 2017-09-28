using System;
using System.Collections.Generic;
using System.Linq;

using Library.Synchronize;

using Serialization;

namespace Protocol
{
    public class TypeDisintegrator
    {
        public readonly Type[] Types;

        public TypeDisintegrator(Type type)
        {
            var types = new HashSet<Type>();

            if(_IsAtom(type))
            {
                types.Add(type);
            }
            else if(_IsString(type))
            {
                types.Add(typeof(char));
                types.Add(typeof(char[]));
                types.Add(typeof(string));
            }
            else if(_IsArray(type))
            {
                types.Add(type);
                TypeDisintegrator._Add(
                                       new[]
                                           {
                                               type.GetElementType()
                                           }, 
                                       types);
            }
            else if(_IsType(type))
            {
                if(TypeIdentifier.IsClass(type))
                {
                    types.Add(type);
                }

                TypeDisintegrator._Add(_GetEvents(type), types);
                TypeDisintegrator._Add(_GetPropertys(type), types);
                TypeDisintegrator._Add(_GetMethods(type), types);
                TypeDisintegrator._Add(_GetFields(type), types);

            }

            Types = types.ToArray();
        }

        protected TypeDisintegrator(HashSet<Type> types, Type type)
        {
            Types = new Type[0];
            if(types.Contains(type) == false && TypeDisintegrator._Valid(type))
            {
                Types = new TypeDisintegrator(type).Types;
            }
        }

        private bool _IsString(Type type)
        {
            return TypeIdentifier.IsString(type);
        }

        private bool _IsArray(Type type)
        {
            return type.IsArray && type != typeof(object[]);
        }

        private static void _Add(IEnumerable<Type> types1, HashSet<Type> types)
        {
            foreach(var eType in types1.SelectMany(t => new TypeDisintegrator(types, t).Types))
            {
                types.Add(eType);
            }
        }

        private bool _IsType(Type type)
        {
            return type.IsInterface || TypeIdentifier.IsClass(type);
        }

        private bool _IsAtom(Type type)
        {
            return TypeIdentifier.IsAtom(type);
        }

        private static bool _Valid(Type type)
        {
            return type.IsByRef == false && type.IsInterface == false && type.IsGenericType == false && type.IsAbstract == false && type != typeof(object);
        }

        private IEnumerable<Type> _GetPropertys(Type type)
        {
            foreach(var propertyInfo in type.GetProperties())
            {
                yield return propertyInfo.PropertyType;
            }
        }

        private IEnumerable<Type> _GetEvents(Type type)
        {
            foreach(var eventInfo in type.GetEvents())
            {
                var method = eventInfo.GetRaiseMethod();
                var handleType = eventInfo.EventHandlerType;
                if(handleType.IsGenericType)
                {
                    var args = handleType.GetGenericArguments();
                    foreach(var type1 in args)
                    {
                        yield return type1;
                    }
                }
                else
                {
                    foreach(var parameterInfo in handleType.GetMethod("Invoke").GetParameters())
                    {
                        yield return parameterInfo.ParameterType;
                    }
                }
            }
        }

        private IEnumerable<Type> _GetMethods(Type type)
        {
            var types = new List<Type>();
            foreach(var methodInfo in type.GetMethods())
            {
                if(methodInfo.IsGenericMethod || methodInfo.IsHideBySig )
                {
                    continue;
                }

                types.AddRange(methodInfo.GetParameters().Select(m => m.ParameterType));

                var retType = methodInfo.ReturnType;

                if(retType.IsGenericType && retType.GetGenericTypeDefinition() == typeof(Value<>))
                {
                    types.AddRange(retType.GetGenericArguments());
                }
            }

            return types;
        }

        private IEnumerable<Type> _GetFields(Type type)
        {
            foreach(var fieldInfo in type.GetFields())
            {
                if(fieldInfo.IsPublic && fieldInfo.IsSpecialName == false)
                {
                    yield return fieldInfo.FieldType;
                }
            }
        }
    }
}
