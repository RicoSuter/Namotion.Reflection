using System;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    public class FieldWithContext : TypeWithContext
    {
        internal FieldWithContext(FieldInfo fieldInfo, ref int nullableFlagsIndex)
            : base(fieldInfo.FieldType, fieldInfo.GetCustomAttributes(true).OfType<Attribute>().ToArray(), null, null, ref nullableFlagsIndex)
        {
            FieldInfo = fieldInfo;
        }

        /// <summary>
        /// Gets the type context's field info.
        /// </summary>
        public FieldInfo FieldInfo { get; }
    }
}
