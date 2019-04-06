using System;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    public static class TypeWithContextExtensions
    {
        /// <summary>
        /// Gets a type with context for the given <see cref="ParameterInfo"/> instance.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <returns>The type with context.</returns>
        public static TypeWithContext GetTypeWithContext(this ParameterInfo parameterInfo)
        {
            var attributes = parameterInfo.GetCustomAttributes(true).OfType<Attribute>().ToArray();
            var type = TypeWithContext.ForType(parameterInfo.ParameterType, attributes);
            type.ParemterInfo = parameterInfo;
            return type;
        }

        /// <summary>
        /// Gets a type with context for the given <see cref="MemberInfo"/> instance.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <returns>The type with context.</returns>
        public static TypeWithContext GetTypeWithContext(this MemberInfo memberInfo)
        {
            var attributes = memberInfo.GetCustomAttributes(true).OfType<Attribute>().ToArray();

            if (memberInfo is PropertyInfo propertyInfo)
            {
                var type = TypeWithContext.ForType(propertyInfo.PropertyType, attributes);
                type.PropertyInfo = propertyInfo;
                return type;
            }
            else if (memberInfo is FieldInfo fieldInfo)
            {
                var type = TypeWithContext.ForType(fieldInfo.FieldType, attributes);
                type.FieldInfo = fieldInfo;
                return type;
            }

            throw new NotSupportedException();
        }
    }
}
