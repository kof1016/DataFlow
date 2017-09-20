using System;

using Library.Serialization;

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
            return Varint.GetByteCount(Convert.ToUInt64(instance));
        }

        public object Default { get; }

        int ITypeDescriber.ToBuffer(object instance, byte[] buffer, int begin)
        {
            return Varint.NumberToBuffer(buffer, begin, Convert.ToUInt64(instance));
        }

        int ITypeDescriber.ToObject(byte[] buffer, int begin, out object instance)
        {
            ulong value;
            var readed = Varint.BufferToNumber(buffer, begin, out value);
            instance = Convert.ChangeType(value, Type);
            return readed;
        }

        public void SetMap(TypeSet type_set)
        {
        }
    }
}
