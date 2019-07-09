namespace Namotion.Reflection
{
    /// <summary>
    /// Specifies the nullability in the given context.
    /// </summary>
    public enum Nullability
    {
        /// <summary>
        /// Nullability is unknown (NRT is not enabled).
        /// </summary>
        Unknown,

        ///// <summary>
        ///// Never null because this is not a reference type.
        ///// </summary>
        //NeverNull,

        /// <summary>
        /// Reference type is not nullable.
        /// </summary>
        NotNullable,

        /// <summary>
        /// Reference type can be null.
        /// </summary>
        Nullable
    }
}
