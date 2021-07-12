using System;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    /// <summary>
    /// A field info with contextual information.
    /// </summary>
    public class ContextualFieldInfo : ContextualAccessorInfo
    {
        private string? _name;

        internal ContextualFieldInfo(FieldInfo fieldInfo, ref int nullableFlagsIndex, byte[]? nullableFlags)
        {
            FieldInfo = fieldInfo;
            FieldType = new ContextualType(
               fieldInfo.FieldType,
               fieldInfo.GetCustomAttributes(true).OfType<Attribute>().ToArray(),
               null,
               ref nullableFlagsIndex,
               nullableFlags,
               fieldInfo.DeclaringType.IsNested ?
                   new dynamic[] { fieldInfo.DeclaringType, fieldInfo.DeclaringType.DeclaringType, fieldInfo.DeclaringType.GetTypeInfo().Assembly } :
                   new dynamic[] { fieldInfo.DeclaringType, fieldInfo.DeclaringType.GetTypeInfo().Assembly });
        }

        /// <summary>
        /// Gets the type context's field info.
        /// </summary>
        public FieldInfo FieldInfo { get; }

        /// <summary>
        /// Gets the type context's member info.
        /// </summary>
        public override MemberInfo MemberInfo => FieldInfo;

        /// <inheritdoc />
        public override ContextualType AccessorType => FieldType;

        /// <summary>
        /// Gets the field's contextual type.
        /// </summary>
        public ContextualType FieldType { get; private set; }

        /// <summary>
        /// Gets the cached field name.
        /// </summary>
        public override string Name => _name ?? (_name = FieldInfo.Name);

        /// <summary>
        /// Returns the value of a field supported by a given object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The value.</returns>
        public override object? GetValue(object? obj)
        {
            return FieldInfo.GetValue(obj);
        }

        /// <summary>
        /// Sets the value of the field supported by the given object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public override void SetValue(object? obj, object? value)
        {
            FieldInfo.SetValue(obj, value);
        }
    }
}
