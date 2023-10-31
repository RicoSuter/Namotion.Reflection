using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    /// <summary>
    /// Helper to support old runtimes.
    /// </summary>
    internal static class ArrayExt
    {
        private static class EmptyHolder<T>
        {
            internal static readonly T[] _empty = new T[0];
        }

        public static T[] Empty<T>() => EmptyHolder<T>._empty;
    }

    /// <summary>
    /// Type and member extension methods to extract contextual or cached types.
    /// </summary>
    public static class ContextualTypeExtensions
    {
        private readonly record struct CacheKey(string Prefix, string Key1, string? Key2 = null, string? Key3 = null, string? Key4 = null);
        private static readonly ConcurrentDictionary<CacheKey, object> Cache = new();

        internal static void ClearCache()
        {
            Cache.Clear();
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
                return ContextualType.ForType(type, null);
            }

            var key = new CacheKey("Type:Context", type.FullName);
            return (ContextualType) Cache.GetOrAdd(key, k => ContextualType.ForType(type, null));
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

            var key = new CacheKey("Type", type.FullName);
            return (CachedType) Cache.GetOrAdd(key, k => new CachedType(type));
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
            return ContextualType.ForType(type, new GenericTypeContext(attributes));
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
            var key = new CacheKey("Parameter", parameterInfo.Name, parameterInfo.ParameterType.FullName, parameterInfo.Member.Name, parameterInfo.Member.DeclaringType.FullName);
            return (ContextualParameterInfo) Cache.GetOrAdd(key, k =>
            {
                var index = 0;
                return new ContextualParameterInfo(parameterInfo, ref index, null);
            });
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
            var key = new CacheKey("Property",propertyInfo.Name, propertyInfo.DeclaringType.FullName);
            return (ContextualPropertyInfo) Cache.GetOrAdd(key, k =>
            {
                var index = 0;
                return new ContextualPropertyInfo(propertyInfo, nullableFlagsIndex: ref index, nullableFlags: null);
            });
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
            var key = new CacheKey("Field", fieldInfo.Name, fieldInfo.DeclaringType.FullName);
            return (ContextualFieldInfo) Cache.GetOrAdd(key, k =>
            {
                var index = 0;
                return new ContextualFieldInfo(fieldInfo, nullableFlagsIndex: ref index, nullableFlags: null);
            });
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
