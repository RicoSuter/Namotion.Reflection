using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    /// <summary>
    /// A method info with contextual information.
    /// </summary>
    public class ContextualMethodInfo : ContextualMemberInfo
    {
        internal ContextualMethodInfo(
            MethodInfo methodInfo,
            ContextualParameterInfo returnParameter,
            IEnumerable<ContextualParameterInfo> parameters)
        {
            MethodInfo = methodInfo;
            ReturnParameter = returnParameter;
            Parameters = parameters.ToArray();
        }

        /// <summary>
        /// Gets the type context's method info.
        /// </summary>
        public MethodInfo MethodInfo { get; }

        /// <summary>
        /// Gets the name of the cached method name.
        /// </summary>
        public override string Name => MethodInfo.Name;

        /// <summary>
        /// Gets the contextual parameters.
        /// </summary>
        public ContextualParameterInfo[] Parameters { get; }

        /// <summary>
        /// Gets the contextual return parameter.
        /// </summary>
        public ContextualParameterInfo ReturnParameter { get; }

        /// <inheritdoc />
        public override MemberInfo MemberInfo => MethodInfo;

        /// <inheritdoc />
        public override string ToString()
        {
            return Name + " (" + GetType().Name.Replace("Contextual", "").Replace("Info", "") + ") - " + base.ToString();
        }

        /// <inheritdoc />
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return MethodInfo.GetCustomAttributes(attributeType, inherit);
        }

        /// <inheritdoc />
        public override object[] GetCustomAttributes(bool inherit)
        {
            return MethodInfo.GetCustomAttributes(inherit);
        }

        /// <inheritdoc />
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return MethodInfo.IsDefined(attributeType, inherit);
        }
    }
}
