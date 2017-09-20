using System;

namespace Serialization
{
    public class ByteArrayDescriber : ITypeDescriber
    {
        private readonly int _Id;

        private ITypeDescriber _IntTypeDescriber;

        public ByteArrayDescriber(int id)
        {
            _Id = id;
        }

        int ITypeDescriber.Id => _Id;

        Type ITypeDescriber.Type => typeof(byte[]);

        object ITypeDescriber.Default => null;

        int ITypeDescriber.GetByteCount(object instance)
        {
            var array = instance as Array;
            var len = array.Length;
            var lenByetCount = _IntTypeDescriber.GetByteCount(len);
            return len + lenByetCount;
        }

        int ITypeDescriber.ToBuffer(object instance, byte[] buffer, int begin)
        {
            var array = instance as byte[];
            var len = array.Length;

            var offset = begin;

            offset += _IntTypeDescriber.ToBuffer(len, buffer, offset);
            for(var i = 0; i < len; i++)
            {
                buffer[offset++] = array[i];
            }

            return offset - begin;
        }

        int ITypeDescriber.ToObject(byte[] buffer, int begin, out object instance)
        {
            var offset = begin;
            object lenObject = null;
            offset += _IntTypeDescriber.ToObject(buffer, offset, out lenObject);

            var len = (int)lenObject;
            var array = new byte[len];
            for(var i = 0; i < len; i++)
            {
                array[i] = buffer[offset++];
            }

            instance = array;
            return offset - begin;
        }

        void ITypeDescriber.SetMap(TypeSet type_set)
        {
            var type = type_set.GetByType(typeof(int));
            _IntTypeDescriber = type;
        }
    }
}
