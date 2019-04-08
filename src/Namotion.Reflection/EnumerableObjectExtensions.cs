//-----------------------------------------------------------------------
// <copyright file="ReflectionExtensions.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Namotion.Reflection
{
    public static class EnumerableObjectExtensions
    {
        /// <summary>Tries to get the first object of the given type name.</summary>
        /// <param name="objects">The objects.</param>
        /// <param name="typeName">Type of the attribute.</param>
        /// <param name="typeNameStyle">The type name style.</param>
        /// <returns>May return null.</returns>
        public static T TryGetByObjectType<T>(this IEnumerable<T> objects, string typeName, TypeNameStyle typeNameStyle = TypeNameStyle.FullName)
        {
            return objects.FirstOrDefault(a => a.GetType().FullName == typeName);
        }

        /// <summary>Tries to get the first object which is assignable to the given type name.</summary>
        /// <param name="objects">The objects.</param>
        /// <param name="typeName">Type of the attribute.</param>
        /// <param name="typeNameStyle">The type name style.</param>
        /// <returns>May return null (not found).</returns>
        public static T TryGetIfAssignableTo<T>(this IEnumerable<T> objects, string typeName, TypeNameStyle typeNameStyle = TypeNameStyle.FullName)
        {
            return objects != null ?
                objects.FirstOrDefault(a => a.GetType().IsAssignableTo(typeName, typeNameStyle)) :
                default;
        }
    }
}