using System;
using System.Reflection;

namespace Namotion.Reflection
{
    /// <summary>
    /// A parameter info with contextual information.
    /// </summary>
    public class ContextualParameterInfo : ICustomAttributeProvider
    {
        private string? _name;

        internal ContextualParameterInfo(ParameterInfo parameterInfo, ref int nullableFlagsIndex, byte[]? nullableFlags)
        {
            ParameterInfo = parameterInfo;

            var attributeProviders = parameterInfo.Member.DeclaringType.IsNested
                    ? new[] { NullableFlagsSource.Create(parameterInfo.Member), NullableFlagsSource.Create(parameterInfo.Member.DeclaringType), NullableFlagsSource.Create(parameterInfo.Member.DeclaringType.DeclaringType, parameterInfo.Member.DeclaringType.GetTypeInfo().Assembly) }
                    : new[] { NullableFlagsSource.Create(parameterInfo.Member), NullableFlagsSource.Create(parameterInfo.Member.DeclaringType, parameterInfo.Member.DeclaringType.GetTypeInfo().Assembly) };

            ParameterType = new ContextualType(
                parameterInfo.ParameterType,
                this,
                null,
                ref nullableFlagsIndex,
                nullableFlags,
                attributeProviders);
        }

        /// <summary>
        /// Gets the type context's parameter info.
        /// </summary>
        public ParameterInfo ParameterInfo { get; }

        /// <summary>
        /// Gets the parameters contextual type.
        /// </summary>
        public ContextualType ParameterType { get; }

        /// <summary>
        /// Gets the nullability information of this parameter.
        /// </summary>
        public Nullability Nullability => ParameterType.Nullability;

        /// <summary>
        /// Gets the cached parameter name.
        /// </summary>
        public string Name => _name ?? (_name = ParameterInfo.Name);

        /// <inheritdoc />
        public override string ToString()
        {
            return Name + " (Parameter) - " + base.ToString();
        }

        /// <inheritdoc />
        public object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return ParameterInfo.GetCustomAttributes(attributeType, inherit);
        }

        /// <inheritdoc />
        public object[] GetCustomAttributes(bool inherit)
        {
            return ParameterInfo.GetCustomAttributes(inherit);
        }

        /// <inheritdoc />
        public bool IsDefined(Type attributeType, bool inherit)
        {
            return ParameterInfo.IsDefined(attributeType, inherit);
        }
    }
}
