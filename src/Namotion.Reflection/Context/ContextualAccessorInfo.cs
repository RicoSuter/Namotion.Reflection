namespace Namotion.Reflection
{
    /// <summary>Base class for a contextual property or field.</summary>
    public abstract class ContextualAccessorInfo : ContextualMemberInfo
    {
        /// <summary>
        /// Gets the accessor's type.
        /// </summary>
        public abstract ContextualType AccessorType { get; }

        // TODO: Should we implement this?
        /// <summary>
        /// Gets the nullability information of this accessor's type in the given context by unwrapping Nullable{T}.
        /// </summary>
        public Nullability Nullability => AccessorType.Nullability;

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
    }
}
