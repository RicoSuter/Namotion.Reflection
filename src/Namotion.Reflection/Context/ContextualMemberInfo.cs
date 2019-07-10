using System;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    /// <summary>
    /// A member info with contextual information.
    /// </summary>
    public abstract class ContextualMemberInfo : ContextualType
    {
        internal ContextualMemberInfo(MemberInfo memberInfo, Type memberType, ref int nullableFlagsIndex)
            : base(memberType,
                memberInfo.GetCustomAttributes(true).OfType<Attribute>().ToArray(),
                null, null, ref nullableFlagsIndex,
                new dynamic[] { memberInfo.DeclaringType, memberInfo.DeclaringType.GetTypeInfo().Assembly })
        {
        }

        /// <summary>
        /// Gets the type context's member info.
        /// </summary>
        public abstract MemberInfo MemberInfo { get; }

        /// <summary>
        /// Gets the name of the cached member name (property or parameter name).
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Returns the value of a field supported by a given object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The value.</returns>
        public abstract object GetValue(object obj);

        /// <summary>
        /// Sets the value of the field supported by the given object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public abstract void SetValue(object obj, object value);

        /// <inheritdocs />
        public override string ToString()
        {
            return Name + " (" + GetType().Name.Replace("Contextual", "").Replace("Info", "") + ") - " + base.ToString();
        }
    }
}
