using System;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    /// <summary>
    /// A member info with contextual information.
    /// </summary>
    public abstract class ContextualMemberInfo
    {
        /// <summary>
        /// Gets the type context's member info.
        /// </summary>
        public abstract MemberInfo MemberInfo { get; }

        /// <summary>
        /// Gets the name of the cached member name (property or parameter name).
        /// </summary>
        public abstract string Name { get; }

        /// <inheritdocs />
        public override string ToString()
        {
            return Name + " (" + GetType().Name.Replace("Contextual", "").Replace("Info", "") + ") - " + base.ToString();
        }
    }
}
