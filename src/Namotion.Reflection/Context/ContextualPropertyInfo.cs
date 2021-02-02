using System.Reflection;
using System.Runtime.CompilerServices;

namespace Namotion.Reflection
{
    /// <summary>
    /// A property info with contextual information.
    /// </summary>
    public class ContextualPropertyInfo : ContextualMemberInfo
    {
        private string? _name;
        private bool? _isValueType;
        private bool? _canWrite;
        private bool? _canRead;

        internal ContextualPropertyInfo(PropertyInfo propertyInfo, ref int nullableFlagsIndex, byte[]? nullableFlags = null)
            : base(propertyInfo, propertyInfo.PropertyType, ref nullableFlagsIndex, nullableFlags)
        {
            PropertyInfo = propertyInfo;
        }

        /// <summary>
        /// Gets the type context's property info.
        /// </summary>
        public PropertyInfo PropertyInfo { get; }

        /// <summary>
        /// Gets the cached field name.
        /// </summary>
        public override string Name => _name ?? (_name = PropertyInfo.Name);

        /// <summary>
        /// Gets the type context's member info.
        /// </summary>
        public override MemberInfo MemberInfo => PropertyInfo;

        /// <summary>
        /// Gets a value indicating whether the System.Type is a value type.
        /// </summary>
        public bool IsValueType => _isValueType ?? ((bool)(_isValueType = TypeInfo.IsValueType));

        /// <summary>
        /// Gets a value indicating whether the property can be written to.
        /// </summary>
        public bool CanWrite => _canWrite ?? ((bool)(_canWrite = PropertyInfo.CanWrite));

        /// <summary>
        /// Gets a value indicating whether the property can be read from.
        /// </summary>
        public bool CanRead => _canRead ?? ((bool)(_canRead = PropertyInfo.CanRead));

        /// <summary>
        /// Returns the value of a field supported by a given object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The value.</returns>
#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override object? GetValue(object? obj)
        {
            if (_propertyReader == null)
            {
                lock (this)
                {
                    if (_propertyReader == null)
                    {
                        _propertyReader = PropertyReader.Create(PropertyInfo.DeclaringType, OriginalType, PropertyInfo);
                    }
                }
            }

            return _propertyReader.GetValue(obj);
        }

        private IPropertyReader? _propertyReader;

        /// <summary>
        /// Sets the value of the field supported by the given object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override void SetValue(object? obj, object? value)
        {
            if (_propertyWriter == null)
            {
                lock (this)
                {
                    if (_propertyWriter == null)
                    {
                        _propertyWriter = PropertyWriter.Create(PropertyInfo.DeclaringType, OriginalType, PropertyInfo);
                    }
                }
            }

            _propertyWriter.SetValue(obj, value);
        }

        private IPropertyWriter? _propertyWriter;
    }
}
