using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    public static class TypeWithContextExtensions
    {
        private static object _lock = new object();
        private static Dictionary<string, object> _cache = new Dictionary<string, object>();

        /// <summary>
        /// Gets an array of <see cref="PropertyWithContext"/> for the given <see cref="Type"/> instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The runtime properties.</returns>
        public static PropertyWithContext[] GetRuntimePropertiesWithContext(this Type type)
        {
            var key = "Properties:" + type.FullName;
            lock (_lock)
            {
                if (!_cache.ContainsKey(key))
                {
#if NET40
                    _cache[key] = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
#else
                    _cache[key] = type.GetRuntimeProperties()
#endif
                        .Select(p => p.GetPropertyWithContext()).ToArray();
                }

                return (PropertyWithContext[])_cache[key];
            }
        }

        /// <summary>
        /// Gets an array of <see cref="FieldWithContext"/> for the given <see cref="Type"/> instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The runtime fields.</returns>
        public static FieldWithContext[] GetRuntimeFieldsWithContext(this Type type)
        {
            var key = "Fields:" + type.FullName;
            lock (_lock)
            {
                if (!_cache.ContainsKey(key))
                {
#if NET40
                    _cache[key] = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
#else
                    _cache[key] = type.GetRuntimeFields()
#endif
                        .Select(p => p.GetFieldWithContext()).ToArray();
                }

                return (FieldWithContext[])_cache[key];
            }
        }

        /// <summary>
        /// Gets a <see cref="ParameterWithContext"/> for the given <see cref="ParameterInfo"/> instance.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <returns>The <see cref="ParameterWithContext"/>.</returns>
        public static ParameterWithContext GetParameterWithContext(this ParameterInfo parameterInfo)
        {
            var key = "Parameter:" + parameterInfo.Name + ":" + parameterInfo.Member.DeclaringType.FullName;
            lock (_lock)
            {
                if (!_cache.ContainsKey(key))
                {
                    var index = 0;
                    _cache[key] = new ParameterWithContext(parameterInfo, ref index);
                }

                return (ParameterWithContext)_cache[key];
            }
        }

        /// <summary>
        /// Gets a <see cref="PropertyWithContext"/> for the given <see cref="PropertyInfo"/> instance.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <returns>The <see cref="PropertyWithContext"/>.</returns>
        public static PropertyWithContext GetPropertyWithContext(this PropertyInfo propertyInfo)
        {
            var key = "Property:" + propertyInfo.Name + ":" + propertyInfo.DeclaringType.FullName;
            lock (_lock)
            {
                if (!_cache.ContainsKey(key))
                {
                    var index = 0;
                    _cache[key] = new PropertyWithContext(propertyInfo, ref index);
                }

                return (PropertyWithContext)_cache[key];
            }
        }

        /// <summary>
        /// Gets a <see cref="FieldWithContext"/> for the given <see cref="FieldInfo"/> instance.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <returns>The <see cref="FieldWithContext"/>.</returns>
        public static FieldWithContext GetFieldWithContext(this FieldInfo fieldInfo)
        {
            var key = "Field:" + fieldInfo.Name + ":" + fieldInfo.DeclaringType.FullName;
            lock (_lock)
            {
                if (!_cache.ContainsKey(key))
                {
                    var index = 0;
                    _cache[key] = new FieldWithContext(fieldInfo, ref index);
                }

                return (FieldWithContext)_cache[key];
            }
        }

        /// <summary>
        /// Gets a <see cref="TypeWithContext"/> for the given <see cref="MemberInfo"/> instance.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <returns>The <see cref="TypeWithContext"/>.</returns>
        public static TypeWithContext GetMemberWithContext(this MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo propertyInfo)
            {
                return propertyInfo.GetPropertyWithContext();
            }
            else if (memberInfo is FieldInfo fieldInfo)
            {
                return fieldInfo.GetFieldWithContext();
            }

            throw new NotSupportedException();
        }
    }
}
