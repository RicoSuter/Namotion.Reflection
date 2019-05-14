using System.Reflection;

namespace Namotion.Reflection
{
    public class PropertyWithContext : MemberWithContext
    {
        private string _name;

        internal PropertyWithContext(PropertyInfo propertyInfo, ref int nullableFlagsIndex)
            : base(propertyInfo, propertyInfo.PropertyType, ref nullableFlagsIndex)
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
        /// Returns the value of a field supported by a given object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The value.</returns>
        public override object GetValue(object obj)
        {
            return PropertyInfo.GetValue(obj);
        }

        /// <summary>
        /// Sets the value of the field supported by the given object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public override void SetValue(object obj, object value)
        {
            PropertyInfo.SetValue(obj, value);
        }
    }
}
