using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    /// <summary>
    /// Provides attribute extensions.
    /// </summary>
    public static class AttributeExtensions
    {
        /// <summary>
        /// Gets an attribute of the given type which is defined on the context or on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attribute or null.</returns>
        public static T? GetContextOrTypeAttribute<T>(this ContextualType contextualType, bool inherit)
        {
            return (T?)contextualType.Context.GetCustomAttributes(typeof(T), inherit).SingleOrDefault() ??
                (T?)contextualType.GetCustomAttributes(typeof(T), inherit).SingleOrDefault();
        }

        /// <summary>
        /// Gets the attributes of the given type which are defined on the context or on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attributes.</returns>
        public static IEnumerable<T> GetContextOrTypeAttributes<T>(this ContextualType contextualType, bool inherit)
        {
            return contextualType.Context.GetCustomAttributes(typeof(T), inherit)
                .Concat(contextualType.GetCustomAttributes(typeof(T), inherit))
                .OfType<T>();
        }

        /// <summary>
        /// Gets the attributes of the given type which are defined on the context or on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attributes.</returns>
        public static IEnumerable<Attribute> GetContextOrTypeAttributes(this ContextualType contextualType, bool inherit)
        {
            return contextualType.GetContextOrTypeAttributes<Attribute>(inherit);
        }


        /// <summary>
        /// Gets an attribute of the given type which is defined on the context or on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attribute or null.</returns>
        public static T? GetContextAttribute<T>(this ContextualType contextualType, bool inherit)
        {
            return (T?)contextualType.Context.GetCustomAttributes(typeof(T), inherit).SingleOrDefault();
        }

        /// <summary>
        /// Gets the attributes of the given type which are defined on the context or on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attributes.</returns>
        public static IEnumerable<T> GetContextAttributes<T>(this ContextualType contextualType, bool inherit)
        {
            return contextualType.Context.GetCustomAttributes(typeof(T), inherit).OfType<T>();
        }

        /// <summary>
        /// Gets the attributes of the given type which are defined on the context or on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attributes.</returns>
        public static IEnumerable<Attribute> GetContextAttributes(this ContextualType contextualType, bool inherit)
        {
            return contextualType.GetContextAttributes<Attribute>(inherit);
        }

        /// <summary>
        /// Gets the attributes of the given type which are defined on the context or on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attributes.</returns>
        public static bool IsContextAttributeDefined<T>(this ContextualType contextualType, bool inherit)
        {
            return contextualType.Context.IsDefined(typeof(T), inherit);
        }


        /// <summary>
        /// Gets an attribute of the given type which is defined on the context or on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attribute or null.</returns>
        public static T? GetAttribute<T>(this ContextualMemberInfo info, bool inherit)
        {
            return ((ICustomAttributeProvider)info).GetAttribute<T>(inherit);
        }

        /// <summary>
        /// Gets the attributes of the given type which are defined on the context or on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attributes.</returns>
        public static IEnumerable<T> GetAttributes<T>(this ContextualMemberInfo info, bool inherit)
        {
            return ((ICustomAttributeProvider)info).GetAttributes<T>(inherit);
        }

        /// <summary>
        /// Gets the attributes of the given type which are defined on the context or on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attributes.</returns>
        public static IEnumerable<Attribute> GetAttributes(this ContextualMemberInfo info, bool inherit)
        {
            return ((ICustomAttributeProvider)info).GetAttributes(inherit);
        }

        /// <summary>
        /// Gets the attributes of the given type which are defined on the context or on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attributes.</returns>
        public static bool IsAttributeDefined<T>(this ContextualMemberInfo info, bool inherit)
        {
            return ((ICustomAttributeProvider)info).IsAttributeDefined<T>(inherit);
        }


        /// <summary>
        /// Gets an attribute of the given type which is defined on the context or on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attribute or null.</returns>
        public static T? GetAttribute<T>(this ContextualParameterInfo info, bool inherit)
        {
            return ((ICustomAttributeProvider)info).GetAttribute<T>(inherit);
        }

        /// <summary>
        /// Gets the attributes of the given type which are defined on the context or on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attributes.</returns>
        public static IEnumerable<T> GetAttributes<T>(this ContextualParameterInfo info, bool inherit)
        {
            return ((ICustomAttributeProvider)info).GetAttributes<T>(inherit);
        }

        /// <summary>
        /// Gets the attributes of the given type which are defined on the context or on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attributes.</returns>
        public static IEnumerable<Attribute> GetAttributes(this ContextualParameterInfo info, bool inherit)
        {
            return ((ICustomAttributeProvider)info).GetAttributes(inherit);
        }

        /// <summary>
        /// Gets the attributes of the given type which are defined on the context or on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attributes.</returns>
        public static bool IsAttributeDefined<T>(this ContextualParameterInfo info, bool inherit)
        {
            return ((ICustomAttributeProvider)info).IsAttributeDefined<T>(inherit);
        }


        /// <summary>
        /// Gets an attribute of the given type which is defined on the context or on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attribute or null.</returns>
        public static T? GetAttribute<T>(this CachedType info, bool inherit)
        {
            return ((ICustomAttributeProvider)info).GetAttribute<T>(inherit);
        }

        /// <summary>
        /// Gets the attributes of the given type which are defined on the context or on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attributes.</returns>
        public static IEnumerable<T> GetAttributes<T>(this CachedType info, bool inherit)
        {
            return ((ICustomAttributeProvider)info).GetAttributes<T>(inherit);
        }

        /// <summary>
        /// Gets the attributes of the given type which are defined on the context or on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attributes.</returns>
        public static IEnumerable<Attribute> GetAttributes(this CachedType info, bool inherit)
        {
            return ((ICustomAttributeProvider)info).GetAttributes(inherit);
        }

        /// <summary>
        /// Gets the attributes of the given type which are defined on the context or on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attributes.</returns>
        public static bool IsAttributeDefined<T>(this CachedType info, bool inherit)
        {
            return ((ICustomAttributeProvider)info).IsAttributeDefined<T>(inherit);
        }


        internal static T? GetAttribute<T>(this ICustomAttributeProvider provider, bool inherit)
        {
            return (T?)provider.GetCustomAttributes(typeof(T), inherit).SingleOrDefault();
        }

        internal static IEnumerable<T> GetAttributes<T>(this ICustomAttributeProvider provider, bool inherit)
        {
            return provider.GetCustomAttributes(typeof(T), inherit).OfType<T>();
        }

        internal static IEnumerable<Attribute> GetAttributes(this ICustomAttributeProvider provider, bool inherit)
        {
            return provider.GetAttributes<Attribute>(inherit);
        }

        internal static bool IsAttributeDefined<T>(this ICustomAttributeProvider provider, bool inherit)
        {
            return provider.IsDefined(typeof(T), inherit);
        }
    }
}
