using System;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    public class ParameterWithContext : TypeWithContext
    {
        private string _name;

        internal ParameterWithContext(ParameterInfo parameterInfo, ref int nullableFlagsIndex)
            : base(parameterInfo.ParameterType, parameterInfo.GetCustomAttributes(true).OfType<Attribute>().ToArray(), null, null, ref nullableFlagsIndex)
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
    }
}
