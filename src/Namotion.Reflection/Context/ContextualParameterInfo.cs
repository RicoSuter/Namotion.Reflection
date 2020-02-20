using System;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    /// <summary>
    /// A parameter info with contextual information.
    /// </summary>
    public class ContextualParameterInfo : ContextualType
    {
        private string _name;

        internal ContextualParameterInfo(ParameterInfo parameterInfo, ref int nullableFlagsIndex)
            : base(parameterInfo.ParameterType, GetContextualAttributes(parameterInfo),
                null, null, ref nullableFlagsIndex,
                parameterInfo.Member.DeclaringType.IsNested ?
                    new dynamic[] { parameterInfo.Member, parameterInfo.Member.DeclaringType, parameterInfo.Member.DeclaringType.DeclaringType, parameterInfo.Member.DeclaringType.GetTypeInfo().Assembly } :
                    new dynamic[] { parameterInfo.Member, parameterInfo.Member.DeclaringType, parameterInfo.Member.DeclaringType.GetTypeInfo().Assembly })
        {
            ParameterInfo = parameterInfo;
        }

        /// <summary>
        /// Gets the type context's parameter info.
        /// </summary>
        public ParameterInfo ParameterInfo { get; }

        /// <summary>
        /// Gets the cached parameter name.
        /// </summary>
        public string Name => _name ?? (_name = ParameterInfo.Name);

        /// <inheritdocs />
        public override string ToString()
        {
            return Name + " (Parameter) - " + base.ToString();
        }

        private static Attribute[] GetContextualAttributes(ParameterInfo parameterInfo)
        {
            try
            {
                return parameterInfo.GetCustomAttributes(true).OfType<Attribute>().ToArray();
            }
            catch
            {
                // Needed for legacy runtimes
                return parameterInfo.GetCustomAttributes(false).OfType<Attribute>().ToArray();
            }
        }
    }
}
