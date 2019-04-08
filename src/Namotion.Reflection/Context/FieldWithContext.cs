using System.Reflection;

namespace Namotion.Reflection
{
    public class FieldWithContext : MemberWithContext
    {
        internal FieldWithContext(FieldInfo fieldInfo, ref int nullableFlagsIndex)
            : base(fieldInfo, fieldInfo.FieldType, ref nullableFlagsIndex)
        {
            FieldInfo = fieldInfo;
        }

        /// <summary>
        /// Gets the type context's field info.
        /// </summary>
        public FieldInfo FieldInfo { get; }

        /// <summary>
        /// Gets the type context's member info.
        /// </summary>
        public override MemberInfo MemberInfo => FieldInfo;

        public override object GetValue(object obj)
        {
            return FieldInfo.GetValue(obj);
        }

        public override void SetValue(object obj, object value)
        {
            FieldInfo.SetValue(obj, value);
        }
    }
}
