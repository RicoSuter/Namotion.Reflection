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
        /// <returns>May return null (not found).</returns>
        public static T FirstAssignableToTypeNameOrDefault<T>(this IEnumerable<T> objects, string typeName, TypeNameStyle typeNameStyle = TypeNameStyle.FullName)
        {
            return objects != null ?
                objects.FirstOrDefault(a => a.GetType().IsAssignableToTypeName(typeName, typeNameStyle)) :
                default;
        }

        /// <summary>Finds the first common base of the given types.</summary>
        /// <param name="types">The types.</param>
        /// <returns>The common base type.</returns>
        public static Type FindCommonBaseType(this IEnumerable<Type> types)
        {
            var baseType = types.First();
            while (baseType != typeof(object) && baseType != null)
            {
                if (types.All(t => baseType.GetTypeInfo().IsAssignableFrom(t.GetTypeInfo())))
                    return baseType;

                baseType = baseType.GetTypeInfo().BaseType;
            }

            return typeof(object);
        }
    }
}