using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Namotion.Reflection
{
    /// <summary>
    /// Contains the logic to maintain formatting of XML-doc elements.
    /// </summary>
    internal static class XmlDocsFormatting
    {
        private static readonly IDictionary<XmlDocsFormattingMode, Func<StringBuilder, XElement, StringBuilder>> formattingFunctions =
            new Dictionary<XmlDocsFormattingMode, Func<StringBuilder, XElement, StringBuilder>>()
            {
                { XmlDocsFormattingMode.Unformatted, AppendUnformattedElement },
                { XmlDocsFormattingMode.Html, AppendHtmlFormattedElement },
                { XmlDocsFormattingMode.Markdown, AppendMarkdownFormattedElement }
            };

        /// <summary>
        /// Appends the value of <paramref name="element"/> to the <paramref name="stringBuilder"/> respecting
        /// <paramref name="formattingMode"/> to generate additional formatting information.
        /// </summary>
        /// <param name="stringBuilder">The current <see cref="StringBuilder"/>.</param>
        /// <param name="element">The <see cref="XElement"/> to append.</param>
        /// <param name="formattingMode">The <see cref="XmlDocsFormattingMode"/> for generating additional formatting tags.</param>
        /// <returns>The passed in <paramref name="stringBuilder"/>.</returns>
        public static StringBuilder AppendFormattedElement(this StringBuilder stringBuilder, XElement element, XmlDocsFormattingMode formattingMode)
        {
            // call apropriate formatting function
            return formattingFunctions[formattingMode](stringBuilder, element);
        }

        #region No formatting
        /// <summary>
        /// Appends the value of <paramref name="element"/> without any formatting information.
        /// </summary>
        /// <param name="stringBuilder">The current <see cref="StringBuilder"/>.</param>
        /// <param name="element">The <see cref="XElement"/> to append.</param>
        /// <returns>The value of <paramref name="stringBuilder"/>.</returns>
        private static StringBuilder AppendUnformattedElement(this StringBuilder stringBuilder, XElement element)
        {
            stringBuilder.Append(element.Value);
            return stringBuilder;
        }
        #endregion
        #region HTML formatting
        private static readonly IDictionary<string, Func<StringBuilder, XElement, StringBuilder>> htmlTagMap =
            new Dictionary<string, Func<StringBuilder, XElement, StringBuilder>>()
            {
                { "c", (sb, e) => AppendSimpleTaggedElement(sb, e, "<pre>", "</pre>") },
                { "b", (sb, e) => AppendSimpleTaggedElement(sb, e, "<strong>", "</strong>") },
                { "strong", (sb, e) => AppendSimpleTaggedElement(sb, e, "<strong>", "</strong>") },
                { "i", (sb, e) => AppendSimpleTaggedElement(sb, e, "<i>", "</i>") }
            };

        /// <summary>
        /// Appends the value of <paramref name="element"/> surrounded by the neccessary HTML-tags to maintain its formatting information (if supported).
        /// </summary>
        /// <param name="stringBuilder">The current <see cref="StringBuilder"/>.</param>
        /// <param name="element">The <see cref="XElement"/> to append.</param>
        /// <returns>The value of <paramref name="stringBuilder"/>.</returns>
        private static StringBuilder AppendHtmlFormattedElement(StringBuilder stringBuilder, XElement element)
        {
            return AppendMapFormattedElement(stringBuilder, element, htmlTagMap);
        }
        #endregion

        #region Markdown formatting
        private static readonly IDictionary<string, Func<StringBuilder, XElement, StringBuilder>> markdownTagMap =
            new Dictionary<string, Func<StringBuilder, XElement, StringBuilder>>()
            {
                { "c", (sb, e) => AppendSimpleTaggedElement(sb, e, "`", "`") },
                { "b", (sb, e) => AppendSimpleTaggedElement(sb, e, "**", "**") },
                { "strong", (sb, e) => AppendSimpleTaggedElement(sb, e, "**", "**") },
                { "i", (sb, e) => AppendSimpleTaggedElement(sb, e, "*", "*") }
            };

        /// <summary>
        /// Appends the value of <paramref name="element"/> surrounded by the neccessary Markdown-tags to maintain its formatting information (if supported).
        /// </summary>
        /// <param name="stringBuilder">The current <see cref="StringBuilder"/>.</param>
        /// <param name="element">The <see cref="XElement"/> to append.</param>
        /// <returns>The value of <paramref name="stringBuilder"/>.</returns>
        private static StringBuilder AppendMarkdownFormattedElement(StringBuilder stringBuilder, XElement element)
        {
            return AppendMapFormattedElement(stringBuilder, element, markdownTagMap);
        }
        #endregion

        /// <summary>
        /// Appends the value of <paramref name="element"/> surrounded by the tags specified in <paramref name="map"/> (if supported).
        /// </summary>
        /// <param name="stringBuilder">The current <see cref="StringBuilder"/>.</param>
        /// <param name="element">The <see cref="XElement"/> to append.</param>
        /// <param name="map">Map of formatting tags.</param>
        /// <returns>The value of <paramref name="stringBuilder"/>.</returns>
        private static StringBuilder AppendMapFormattedElement(
            StringBuilder stringBuilder,
            XElement element,
            IDictionary<string, Func<StringBuilder, XElement, StringBuilder>> map)
        {
            if (map.ContainsKey(element.Name.LocalName))
            {
                return map[element.Name.LocalName](stringBuilder, element);
            }
            else
            {
                return AppendUnformattedElement(stringBuilder, element);
            }
        }

        /// <summary>
        /// Appends the value of <paramref name="element"/> surrounded by <paramref name="startTag"/> and <paramref name="endTag"/>
        /// to maintain its formatting information (if supported).
        /// </summary>
        /// <param name="stringBuilder">The current <see cref="StringBuilder"/>.</param>
        /// <param name="element">The <see cref="XElement"/> to append.</param>
        /// <param name="startTag">The start-tag.</param>
        /// <param name="endTag">The end-tag.</param>
        /// <returns>The value of <paramref name="stringBuilder"/>.</returns>
        private static StringBuilder AppendSimpleTaggedElement(StringBuilder stringBuilder, XElement element, string startTag, string endTag)
        {
            stringBuilder.Append(startTag, element.Value, endTag);
            return stringBuilder;
        }
    }
}
