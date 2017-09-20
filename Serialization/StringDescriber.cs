using System;

namespace Serialization
{
    public class StringDescriber : ITypeDescriber
    {
        private readonly int _Id;

        private ITypeDescriber _CharArrayDescriber;

        public StringDescriber(int id)
        {
            _Id = id;
        }

        int ITypeDescriber.Id => _Id;

        Type ITypeDescriber.Type => typeof(string);

        object ITypeDescriber.Default => null;

        int ITypeDescriber.GetByteCount(object instance)
        {
            var str = instance as string;
            var chars = str.ToCharArray();

            var charCount = _CharArrayDescriber.GetByteCount(chars);

            return charCount;
        }

        int ITypeDescriber.ToBuffer(object instance, byte[] buffer, int begin)
        {
            var str = instance as string;
            var chars = str.ToCharArray();
            var offset = begin;
            offset += _CharArrayDescriber.ToBuffer(chars, buffer, offset);
            return offset - begin;
        }

        int ITypeDescriber.ToObject(byte[] buffer, int begin, out object instnace)
        {
            var offset = begin;
            object chars;
            offset += _CharArrayDescriber.ToObject(buffer, offset, out chars);

            instnace = new string(chars as char[]);

            return offset - begin;
        }

        void ITypeDescriber.SetMap(TypeSet type_set)
        {
            _CharArrayDescriber = type_set.GetByType(typeof(char[]));
        }
    }
}
