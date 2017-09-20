using System;

namespace Serialization
{
    public interface ITypeDescriber
    {
        int Id { get; }

        Type Type { get; }

        object Default { get; }

        int GetByteCount(object instance);

        int ToBuffer(object instance, byte[] buffer, int begin);

        int ToObject(byte[] buffer, int begin, out object instance);

        void SetMap(TypeSet type_set);
    }
}
