//-----------------------------------------------------------------------
// <copyright file="ReflectionExtensions.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
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
        public static bool IsAssignableToTypeName(this CachedType type, string typeName, TypeNameStyle typeNameStyle)
        {
            return type.OriginalType.IsAssignableToTypeName(typeName, typeNameStyle);
        }

        /// <summary>Checks whether the given type is assignable to the given type name.</summary>
        /// <param name="type">The type.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="typeNameStyle">The type name style.</param>
        /// <returns></returns>
        public static bool IsAssignableToTypeName(this Type type, string typeName, TypeNameStyle typeNameStyle)
        {
            if (typeNameStyle == TypeNameStyle.Name && type.Name == typeName)
            {
                return true;
            }

            if (typeNameStyle == TypeNameStyle.FullName && type.FullName == typeName)
            {
                return true;
            }

            if (type.InheritsFromTypeName(typeName, typeNameStyle))
            {
                return true;
            }

#if NETSTANDARD1_0
            var interfaces = type.GetTypeInfo().ImplementedInterfaces;
#else
            var interfaces = type.GetInterfaces();
#endif
            foreach (var i in interfaces)
            {
                if (typeNameStyle == TypeNameStyle.Name && i.Name == typeName
                    || typeNameStyle == TypeNameStyle.FullName && i.FullName == typeName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Checks whether the given type inherits from the given type name.</summary>
        /// <param name="type">The type.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="typeNameStyle">The type name style.</param>
        /// <returns>true if the type inherits from typeName.</returns>
        public static bool InheritsFromTypeName(this Type type, string typeName, TypeNameStyle typeNameStyle)
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
        public static Type? GetEnumerableItemType(this Type type)
        {
            var elementType = type.GetElementType();
            if (elementType != null)
            {
                return elementType;
            }

            var getEnumeratorMethod = type.ToContextualType().Methods.SingleOrDefault(m => m.Name == "GetEnumerator");
            if (getEnumeratorMethod != null)
            {
                var genericTypeArguments = type.GetGenericTypeArgumentsOfTypeOrBaseTypes();
                if (genericTypeArguments?.Length == 1)
                {
                    return genericTypeArguments[0];
                }

                var returnParam = getEnumeratorMethod.ReturnParameter;
                if (returnParam?.GenericArguments.Length == 1)
                {
                    return returnParam.GenericArguments[0];
                }
            }

            return null;
        }

        /// <summary>Gets the generic type arguments of a type or its base type.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The type arguments.</returns>
        private static Type[] GetGenericTypeArgumentsOfTypeOrBaseTypes(this Type type)
        {
#if !NET40

            var genericTypeArguments = type.GenericTypeArguments;
            while (type != null && type != typeof(object) && genericTypeArguments.Length == 0)
            {
                type = type.GetTypeInfo().BaseType;
                if (type != null)
                {
                    genericTypeArguments = type.GenericTypeArguments;
                }
            }

            return genericTypeArguments;

#else

            var genericTypeArguments = type.GetGenericArguments();
            while (type != null && type != typeof(object) && genericTypeArguments.Length == 0)
            {
                type = type.GetTypeInfo().BaseType;
                if (type != null)
                {
                    genericTypeArguments = type.GetGenericArguments();
                }
            }

            return genericTypeArguments;

#endif
        }

        /// <summary>Gets a human readable type name (e.g. DictionaryOfStringAndObject).</summary>
        /// <param name="type">The type.</param>
        /// <returns>The type name.</returns>
        public static string GetDisplayName(this Type type)
        {
            var nType = type.ToCachedType();

#if !NET40
            if (nType.Type.IsConstructedGenericType)
#else
            if (nType.Type.IsGenericType)
#endif
            {
                return GetName(nType).Split('`').First() + "Of" +
                       string.Join("And", nType.GenericArguments
                                               .Select(a => GetDisplayName(a.OriginalType)));
            }

            return GetName(nType);
        }

        private static string GetName(CachedType cType)
        {
            return
                cType.TypeName == "Int16" ? GetNullableDisplayName(cType, "Short") :
                cType.TypeName == "Int32" ? GetNullableDisplayName(cType, "Integer") :
                cType.TypeName == "Int64" ? GetNullableDisplayName(cType, "Long") :
                GetNullableDisplayName(cType, cType.TypeName);
        }

        private static string GetNullableDisplayName(CachedType type, string actual)
        {
            return (type.IsNullableType ? "Nullable" : "") + actual;
        }
    }
}