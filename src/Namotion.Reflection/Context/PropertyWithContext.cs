using System.Reflection;

namespace Namotion.Reflection
{
    public class PropertyWithContext : MemberWithContext
    {
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
        /// Gets the type context's member info.
        /// </summary>
        public override MemberInfo MemberInfo => PropertyInfo;

        public override object GetValue(object obj)
        {
            return PropertyInfo.GetValue(obj);
        }

        public override void SetValue(object obj, object value)
        {
            PropertyInfo.SetValue(obj, value);
        }
    }
}
