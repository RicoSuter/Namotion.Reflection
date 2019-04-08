using System;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    public abstract class MemberWithContext : TypeWithContext
    {
        internal MemberWithContext(MemberInfo memberInfo, Type memberType, ref int nullableFlagsIndex) 
            : base(memberType, memberInfo.GetCustomAttributes(true).OfType<Attribute>().ToArray(), null, null, ref nullableFlagsIndex)
        {
        }

        /// <summary>
        /// Gets the type context's member info.
        /// </summary>
        public abstract MemberInfo MemberInfo { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public abstract object GetValue(object obj);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public abstract void SetValue(object obj, object value);
    }
}
