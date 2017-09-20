using System;
using System.Linq;

namespace Serialization
{
    public class TypeIdentifier
    {
        private static readonly Type[] _NumberTypes =
            {
                typeof(short),
                typeof(ushort),
                typeof(int),
                typeof(uint),
                typeof(bool),
                typeof(long),
                typeof(ulong)
            };

        private static readonly Type[] _BufferTypes =
            {
                typeof(char[])
            };

        private static readonly Type[] _BittableTypes =
            {
                typeof(float),
                typeof(decimal),
                typeof(double),
                typeof(Guid),
                typeof(char),
                typeof(byte)
            };

        public readonly ITypeDescriber Describer;

        public TypeIdentifier(Type type, int id)
        {
            if(TypeIdentifier._IsEnum(type))
            {
                Describer = new EnumDescriber(id, type);
            }
            else if(TypeIdentifier._IsNumber(type))
            {
                Describer = new NumberDescriber(id, type);
            }
            else if(TypeIdentifier._IsByteArray(type))
            {
                Describer = new ByteArrayDescriber(id);
            }
            else if(TypeIdentifier._IsBuffer(type))
            {
                Describer = new BufferDescriber(id, type);
            }
            else if(TypeIdentifier._IsBittable(type))
            {
                Describer = new BlittableDescriber(id, type);
            }
            else if(TypeIdentifier._IsString(type))
            {
                Describer = new StringDescriber(id);
            }
            else if(_IsArray(type))
            {
                Describer = new ArrayDescriber(id, type);
            }
            else if(TypeIdentifier._IsClass(type))
            {
                Describer = new ClassDescriber(id, type);
            }
            else
            {
                throw new Exception("Unrecognized type " + type.FullName);
            }
        }

        private static bool _IsByteArray(Type type)
        {
            return type == typeof(byte[]);
        }

        private static bool _IsBuffer(Type type)
        {
            return TypeIdentifier._BufferTypes.Any(t => t == type);
        }

        private static bool _IsBittable(Type type)
        {
            return TypeIdentifier._BittableTypes.Any(t => t == type);
        }

        private static bool _IsString(Type type)
        {
            return type == typeof(string);
        }

        private static bool _IsEnum(Type type)
        {
            return type.IsEnum;
        }

        public static bool IsClass(Type type)
        {
            return TypeIdentifier._IsClass(type);
        }

        private static bool _IsClass(Type type)
        {
            return type.IsByRef == false && type.IsAbstract == false && type.IsInterface == false && type.IsCOMObject == false && type.IsSpecialName == false && type.IsSubclassOf(typeof(Delegate)) == false;
        }

        private bool _IsArray(Type type)
        {
            return type.IsArray;
        }

        private static bool _IsNumber(Type type)
        {
            return TypeIdentifier._NumberTypes.Any(t => t == type);
        }

        public static bool IsAtom(Type type)
        {
            return TypeIdentifier._IsNumber(type) || TypeIdentifier._IsBittable(type) || TypeIdentifier._IsBuffer(type) || TypeIdentifier._IsByteArray(type) || TypeIdentifier._IsEnum(type);
        }

        public static bool IsString(Type type)
        {
            return TypeIdentifier._IsString(type);
        }
    }
}
