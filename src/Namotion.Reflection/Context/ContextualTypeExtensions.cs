using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    /// <summary>
    /// Type and member extension methods to extract contextual or cached types.
    /// </summary>
    public static class ContextualTypeExtensions
    {
        private static readonly object Lock = new object();
        private static readonly Dictionary<string, object> Cache = new Dictionary<string, object>();

        internal static void ClearCache()
        {
            lock (Lock)
            {
                Cache.Clear();
            }
        }

        /// <summary>
        /// Gets a <see cref="CachedType"/> for the given <see cref="Type"/> instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The <see cref="CachedType"/>.</returns>
        public static ContextualType ToContextualType(this Type type)
        {
            if (type.FullName == null)
            {
                return ContextualType.ForType(type, new Attribute[0]);
            }

            var key = "Type:Context:" + type.FullName;
            lock (Lock)
            {
                if (!Cache.ContainsKey(key))
                {
                    Cache[key] = ContextualType.ForType(type, new Attribute[0]);
                }

                return (ContextualType)Cache[key];
            }
        }

        /// <summary>
        /// Gets a <see cref="CachedType"/> for the given <see cref="Type"/> instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The <see cref="CachedType"/>.</returns>
        public static CachedType ToCachedType(this Type type)
        {
            if (type.FullName == null)
            {
                // Returns an uncached version of the type since we can't build a cache key
                return new CachedType(type);
            }

            var key = "Type:" + type.FullName;
            if (!Cache.ContainsKey(key))
            {
                lock (Lock)
                {
                    if (!Cache.ContainsKey(key))
                    {
                        Cache[key] = new CachedType(type);
                    }
                }
            }

            return (CachedType)Cache[key];
        }

        /// <summary>
        /// Gets an enumerable of <see cref="ContextualAccessorInfo"/>s (all properties and fields) for the given <see cref="Type"/> instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static IEnumerable<ContextualAccessorInfo> GetContextualAccessors(this Type type)
        {
            var contextualType = type.ToContextualType();
            return contextualType.Fields.OfType<ContextualAccessorInfo>()
                .Concat(contextualType.Properties);
        }

        /// <summary>
        /// Gets an array of <see cref="ContextualPropertyInfo"/> for the given <see cref="Type"/> instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The runtime properties.</returns>
        public static ContextualPropertyInfo[] GetContextualProperties(this Type type)
        {
            return type.ToContextualType().Properties;
        }

        /// <summary>
        /// Gets an array of <see cref="ContextualFieldInfo"/> for the given <see cref="Type"/> instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The runtime fields.</returns>
        public static ContextualFieldInfo[] GetContextualFields(this Type type)
        {
            return type.ToContextualType().Fields;
        }

        /// <summary>
        /// Gets an uncached <see cref="ContextualType"/> for the given <see cref="Type"/> instance and attributes.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="attributes">The attributes.</param>
        /// <returns>The <see cref="CachedType"/>.</returns>
        public static ContextualType ToContextualType(this Type type, IEnumerable<Attribute> attributes)
        {
            return ContextualType.ForType(type, attributes);
        }

        /// <summary>
        /// Gets an array of <see cref="ContextualParameterInfo"/> for the given <see cref="MethodBase"/> instance.
        /// Warning: Retrieving contextual information directly from <see cref="MethodBase"/> might lose original context data (NRT on original generic type parameters).
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>The runtime properties.</returns>
        [Obsolete("Remove usages in code and then obsolete attribute when PR is ready.")]
        public static ContextualParameterInfo[] GetContextualParameters(this MethodBase method)
        {
            // TODO: Remove usages of this method in code (might lose context)!!
            var key = "Parameters:" + method.Name + ":" + method.DeclaringType.FullName + ":" + EnumeratedParameters(method.GetParameters());
            if (!Cache.ContainsKey(key))
            {
                lock (Lock)
                {
                    if (!Cache.ContainsKey(key))
                    {
                        Cache[key] = method.GetParameters()
                            .Select(p => p.ToContextualParameter())
                            .ToArray();
                    }
                }
            }

            return (ContextualParameterInfo[])Cache[key];
        }

        private static string EnumeratedParameters(ParameterInfo[] parameters)
        {
            return string.Join("-", parameters.Select(p => p.ParameterType.FullName));
        }

        /// <summary>
        /// Gets a <see cref="ContextualParameterInfo"/> for the given <see cref="ParameterInfo"/> instance.
        /// Warning: Retrieving contextual information directly from <see cref="ParameterInfo"/> might lose original context data (NRT on original generic type parameters).
        /// </summary>
        /// <param name="parameterInfo">The parameter info.</param>
        /// <returns>The <see cref="ContextualParameterInfo"/>.</returns>
        public static ContextualParameterInfo ToContextualParameter(this ParameterInfo parameterInfo)
        {
            // TODO: Remove usages of this method in code (might lose context)!!
            var key = "Parameter:" + parameterInfo.Name + ":" + parameterInfo.ParameterType.FullName + ":" + parameterInfo.Member.Name + ":" + parameterInfo.Member.DeclaringType.FullName;
            if (!Cache.ContainsKey(key))
            {
                lock (Lock)
                {
                    if (!Cache.ContainsKey(key))
                    {
                        var index = 0;
                        Cache[key] = new ContextualParameterInfo(parameterInfo, ref index);
                    }

                }
            }

            return (ContextualParameterInfo)Cache[key];
        }

        /// <summary>
        /// Gets a <see cref="ContextualPropertyInfo"/> for the given <see cref="PropertyInfo"/> instance.
        /// Warning: Retrieving contextual information directly from <see cref="PropertyInfo"/> might lose original context data (NRT on original generic type parameters).
        /// </summary>
        /// <param name="propertyInfo">The property info.</param>
        /// <returns>The <see cref="ContextualPropertyInfo"/>.</returns>
        public static ContextualPropertyInfo ToContextualProperty(this PropertyInfo propertyInfo)
        {
            // TODO: Remove usages of this method in code (might lose context)!!
            var key = "Property:" + propertyInfo.Name + ":" + propertyInfo.DeclaringType.FullName;
            if (!Cache.ContainsKey(key))
            {
                lock (Lock)
                {
                    if (!Cache.ContainsKey(key))
                    {
                        var index = 0;
                        Cache[key] = new ContextualPropertyInfo(propertyInfo, nullableFlagsIndex: ref index, nullableFlags: null);
                    }

                    return (ContextualPropertyInfo)Cache[key];
                }
            }

            return (ContextualPropertyInfo)Cache[key];
        }

        /// <summary>
        /// Gets a <see cref="ContextualFieldInfo"/> for the given <see cref="FieldInfo"/> instance.
        /// Warning: Retrieving contextual information directly from <see cref="FieldInfo"/> might lose original context data (NRT on original generic type parameters).
        /// </summary>
        /// <param name="fieldInfo">The field info.</param>
        /// <returns>The <see cref="ContextualFieldInfo"/>.</returns>
        public static ContextualFieldInfo ToContextualField(this FieldInfo fieldInfo)
        {
            // TODO: Remove usages of this method in code (might lose context)!!
            var key = "Field:" + fieldInfo.Name + ":" + fieldInfo.DeclaringType.FullName;
            if (!Cache.ContainsKey(key))
            {
                lock (Lock)
                {
                    if (!Cache.ContainsKey(key))
                    {
                        var index = 0;
                        Cache[key] = new ContextualFieldInfo(fieldInfo, nullableFlagsIndex: ref index, nullableFlags: null);
                    }
                }
            }

            return (ContextualFieldInfo)Cache[key];
        }

        /// <summary>
        /// Gets a <see cref="ContextualMemberInfo"/> for the given <see cref="MemberInfo"/> instance.
        /// Warning: Retrieving contextual information directly from <see cref="MemberInfo"/> might lose original context data (NRT on original generic type parameters).
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <returns>The <see cref="ContextualMemberInfo"/>.</returns>
        public static ContextualAccessorInfo ToContextualAccessor(this MemberInfo memberInfo)
        {
            // TODO: Remove usages of this method in code (might lose context)!!
            if (memberInfo is PropertyInfo propertyInfo)
            {
                return propertyInfo.ToContextualProperty();
            }
            else if (memberInfo is FieldInfo fieldInfo)
            {
                return fieldInfo.ToContextualField();
            }

            throw new NotSupportedException("The member info must be a field or property.");
        }
    }
}
