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
        private string? _name;

        internal ContextualParameterInfo(ParameterInfo parameterInfo, ref int nullableFlagsIndex, byte[]? nullableFlags)
            : base(parameterInfo.ParameterType, GetContextualAttributes(parameterInfo),
                null, ref nullableFlagsIndex, nullableFlags,
                parameterInfo.Member.DeclaringType.IsNested
                    ? new[] { NullableFlagsSource.Create(parameterInfo.Member), NullableFlagsSource.Create(parameterInfo.Member.DeclaringType), NullableFlagsSource.Create(parameterInfo.Member.DeclaringType.DeclaringType, parameterInfo.Member.DeclaringType.GetTypeInfo().Assembly) }
                    : new[] { NullableFlagsSource.Create(parameterInfo.Member), NullableFlagsSource.Create(parameterInfo.Member.DeclaringType, parameterInfo.Member.DeclaringType.GetTypeInfo().Assembly) })
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
                var attributes = parameterInfo.GetCustomAttributes(true);
#if !NETSTANDARD1_0
                if (attributes.Length == 0)
                {
                    return ArrayExt.Empty<Attribute>();
                }
#endif
                return attributes.OfType<Attribute>().ToArray();
            }
            catch
            {
                // Needed for legacy runtimes
                return parameterInfo.GetCustomAttributes(false).OfType<Attribute>().ToArray();
            }
        }
    }
}
