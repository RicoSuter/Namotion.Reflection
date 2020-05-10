using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Namotion.Reflection.Markdown
{
    internal static class XmlTransform
    {
        /// <summary>Converts the given XML documentation <see cref="XElement"/> to Markdown.</summary>
        /// <param name="element">The XML element.</param>
        /// <returns>The Markdown</returns>
        public static string ToMarkdown(XElement element)
        {
            // ToMarkdown -> ToMarkdownNode  <-> ToMarkdownNodeCollection
            // ^- top level  ^- inside of XML ^- mutual recursion
            if (element == null)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            var lastIndent = 0;
            foreach (var node in element.Nodes())
            {
                if (node is XElement e && ToMarkdownNode(e) is var markdownNode && markdownNode != null)
                {
                    markdownNode.Render(builder, 0, lastIndent);
                }
                else
                {
                    // Formats of top level texts are preserved
                    var text = node.ToString();
                    var lastNewline = text.LastIndexOfAny(new[] { '\n', '\r' });
                    lastIndent = lastNewline == -1 ? 0 : text.Length - lastNewline - 1;
                    builder.Append(node);
                }
            }

            return DedentFinalResult(builder.ToString());
        }

        private static NodeCollection<MarkdownNode> ToMarkdownNodeCollection(IEnumerable<XNode> nodes)
        {
            var collected = new List<MarkdownNode>();
            foreach (var node in nodes)
            {
                if (node is XElement e && ToMarkdownNode(e) is var markdownNode && markdownNode != null)
                {
                    collected.Add(markdownNode);
                }
                else if (node is XText textNode)
                {
                    collected.Add(ConvertTextNode(textNode));
                }

                // Skip XComment, XDocumentType, XProcessingInstruction, XDocument
            }

            return CreateDeliminatedNodeCollection(collected);
        }

        /// They are defined in <a href="https://spec.commonmark.org/0.29/#html-blocks">CommonMark spec</a>
        private static readonly HashSet<string> htmlBlocks =
            new HashSet<string>
            {
                "address", "article", "aside", "base", "basefont", "blockquote", "body", "caption", "center", "col", 
                "colgroup", "dd", "details", "dialog", "dir", "div", "dl", "dt", "fieldset", "figcaption", "figure", 
                "footer", "form", "frame", "frameset", "h1", "h2", "h3", "h4", "h5", "h6", "head", "header", "hr", 
                "html", "iframe", "legend", "li", "link", "main", "menu", "menuitem", "nav", "noframes", "ol", 
                "optgroup", "option", "p", "param", "section", "source", "summary", "table", "tbody", "td", "tfoot", 
                "th", "thead", "title", "tr", "track", "ul"
            };

        private static MarkdownNode ToMarkdownNode(XElement element)
        {
            if (element.Name == "see")
            {
                var langword = element.Attribute("langword");
                if (langword != null)
                {
                    return new CodeNode(langword.Value);
                }

                var cref = element.Attribute("cref");
                if (cref != null)
                {
                    var crefValue = cref.Value.Trim('!', ':').Trim().Split('.').Last();
                    return new CodeNode(crefValue);
                }

                var href = element.Attribute("href")?.Value ?? string.Empty;

                return new AutoLinkNode(href);
            }

            if (element.Name == "a")
            {
                // assume all inline
                var text = ToMarkdownNodeCollection(element.Nodes());
                var hrefAttribute = element.Attribute("href");
                var href = hrefAttribute != null ? hrefAttribute.Value : string.Empty;
                return new LinkNode(text, href) { Title = element.Attribute("title")?.Value };
            }

            if (element.Name == "c")
            {
                return new CodeNode(DedentElement(element));
            }

            if (element.Name == "code")
            {
                return new CodeBlockNode(DedentElement(element))
                {
                    Info = element.Attribute("lang")?.Value // Non standard, but useful extension
                };
            }

            if (element.Name == "paramref" || element.Name == "typeparamref")
            {
                var name = element.Attribute("name")?.Value ?? string.Empty;
                return new CodeNode(name);
            }

            if (element.Name == "para")
            {
                return new ParagraphNode(ToMarkdownNodeCollection(element.Nodes()));
            }

            if (element.Name == "list")
            {
                var type = element.Attribute("type")?.Value;
                var items = element.Nodes().OfType<XElement>().Where(x => x.Name == "item");
                if (type == "bullet" || type == "number")
                {
                    var node = type == "bullet" ? (ListNode) new BulletListNode() : new NumberedListNode();
                    foreach (var item in items)
                    {
                        // JetBrains Rider prefers <description> element.
                        var description = item.Nodes().OfType<XElement>().FirstOrDefault(x => x.Name == "description");
                        node.Add(ToMarkdownNodeCollection(description != null ? description.Nodes() : item.Nodes()));
                    }

                    return node;
                }

                if (type == "table")
                {
                    var node = new TableNode();

                    var listHeader = element.Nodes().OfType<XElement>().FirstOrDefault(x => x.Name == "listheader");
                    if (listHeader != null)
                    {
                        foreach (var headerElement in listHeader.Nodes().OfType<XElement>())
                        {
                            node.AddHeader(headerElement.Name.LocalName, DedentElement(headerElement));
                        }
                    }

                    foreach (var item in items)
                    {
                        var row = node.CreateRow();
                        foreach (var itemElement in item.Nodes().OfType<XElement>())
                        {
                            row[itemElement.Name.LocalName] = ToMarkdownNodeCollection(itemElement.Nodes());
                        }
                    }

                    return node;
                }
            }

            if (htmlBlocks.Contains(element.Name.LocalName))
            {
                return new HtmlBlockNode(DedentHtmlElement(element));
            }

            return new InlineHtmlNode(DedentHtmlElement(element));
        }

        private static readonly string[] Newlines = { "\r\n", "\n\r", "\n", "\r" };

        private static string DedentElement(XElement element)
        {
            // e.g.
            //  v-- LinePosition. 1-based.
            // <code>
            //           some
            //        irregular
            //            indentation
            //    </code>
            //        ^- adjust to here
            var lines = SplitLine(element.Value, true);
            var indent = GetIndentation(((IXmlLineInfo) element).LinePosition - 2, lines);
            return Dedent(lines, line => TrimWhitespacesAtMost(indent, line));
        }

        private static string DedentHtmlElement(XElement element)
        {
            // e.g.
            //  v-- LinePosition. 1-based.
            // <h1>
            //           some
            //            header
            //       </h1>
            //       ^- adjust to here
            var lines = SplitLine(element.ToString(), true);
            var indent = GetIndentation(((IXmlLineInfo) element).LinePosition - 2, lines);

            var builder = new StringBuilder();
            if (lines.Count > 0)
            {
                builder.Append(lines[0]); // Append without dedent since XNode.ToString() doesn't indent first line.
            }

            foreach (var line in lines.Skip(1))
            {
                builder.Append('\n');
                builder.Append(line.Substring(indent));
            }

            return builder.ToString();
        }

        private static string DedentFinalResult(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            // e.g.
            //      some
            //    irregular
            //       indentation
            //    ^- adjust to here
            var lines = SplitLine(value, true);
            var indent = GetIndentation(1, lines);
            return Dedent(lines, line => TrimWhitespacesAtMost(indent, line));
        }

        private static string Dedent(IList<string> lines, Func<string, string> dedent)
        {
            var builder = new StringBuilder();
            if (lines.Count > 0)
            {
                builder.Append(dedent(lines[0]));
            }

            foreach (var line in lines.Skip(1))
            {
                builder.Append('\n');
                builder.Append(dedent(line));
            }

            return builder.ToString();
        }

        private static List<string> SplitLine(string value, bool trimBothEmptyLines)
        {
            var lines = value.Split(Newlines, StringSplitOptions.None);
            var skip = 0;
            var take = lines.Length;

            if (trimBothEmptyLines)
            {
                if (string.IsNullOrWhiteSpace(lines.First()))
                {
                    skip++;
                    take--;
                }

                if (string.IsNullOrWhiteSpace(lines.Last()))
                {
                    take--;
                }
            }

            return lines.Skip(skip).Take(take).ToList();
        }

        private static int GetIndentation(int filterMinimumIndent, IList<string> lines)
        {
            if (lines.Count == 0)
            {
                return 0;
            }

            var indent = int.MaxValue;
            foreach (var line in lines)
            {
                var whitespaces = Regex.Match(line, "^\\s*").Length;
                if (whitespaces < filterMinimumIndent)
                {
                    continue;
                }

                indent = Math.Min(indent, whitespaces);
            }

            return indent;
        }

        private static string TrimWhitespacesAtMost(int count, string text)
        {
            var nonWsIndex = 0;
            while (nonWsIndex < text.Length && char.IsWhiteSpace(text[nonWsIndex]) && nonWsIndex < count)
            {
                nonWsIndex++;
            }

            return text.Substring(nonWsIndex);
        }

        private static MarkdownNode ConvertTextNode(XText textNode)
        {
            var lines = textNode.Value.Split(new[] { "\r\n", "\n\r", "\n", "\r" }, StringSplitOptions.None);
            // Assert(lines.Length > 0)
            if (lines.All(string.IsNullOrWhiteSpace))
            {
                switch (lines.Length)
                {
                    case 1 when lines[0].Length == 0:
                        return DelimiterNode.None; // custom delimiter
                    case 1:
                        return DelimiterNode.Space; // custom delimiter
                    case 2:
                        return DelimiterNode.NewLine; // custom delimiter
                    case 3:
                        return new TextNode(
                            string.Join("", Enumerable.Repeat("\n", lines.Length - 3)),
                            DelimiterNode.NewLine,
                            DelimiterNode.NewLine);
                }
            }

            var skip = 0;
            var take = lines.Length;

            var startedWith = DelimiterNode.None;
            if (string.IsNullOrWhiteSpace(lines.First()))
            {
                startedWith = DelimiterNode.NewLine;
                skip++;
                take--;
            }
            else if (lines.First().StartsWith(" ") || lines.First().StartsWith("\t"))
            {
                startedWith = DelimiterNode.Space;
            }

            var endedWith = DelimiterNode.None;
            if (string.IsNullOrWhiteSpace(lines.Last()))
            {
                endedWith = DelimiterNode.NewLine;
                take--;
            }
            else if (lines.Last().EndsWith(" ") || lines.Last().EndsWith("\t"))
            {
                endedWith = DelimiterNode.Space;
            }

            var joined = string.Join("\n", lines.Skip(skip).Take(take).Select(line => line.Trim()));
            return new TextNode(joined, startedWith, endedWith);
        }

        /// <summary>
        /// Create <see cref="NodeCollection{T}"/> with delimiter heuristics.
        /// </summary>
        private static NodeCollection<MarkdownNode> CreateDeliminatedNodeCollection(List<MarkdownNode> collected)
        {
            var result = new NodeCollection<MarkdownNode>();
            var end = Math.Max(collected.Count - 1, 0);
            MarkdownNode lastInserted = null;
            for (var i = 0; i < end; i++)
            {
                var left = collected[i];
                var right = collected[i + 1];

                // Trim the first empty line
                if (i == 0 &&
                    (left is TextNode first && string.IsNullOrWhiteSpace(first.Value) ||
                     left is DelimiterNode))
                {
                    continue;
                }

                // Trim the last empty line
                if (i + 1 == end &&
                    (right is TextNode last && string.IsNullOrWhiteSpace(last.Value) ||
                     right is DelimiterNode))
                {
                    result.Add(left);
                    return result;
                }

                // Skip subsequent custom delimiters
                if (lastInserted is DelimiterNode && left is DelimiterNode)
                {
                    continue;
                }

                if (left.IsInline && right.IsInline)
                {
                    if (left is TextNode leftText)
                    {
                        result.Add(left);
                        result.Add(leftText.EndedWith);
                    }
                    else if (right is TextNode rightText)
                    {
                        result.Add(left);
                        result.Add(rightText.StartedWith);
                    }
                    else if (right is DelimiterNode)
                    {
                        result.Add(left);
                        // the delimiter will be decided next iteration
                    }
                    else
                    {
                        result.Add(left);
                        result.Add(DelimiterNode.None);
                    }
                }
                else if (left.IsInline && right.IsBlock)
                {
                    // Skip custom delimiter between inline and block
                    if (!(lastInserted?.IsInline == true && left is DelimiterNode))
                    {
                        result.Add(left);
                    }

                    // Insert a newline at the boundary between inline-block unconditionally.
                    result.Add(DelimiterNode.NewLine);
                }
                else if (left.IsBlock)
                {
                    result.Add(left);
                    // Insert a newline after a block node. Each block node has its taste of newline.
                    result.Add(left.DelimiterAfterBlock);
                }
                // Since IsBlock != IsInline, all cases are covered

                lastInserted = result.Last();
            }

            if (collected.LastOrDefault() != null)
            {
                result.Add(collected.Last());
            }

            return result;
        }
    }
}