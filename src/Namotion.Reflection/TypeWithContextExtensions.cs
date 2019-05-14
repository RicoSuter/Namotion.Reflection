using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    public static class TypeWithContextExtensions
    {
        private static object Lock = new object();
        private static Dictionary<string, object> Cache = new Dictionary<string, object>();

        /// <summary>
        /// Gets an enumerable of <see cref="PropertyWithContext"/> and <see cref="FieldWithContext"/> for the given <see cref="Type"/> instance.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<MemberWithContext> GetPropertiesAndFieldsWithContext(this Type type)
        {
            return type.GetRuntimePropertiesWithContext()
                .OfType<MemberWithContext>()
                .Union(type.GetRuntimeFieldsWithContext());
        }

        /// <summary>
        /// Gets an array of <see cref="PropertyWithContext"/> for the given <see cref="Type"/> instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The runtime properties.</returns>
        public static PropertyWithContext[] GetRuntimePropertiesWithContext(this Type type)
        {
            var key = "Properties:" + type.FullName;

            if (!Cache.ContainsKey(key))
            {
                lock (Lock)
                {
                    if (!Cache.ContainsKey(key))
                    {
#if NET40
                        Cache[key] = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
#else
                        Cache[key] = type.GetRuntimeProperties()
#endif
                            .Select(p => p.GetPropertyWithContext()).ToArray();
                    }
                }
            }

            return (PropertyWithContext[])Cache[key];
        }

        /// <summary>
        /// Gets an array of <see cref="FieldWithContext"/> for the given <see cref="Type"/> instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The runtime fields.</returns>
        public static FieldWithContext[] GetRuntimeFieldsWithContext(this Type type)
        {
            var key = "Fields:" + type.FullName;
            if (!Cache.ContainsKey(key))
            {
                lock (Lock)
                {
                    if (!Cache.ContainsKey(key))
                    {
#if NET40
                        Cache[key] = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
#else
                        Cache[key] = type.GetRuntimeFields()
#endif
                            .Select(p => p.GetFieldWithContext()).ToArray();
                    }
                }
            }

            return (FieldWithContext[])Cache[key];
        }

        /// <summary>
        /// Gets a <see cref="ParameterWithContext"/> for the given <see cref="ParameterInfo"/> instance.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <returns>The <see cref="ParameterWithContext"/>.</returns>
        public static ParameterWithContext GetParameterWithContext(this ParameterInfo parameterInfo)
        {
            var key = "Parameter:" + parameterInfo.Name + ":" + parameterInfo.Member.DeclaringType.FullName;
            if (!Cache.ContainsKey(key))
            {
                lock (Lock)
                {
                    if (!Cache.ContainsKey(key))
                    {
                        var index = 0;
                        Cache[key] = new ParameterWithContext(parameterInfo, ref index);
                    }

                }
            }

            return (ParameterWithContext)Cache[key];
        }

        /// <summary>
        /// Gets a <see cref="PropertyWithContext"/> for the given <see cref="PropertyInfo"/> instance.
        /// </summary>
        /// <param name="propertyInfo">The property info.</param>
        /// <returns>The <see cref="PropertyWithContext"/>.</returns>
        public static PropertyWithContext GetPropertyWithContext(this PropertyInfo propertyInfo)
        {
            var key = "Property:" + propertyInfo.Name + ":" + propertyInfo.DeclaringType.FullName;
            if (!Cache.ContainsKey(key))
            {
                lock (Lock)
                {
                    if (!Cache.ContainsKey(key))
                    {
                        var index = 0;
                        Cache[key] = new PropertyWithContext(propertyInfo, ref index);
                    }

                    return (PropertyWithContext)Cache[key];
                }
            }

            return (PropertyWithContext)Cache[key];
        }

        /// <summary>
        /// Gets a <see cref="FieldWithContext"/> for the given <see cref="FieldInfo"/> instance.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <returns>The <see cref="FieldWithContext"/>.</returns>
        public static FieldWithContext GetFieldWithContext(this FieldInfo fieldInfo)
        {
            var key = "Field:" + fieldInfo.Name + ":" + fieldInfo.DeclaringType.FullName;
            if (!Cache.ContainsKey(key))
            {
                lock (Lock)
                {
                    if (!Cache.ContainsKey(key))
                    {
                        var index = 0;
                        Cache[key] = new FieldWithContext(fieldInfo, ref index);
                    }
                }
            }

            return (FieldWithContext)Cache[key];
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

        /// <summary>
        /// Gets a <see cref="TypeWithoutContext"/> for the given <see cref="Type"/> instance and attributes (uncached).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="attributes">The attributes.</param>
        /// <returns>The <see cref="TypeWithoutContext"/>.</returns>
        public static TypeWithContext GetTypeWithContext(this Type type, Attribute[] attributes)
        {
            // TODO: Cache this?
            return TypeWithContext.ForType(type, attributes);
        }

        /// <summary>
        /// Gets a <see cref="TypeWithoutContext"/> for the given <see cref="Type"/> instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The <see cref="TypeWithoutContext"/>.</returns>
        public static TypeWithContext GetTypeWithContext(this Type type)
        {
            var key = "Type:Context:" + type.FullName;
            lock (Lock)
            {
                if (!Cache.ContainsKey(key))
                {
                    Cache[key] = TypeWithContext.ForType(type, new Attribute[0]);
                }

                return (TypeWithContext)Cache[key];
            }
        }

        /// <summary>
        /// Gets a <see cref="TypeWithoutContext"/> for the given <see cref="Type"/> instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The <see cref="TypeWithoutContext"/>.</returns>
        public static TypeWithoutContext GetTypeWithoutContext(this Type type)
        {
            var key = "Type:" + type.FullName;
            if (!Cache.ContainsKey(key))
            {
                lock (Lock)
                {
                    if (!Cache.ContainsKey(key))
                    {
                        Cache[key] = new TypeWithoutContext(type);
                    }
                }
            }

            return (TypeWithoutContext)Cache[key];
        }
    }
}
