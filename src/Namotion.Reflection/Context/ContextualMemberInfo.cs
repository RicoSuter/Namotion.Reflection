using System;
using System.Reflection;

namespace Namotion.Reflection
{
    /// <summary>
    /// A member info with contextual information.
    /// </summary>
    public abstract class ContextualMemberInfo : ICustomAttributeProvider
    {
        /// <summary>
        /// Gets the type context's member info.
        /// </summary>
        public abstract MemberInfo MemberInfo { get; }

        /// <summary>
        /// Gets the name of the cached member name (property or parameter name).
        /// </summary>
        public abstract string Name { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name + " (" + GetType().Name.Replace("Contextual", "").Replace("Info", "") + ") - " + base.ToString();
        }

        /// <inheritdoc />
        public abstract object[] GetCustomAttributes(Type attributeType, bool inherit);

        /// <inheritdoc />
        public abstract object[] GetCustomAttributes(bool inherit);

        /// <inheritdoc />
        public abstract bool IsDefined(Type attributeType, bool inherit);
    }
}
