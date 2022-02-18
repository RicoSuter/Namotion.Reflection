using System.Text;

namespace Namotion.Reflection
{
    /// <summary>
    /// Contains extension for <see cref="StringBuilder"/>.
    /// </summary>
    public static class StringBuilderExtensions
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
        public static StringBuilder Append(this StringBuilder stringBuilder, params string[] values)
        {
            foreach (string value in values)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    stringBuilder.Append(value);
                }
            }
            return stringBuilder;
        }
    }
}
