using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    internal class GenericTypeContext : ICustomAttributeProvider
    {
        private readonly Attribute[] _attributes;

        public GenericTypeContext(IEnumerable<Attribute> attributes)
        {
            _attributes = attributes.ToArray();
        }

        public object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return _attributes.Where(a => attributeType.IsAssignableFrom(a.GetType())).ToArray();
        }

        public object[] GetCustomAttributes(bool inherit)
        {
            return _attributes;
        }

        public bool IsDefined(Type attributeType, bool inherit)
        {
            return _attributes.Any(a => attributeType.IsAssignableFrom(a.GetType()));
        }
    }
}
