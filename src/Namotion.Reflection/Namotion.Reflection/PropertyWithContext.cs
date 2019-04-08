using System;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    public class PropertyWithContext : TypeWithContext
    {
        internal PropertyWithContext(PropertyInfo propertyInfo, ref int nullableFlagsIndex)
            : base(propertyInfo.PropertyType, propertyInfo.GetCustomAttributes(true).OfType<Attribute>().ToArray(), null, null, ref nullableFlagsIndex)
        {
            PropertyInfo = propertyInfo;
        }

        /// <summary>
        /// Gets the type context's property info.
        /// </summary>
        public PropertyInfo PropertyInfo { get; }
    }
}
