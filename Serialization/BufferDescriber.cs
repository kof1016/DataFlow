using System;

using Library.Serialization;

namespace Serialization
{
    public class BufferDescriber<T> : BufferDescriber
    {
        public BufferDescriber(int id)
            : base(id, typeof(T))
        {
        }
    }

    public class BufferDescriber : ITypeDescriber
    {
        private readonly int _Id;

        private readonly Type _Type;

        public BufferDescriber(int id, Type type)
        {
            if(type.IsArray == false)
            {
                throw new ArgumentException("type is not array " + type.FullName);
            }

            _Type = type;
            _Id = id;
        }

        int ITypeDescriber.Id => _Id;

        Type ITypeDescriber.Type => _Type;

        object ITypeDescriber.Default => null;

        int ITypeDescriber.GetByteCount(object instance)
        {
            var src = instance as Array;
            var bufferLength = Buffer.ByteLength(src);
            var bufferLen = Varint.GetByteCount(bufferLength);
            var elementLen = Varint.GetByteCount(src.Length);
            return bufferLen + bufferLength + elementLen;
        }

        int ITypeDescriber.ToBuffer(object instance, byte[] buffer, int begin)
        {
            var src = instance as Array;
            var bufferLength = Buffer.ByteLength(src);
            var offset = begin;
            offset += Varint.NumberToBuffer(buffer, offset, bufferLength);
            offset += Varint.NumberToBuffer(buffer, offset, src.Length);

            Buffer.BlockCopy(src, 0, buffer, offset, bufferLength);
            return offset - begin + bufferLength;
        }

        int ITypeDescriber.ToObject(byte[] buffer, int begin, out object instnace)
        {
            var offset = begin;
            var bufferLen = 0;
            offset += Varint.BufferToNumber(buffer, offset, out bufferLen);
            var elementLen = 0;
            offset += Varint.BufferToNumber(buffer, offset, out elementLen);
            var dst = _Create(elementLen);
            Buffer.BlockCopy(buffer, offset, dst, 0, bufferLen);

            instnace = dst;
            return offset - begin + bufferLen;
        }

        void ITypeDescriber.SetMap(TypeSet type_set)
        {
        }

        private Array _Create(int len)
        {
            return Activator.CreateInstance(_Type, len) as Array;
        }
    }
}
