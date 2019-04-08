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
    public static class EnumerableTypeExtensions
    {
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