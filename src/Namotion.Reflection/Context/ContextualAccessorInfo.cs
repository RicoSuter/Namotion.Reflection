using System;
using System.Collections.Generic;
using System.Linq;

namespace Namotion.Reflection
{
    /// <summary>Base class for a contextual property or field.</summary>
    public abstract class ContextualAccessorInfo : ContextualMemberInfo
    {
        /// <summary>
        /// Gets the accessor's type.
        /// </summary>
        public abstract ContextualType AccessorType { get; }

        /// <summary>
        /// Gets the nullability information of this accessor's type in the given context by unwrapping Nullable{T}.
        /// </summary>
        public Nullability Nullability => AccessorType.Nullability;

        /// <summary>
        /// Gets the accessor's contextual attributes (e.g. attributes on property or field).
        /// </summary>
        public Attribute[] ContextAttributes => AccessorType.ContextAttributes;

        /// <summary>
        /// Returns the value of a field supported by a given object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The value.</returns>
        public abstract object? GetValue(object? obj);

        /// <summary>
        /// Sets the value of the field supported by the given object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public abstract void SetValue(object? obj, object? value);

        /// <summary>
        /// Gets an attribute of the given type which is defined on the context (property, field, parameter or contextual generic argument type).
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attribute or null.</returns>
        public T? GetContextAttribute<T>()
        {
            return ContextAttributes.OfType<T>().SingleOrDefault();
        }

        /// <summary>
        /// Gets the attributes of the given type which are defined on the context (property, field, parameter or contextual generic argument type).
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attributes.</returns>
        public IEnumerable<T> GetContextAttributes<T>()
        {
            return ContextAttributes.OfType<T>();
        }
    }
}
