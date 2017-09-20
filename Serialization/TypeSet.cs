using System;
using System.Linq;

namespace Serialization
{
    public class TypeSet
    {
        private readonly ITypeDescriber[] _Describers;

        public TypeSet(ITypeDescriber[] describers)
        {
            _Describers = describers;
        }

        public ITypeDescriber GetByType(Type type)
        {
            try
            {
                return _Describers.First(d => d.Type == type);
            }
            catch (Exception)
            {

                throw new Exception($"沒有 類型{type.FullName}.");
            }

        }

        public ITypeDescriber GetById(int id)
        {
            try
            {
                return _Describers.First(d => d.Id == id);
            }
            catch (Exception)
            {

                throw new Exception($"沒有Id {id}.");
            }

        }
    }
}