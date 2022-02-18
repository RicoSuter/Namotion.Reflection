namespace Namotion.Reflection
{
    /// <summary>
    /// Contains the formatting modes supported.
    /// </summary>
    public enum XmlDocsFormattingMode
    {
        /// <summary>
        /// Doesn't use any formatting.
        /// </summary>
        Unformatted,

        /// <summary>
        /// Maintains formatting through HTML-tags.
        /// </summary>
        Html,

        /// <summary>
        /// Maintains formatting through Markdown-tags.
        /// </summary>
        Markdown
    }
}
