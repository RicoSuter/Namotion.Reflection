//-----------------------------------------------------------------------
// <copyright file="ReflectionExtensions.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    /// <summary>Provides extension methods for reflection.</summary>
    public static class TypeExtensions
    {
        /// <summary>Checks whether the given type is assignable to the given type name.</summary>
        /// <param name="type">The type.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="typeNameStyle">The type name style.</param>
        /// <returns></returns>
        public static bool IsAssignableTo(this Type type, string typeName, TypeNameStyle typeNameStyle)
        {
            if (typeNameStyle == TypeNameStyle.Name && type.Name == typeName)
                return true;

            if (typeNameStyle == TypeNameStyle.FullName && type.FullName == typeName)
                return true;

            return type.InheritsFrom(typeName, typeNameStyle);
        }

        /// <summary>Checks whether the given type inherits from the given type name.</summary>
        /// <param name="type">The type.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="typeNameStyle">The type name style.</param>
        /// <returns>true if the type inherits from typeName.</returns>
        public static bool InheritsFrom(this Type type, string typeName, TypeNameStyle typeNameStyle)
        {
            var baseType = type.GetTypeInfo().BaseType;
            while (baseType != null)
            {
                if (typeNameStyle == TypeNameStyle.Name && baseType.Name == typeName)
                    return true;
                if (typeNameStyle == TypeNameStyle.FullName && baseType.FullName == typeName)
                    return true;

                baseType = baseType.GetTypeInfo().BaseType;
            }
            return false;
        }

        /// <summary>Gets the type of the array item.</summary>
        public static Type GetEnumerableItemType(this Type type)
        {
            var genericTypeArguments = type.GetGenericTypeArguments();

            var itemType = genericTypeArguments.Length == 0 ? type.GetElementType() : genericTypeArguments[0];
            if (itemType == null)
            {
#if !NET40
                foreach (var iface in type.GetTypeInfo().ImplementedInterfaces)
#else
                foreach (var iface in type.GetTypeInfo().GetInterfaces())
#endif
                {
                    if (typeof(IEnumerable).GetTypeInfo()
                        .IsAssignableFrom(iface.GetTypeInfo()))
                    {
                        itemType = iface.GetEnumerableItemType();
                        if (itemType != null)
                            return itemType;
                    }
                }
            }
            return itemType;
        }

        /// <summary>Gets the generic type arguments of a type.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The type arguments.</returns>
        public static Type[] GetGenericTypeArguments(this Type type)
        {
            // TODO: Rename method

#if !NET40

            var genericTypeArguments = type.GenericTypeArguments;
            while (type != null && type != typeof(object) && genericTypeArguments.Length == 0)
            {
                type = type.GetTypeInfo().BaseType;
                if (type != null)
                    genericTypeArguments = type.GenericTypeArguments;
            }

            return genericTypeArguments;

#else

            var genericTypeArguments = type.GetGenericArguments();
            while (type != null && type != typeof(object) && genericTypeArguments.Length == 0)
            {
                type = type.GetTypeInfo().BaseType;
                if (type != null)
                    genericTypeArguments = type.GetGenericArguments();
            }

            return genericTypeArguments;

#endif
        }

        /// <summary>Gets a human readable type name (e.g. DictionaryOfStringAndObject).</summary>
        /// <param name="type">The type.</param>
        /// <returns>The type name.</returns>
        public static string GetSafeTypeName(this Type type)
        {
#if !NET40
            if (type.IsConstructedGenericType)
                return type.Name.Split('`').First() + "Of" + string.Join("And", type.GenericTypeArguments.Select(GetSafeTypeName));
#else
            if (type.IsGenericType)
                return type.Name.Split('`').First() + "Of" + string.Join("And", type.GetGenericArguments().Select(GetSafeTypeName));
#endif

            return type.Name;
        }
    }
}