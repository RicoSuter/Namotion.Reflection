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
        // List of methods to use for requested XML-Docs-Formatting.
        private static readonly Dictionary<XmlDocsFormattingMode, Func<StringBuilder, XElement, StringBuilder>> formattingFunctions =
            new Dictionary<XmlDocsFormattingMode, Func<StringBuilder, XElement, StringBuilder>>()
            {
                { XmlDocsFormattingMode.None, AppendUnformattedElement },
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

        /// <summary>
        /// Appends a formatted hyperlink to the specified <see cref="StringBuilder"/>, using the given link text, URL,
        /// and formatting mode.
        /// </summary>
        /// <remarks>When <paramref name="formattingMode"/> is <see cref="XmlDocsFormattingMode.Html"/>,
        /// the link is appended as an HTML a element.
        /// When set to <see cref="XmlDocsFormattingMode.Markdown"/>, the link is appended in Markdown link syntax.
        /// If <see cref="XmlDocsFormattingMode.None"/> is specified, only the link text is appended with no formatting.
        /// </remarks>
        /// <param name="stringBuilder">The <see cref="StringBuilder"/> to which the formatted link will be appended. Cannot be null.</param>
        /// <param name="linkText">The text to display for the link. If <paramref name="formattingMode"/> is
        /// <see cref="XmlDocsFormattingMode.None"/>, this text is appended without formatting.</param>
        /// <param name="linkUrl">The URL to use for the hyperlink. Ignored if <paramref name="formattingMode"/> is
        /// <see cref="XmlDocsFormattingMode.None"/>.</param>
        /// <param name="formattingMode">The formatting mode that determines how the link is rendered. Supported values are
        /// <see cref="XmlDocsFormattingMode.Html"/>, <see cref="XmlDocsFormattingMode.Markdown"/>, and
        /// <see cref="XmlDocsFormattingMode.None"/>.</param>
        /// <returns>The <see cref="StringBuilder"/> instance with the formatted link appended.</returns>
        public static StringBuilder AppendFormattedLink(this StringBuilder stringBuilder, string linkText, string linkUrl, XmlDocsFormattingMode formattingMode)
        {
            return formattingMode switch
            {
                XmlDocsFormattingMode.Html => stringBuilder.Append("<a href=\"", linkUrl, "\">", linkText, "</a>"),
                XmlDocsFormattingMode.Markdown => stringBuilder.Append("[", linkText, "](", linkUrl, ")"),
                _ => stringBuilder.Append(linkText)
            };
        }

        /// <summary>
        /// Appends the value of <paramref name="element"/> without any formatting information.
        /// </summary>
        /// <param name="stringBuilder">The current <see cref="StringBuilder"/>.</param>
        /// <param name="element">The <see cref="XElement"/> to append.</param>
        /// <returns>The value of <paramref name="stringBuilder"/>.</returns>
        public static StringBuilder AppendUnformattedElement(this StringBuilder stringBuilder, XElement element)
        {
            stringBuilder.Append(element.ToXmlDocsContent(new XmlDocsOptions { FormattingMode = XmlDocsFormattingMode.None }).Trim());
            return stringBuilder;
        }

        // Map XML-Docs-Tags to HTML-Tags
        private static readonly Dictionary<string, Func<StringBuilder, XElement, StringBuilder>> htmlTagMap =
            new Dictionary<string, Func<StringBuilder, XElement, StringBuilder>>()
            {
                { "a", AppendHtmlLink },
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

        // Map XML-Docs-Tags to Markdown-Codes
        private static readonly Dictionary<string, Func<StringBuilder, XElement, StringBuilder>> markdownTagMap =
            new Dictionary<string, Func<StringBuilder, XElement, StringBuilder>>()
            {
                { "a", AppendMarkdownLink },
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
            Dictionary<string, Func<StringBuilder, XElement, StringBuilder>> map)
        {
            if (map.TryGetValue(element.Name.LocalName, out Func<StringBuilder, XElement, StringBuilder> formattingFunction))
            {
                return formattingFunction(stringBuilder, element);
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

        /// <summary>
        /// Appends the value of <paramref name="element"/> as an html paragraph.
        /// </summary>
        /// <param name="stringBuilder">The current <see cref="StringBuilder"/>.</param>
        /// <param name="element">The <see cref="XElement"/> to append.</param>
        /// <param name="options">The XML docs reading and formatting options.</param>
        /// <returns>The value of <paramref name="stringBuilder"/>.</returns>
        public static StringBuilder AppendHtmlParagraph(this StringBuilder stringBuilder, XElement element, XmlDocsOptions options)
        {
            var paragraph = element.ToXmlDocsContent(options).Trim();
            stringBuilder.Append("<p>", paragraph, "</p>");
            return stringBuilder;
        }

        /// <summary>
        /// Appends the value of <paramref name="element"/> as a markdown paragraph.
        /// </summary>
        /// <param name="stringBuilder">The current <see cref="StringBuilder"/>.</param>
        /// <param name="element">The <see cref="XElement"/> to append.</param>
        /// <param name="options">The XML docs reading and formatting options.</param>
        /// <returns>The value of <paramref name="stringBuilder"/>.</returns>
        public static StringBuilder AppendMarkdownParagraph(this StringBuilder stringBuilder, XElement element, XmlDocsOptions options)
        {
            if (stringBuilder.Length > 0)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.Append(element.ToXmlDocsContent(options).Trim());
            return stringBuilder;
        }

        /// <summary>
        /// Appends the value of <paramref name="element"/> as an html link.
        /// </summary>
        /// <param name="stringBuilder">The current <see cref="StringBuilder"/>.</param>
        /// <param name="element">The <see cref="XElement"/> to append.</param>
        /// <returns>The value of <paramref name="stringBuilder"/>.</returns>
        private static StringBuilder AppendHtmlLink(StringBuilder stringBuilder, XElement element)
        {
            stringBuilder.Append("<a href=\"", element.Attribute("href")?.Value, "\">", element.Value, "</a>");
            return stringBuilder;
        }

        /// <summary>
        /// Appends the value of <paramref name="element"/> as a markdown link.
        /// </summary>
        /// <param name="stringBuilder">The current <see cref="StringBuilder"/>.</param>
        /// <param name="element">The <see cref="XElement"/> to append.</param>
        /// <returns>The value of <paramref name="stringBuilder"/>.</returns>
        private static StringBuilder AppendMarkdownLink(StringBuilder stringBuilder, XElement element)
        {
            stringBuilder.Append("[", element.Value, "](", element.Attribute("href")?.Value, ")");
            return stringBuilder;
        }
    }
}
