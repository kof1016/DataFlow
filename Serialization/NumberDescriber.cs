using System;

using Gateway.Serialization;

namespace Serialization
{
    public class NumberDescriber<T> : NumberDescriber
    {
        public NumberDescriber(int id)
            : base(id, typeof(T))
        {
        }
    }

    public class NumberDescriber : ITypeDescriber
    {
        private readonly int _Id;

        public NumberDescriber(int id, Type type)
        {
            Default = Activator.CreateInstance(type);
            _Id = id;
            Type = type;
        }

        int ITypeDescriber.Id => _Id;

        public Type Type { get; }

        int ITypeDescriber.GetByteCount(object instance)
        {
            try
            {
                
                return Varint.GetByteCount((ulong)Convert.ToInt64(instance));
            }
            catch (System.Exception e)
            {
                throw new System.Exception($"GetByteCount OverflowException typr:{instance.GetType()} val:{instance}", e);            }
            
        }

        public object Default { get; }

        int ITypeDescriber.ToBuffer(object instance, byte[] buffer, int begin)
        {
            return Varint.NumberToBuffer(buffer, begin, (ulong)Convert.ToInt64(instance));
        }

        int ITypeDescriber.ToObject(byte[] buffer, int begin, out object instance)
        {
            ulong value;
            var readed = Varint.BufferToNumber(buffer, begin, out value);
            if(Type == typeof(ulong))
            {
                instance = value;
            }
            else if (Type == typeof(long))
            {
                instance = (long)value;
            }
            else if (Type == typeof(uint))
            {
                instance = (uint)value;
            }
            else if(Type == typeof(int))
            {
                instance = (int)value;
            }
            else
                instance = Convert.ChangeType(value, Type);
            return readed;
        }

        public void SetMap(TypeSet type_set)
        {
        }
    }
}
