//-----------------------------------------------------------------------
// <copyright file="ReflectionExtensions.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Namotion.Reflection
{
    /// <summary>
    /// Object extensions.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>Determines whether the specified property name exists.</summary>
        /// <param name="obj">The object.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns><c>true</c> if the property exists; otherwise, <c>false</c>.</returns>
        public static bool HasProperty(this object? obj, string propertyName)
        {
            return obj?.GetType().GetRuntimeProperty(propertyName) != null;
        }

        /// <summary>Determines whether the specified property name exists.</summary>
        /// <param name="obj">The object.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="defaultValue">The default value if the property does not exist.</param>
        /// <returns>The property or the default value.</returns>
        public static T? TryGetPropertyValue<T>(this object? obj, string propertyName, T? defaultValue = default)
        {
            var property = obj?.GetType().GetRuntimeProperty(propertyName);
            return property == null ? defaultValue : (T)property.GetValue(obj);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to disable nullability validation completely (global).
        /// </summary>
        public static bool DisableNullabilityValidation { get; set; }

        /// <summary>Checks whether the object has valid non nullable properties.</summary>
        /// <param name="obj">The object.</param>
        /// <param name="checkChildren">Specifies whether to also recursively check children.</param>
        /// <returns>The result.</returns>
        public static bool HasValidNullability(this object obj, bool checkChildren = true)
        {
            return !obj.ValidateNullability(checkChildren).Any();
        }

        /// <summary>Checks whether the object has valid non nullable properties.</summary>
        /// <param name="obj">The object.</param>
        /// <param name="checkChildren">Specifies whether to also recursively check children.</param>
        /// <returns>The result.</returns>
        public static void EnsureValidNullability(this object? obj, bool checkChildren = true)
        {
            if (obj is null)
            {
                return;
            }
            ValidateNullability(obj, obj.GetType().ToContextualType(), checkChildren ? new HashSet<object>() : null, null, false);
        }

        /// <summary>Checks which non nullable properties are null (invalid).</summary>
        /// <param name="obj">The object.</param>
        /// <param name="checkChildren">Specifies whether to also recursively check children.</param>
        /// <returns>The result.</returns>
        public static IEnumerable<string> ValidateNullability(this object obj, bool checkChildren = true)
        {
            var errors = new List<string>();
            ValidateNullability(obj, obj.GetType().ToContextualType(), checkChildren ? new HashSet<object>() : null, errors, false);
            return errors;
        }

        private static void ValidateNullability(object obj, ContextualType type, HashSet<object>? checkedObjects, List<string>? errors, bool stopFirstFail)
        {
            if (DisableNullabilityValidation)
            {
                return;
            }

            if (stopFirstFail && errors is not null && errors.Any())
            {
                return;
            }

            if (checkedObjects != null)
            {
                if (checkedObjects.Contains(obj))
                {
                    return;
                }
                else
                {
                    checkedObjects.Add(obj);
                }
            }

            if (checkedObjects != null && obj is IDictionary dictionary)
            {
                foreach (var item in dictionary.Keys.Cast<object>()
                    .Concat(dictionary.Values.Cast<object>()))
                {
                    ValidateNullability(item, type.GenericArguments[1], checkedObjects, errors, stopFirstFail);
                }
            }
            else if (checkedObjects != null && obj is IEnumerable enumerable && !(obj is string))
            {
                var itemType = type.ElementType ?? type.GenericArguments[0];
                foreach (var item in enumerable.Cast<object>())
                {
                    if (item == null)
                    {
                        if (itemType.Nullability == Nullability.NotNullable)
                        {
                            if (errors != null)
                            {
                                errors.Add(itemType.Type.Name);
                                if (stopFirstFail)
                                {
                                    return;
                                }
                            }
                            else
                            {
                                throw new InvalidOperationException("The object's nullability is invalid, item in enumerable.");
                            }
                        }
                    }
                    else
                    {
                        ValidateNullability(item, itemType, checkedObjects, errors, stopFirstFail);
                    }
                }
            }
            else if (!type.TypeInfo.IsValueType)
            {
                var properties = type.Type.GetContextualProperties();
                for (int i = 0; i < properties.Length; i++)
                {
                    var property = properties[i];
                    if (!property.PropertyType.IsValueType && property.CanRead && 
                        !property.IsAttributeDefined<CompilerGeneratedAttribute>(true))
                    {
                        var value = property.GetValue(obj);
                        if (value == null)
                        {
                            if (property.Nullability == Nullability.NotNullable)
                            {
                                if (errors != null)
                                {
                                    errors.Add(property.Name);
                                    if (stopFirstFail)
                                    {
                                        return;
                                    }
                                }
                                else
                                {
                                    throw new InvalidOperationException(
                                        "The object's nullability is invalid, property: " + property.PropertyType.Type.FullName + "." + property.Name);
                                }
                            }
                        }
                        else if (checkedObjects != null)
                        {
                            ValidateNullability(value, property.PropertyType, checkedObjects, errors, stopFirstFail);
                        }
                    }
                }
            }
        }
    }
}