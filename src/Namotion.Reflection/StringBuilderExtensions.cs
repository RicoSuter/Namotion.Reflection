using System.Text;

namespace Namotion.Reflection
{
    /// <summary>
    /// Contains extension for <see cref="StringBuilder"/>.
    /// </summary>
    internal static class StringBuilderExtensions
    {
        /// <summary>
        /// Allows to append multiple strings to the <see cref="StringBuilder"/> at once.
        /// </summary>
        /// <remarks>
        /// Only strings that are neither <c>null</c> nor <c>string.Empty</c> will be added.
        /// </remarks>
        /// <param name="stringBuilder">The instance of <see cref="StringBuilder"/>.</param>
        /// <param name="values">The values to appends.</param>
        /// <returns>The value of <paramref name="stringBuilder"/>.</returns>
        public static StringBuilder Append(this StringBuilder stringBuilder, params string?[] values)
        {
            foreach (string? value in values)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    stringBuilder.Append(value);
                }
            }
            return stringBuilder;
        }

        /// <summary>
        /// Allows to append multiple strings to the <see cref="StringBuilder"/> at once.
        /// </summary>
        /// <remarks>
        /// Only strings that are neither <c>null</c> nor <c>string.Empty</c> will be added.
        /// </remarks>
        /// <param name="stringBuilder">The instance of <see cref="StringBuilder"/>.</param>
        /// <param name="value1">First value to append.</param>
        /// <param name="value2">Second value to append.</param>
        /// <param name="value3">Third value to append. (optional)</param>
        /// <param name="value4">Fourth value to append. (optional)</param>
        /// <param name="value5">Fifth value to append. (optional)</param>
        /// <param name="value6">Sixth value to append. (optional)</param>
        /// <returns>The value of <paramref name="stringBuilder"/>.</returns>
        public static StringBuilder Append(this StringBuilder stringBuilder, string? value1, string? value2, string? value3 = null, string? value4 = null, string? value5 = null, string? value6 = null)
        {
            // Note: only value3 onwards are optional, as StringBuilder contains Append with one string so at least two must
            // be specified to call this method

            AppendStringToStringBuilder(stringBuilder, value1);
            AppendStringToStringBuilder(stringBuilder, value2);
            AppendStringToStringBuilder(stringBuilder, value3);
            AppendStringToStringBuilder(stringBuilder, value4);
            AppendStringToStringBuilder(stringBuilder, value5);
            AppendStringToStringBuilder(stringBuilder, value6);

            return stringBuilder;

        }

        private static void AppendStringToStringBuilder(StringBuilder stringBuilder, string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                stringBuilder.Append(value);
            }
        }
    }
}
