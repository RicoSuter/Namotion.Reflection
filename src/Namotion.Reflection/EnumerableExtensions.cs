//-----------------------------------------------------------------------
// <copyright file="ReflectionExtensions.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    /// <summary>
    /// IEnumerable extensions.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>Tries to get the first object which is assignable to the given type name.</summary>
        /// <param name="objects">The objects.</param>
        /// <param name="typeName">Type of the attribute.</param>
        /// <param name="typeNameStyle">The type name style.</param>
        /// <returns>The objects which are assignable.</returns>
        public static IEnumerable<T> GetAssignableToTypeName<T>(this IEnumerable<T> objects, string typeName, TypeNameStyle typeNameStyle = TypeNameStyle.FullName)
        {
            foreach (var o in objects)
            {
                if (o.GetType().IsAssignableToTypeName(typeName, typeNameStyle))
                {
                    yield return o;
                }
            }
        }

        /// <summary>Tries to get the first object which is assignable to the given type name.</summary>
        /// <param name="objects">The objects.</param>
        /// <param name="typeName">Type of the attribute.</param>
        /// <param name="typeNameStyle">The type name style.</param>
        /// <returns>May return null (not found).</returns>
        public static T? FirstAssignableToTypeNameOrDefault<T>(this IEnumerable<T>? objects, string typeName, TypeNameStyle typeNameStyle = TypeNameStyle.FullName)
        {
            if (objects is T[] array)
            {
                foreach (var o in array)
                {
                    if (o.GetType().IsAssignableToTypeName(typeName, typeNameStyle))
                    {
                        return o;
                    }
                }
            }
            else if (objects is not null)
            {
                foreach (var o in objects)
                {
                    if (o.GetType().IsAssignableToTypeName(typeName, typeNameStyle))
                    {
                        return o;
                    }
                }
            }
            return default;
        }

        /// <summary>Finds the first common base type of the given types.</summary>
        /// <param name="types">The types.</param>
        /// <returns>The common base type.</returns>
        public static Type GetCommonBaseType(this IEnumerable<Type> types)
        {
            types = types.ToList();
            var baseType = types.First();
            while (baseType != typeof(object) && baseType != null)
            {
                if (types.All(t => baseType.GetTypeInfo().IsAssignableFrom(t.GetTypeInfo())))
                {
                    return baseType;
                }

                baseType = baseType.GetTypeInfo().BaseType;
            }

            return typeof(object);
        }

        internal static T? GetSingleOrDefault<T>(this Attribute[] attributes)
        {
            static void ThrowInvalidOperation()
            {
                throw new InvalidOperationException("Found more than one element");
            }

            T? found = default;
            foreach (var attribute in attributes)
            {
                if (attribute is T typed)
                {
                    if (found is not null)
                    {
                        ThrowInvalidOperation();
                    }

                    found = typed;
                }
            }
            return found;
        }
    }
}
