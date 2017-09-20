using System;
using System.Collections.Generic;

namespace Serialization
{
    public class DescriberBuilder
    {
        public readonly ITypeDescriber[] Describers;
        public DescriberBuilder(params Type[] types)
        {
            Describers = _BuildDescribers(types);
        }
        ITypeDescriber[] _BuildDescribers(params Type[] types)
        {
            int id = 0;
            var describers = new List<ITypeDescriber>();
            foreach (var type in types)
            {
                var identifier = new TypeIdentifier(type, ++id);
                describers.Add(identifier.Describer);

            }

            return describers.ToArray();
        }

    }
}
