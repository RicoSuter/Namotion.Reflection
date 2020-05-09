using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Namotion.Reflection.Markdown
{
    internal abstract class MarkdownNode
    {
        internal virtual bool IsBlock => false;
        internal virtual bool IsInline => !IsBlock;

        /// <summary>
        /// It is used when <c>IsBlock</c>
        /// </summary>
        internal virtual DelimiterNode DelimiterAfterBlock => DelimiterNode.NewLine;

        public string Render(int indent = 0)
        {
            var context = new RenderContext(new StringBuilder(), indent, indent);
            RenderInternal(context);
            return context.ToString();
        }

        public void Render(StringBuilder builder, int indentFirstLine, int indentRest)
        {
            var context = new RenderContext(builder, indentFirstLine, indentRest);
            RenderInternal(context);
        }

        /// <remarks>
        /// General rules
        /// <list type="number">
        /// <item>Don't put a newline character at the end unless it is necessary.</item>
        /// <item>
        ///   Call <c>context.IndentForFirstLine()</c> or its equivalent inside of this function for the first line
        ///   if this is <c>IsBlock</c> element.
        /// </item>
        /// </list>
        /// </remarks>
        internal abstract void RenderInternal(RenderContext context);

        protected static void RenderIndented(RenderContext context, string value)
        {
            var lines = value.Split(new[] { "\r\n", "\n\r", "\n", "\r" }, StringSplitOptions.None);
            if (lines.Length == 0)
            {
                return;
            }

            if (!string.IsNullOrEmpty(lines[0]))
            {
                context.IndentForFirstLine();
                context.Append(lines[0]);
            }
            else
            {
                // Can omit context.IndentForFirstLine();
                // They are dedented later anyway.
            }

            foreach (var line in lines.Skip(1))
            {
                context.AppendLine();
                if (!string.IsNullOrEmpty(line))
                    context.IndentForRest();
                context.Append(line);
            }
        }

        public override string ToString()
        {
            return Render();
        }
    }

    internal sealed class RenderContext
    {
        private readonly StringBuilder _builder;
        private readonly int _indentForFirstLine;
        private readonly int _indentForRest;

        internal RenderContext(StringBuilder builder, int indentForFirstLine, int indentForRest)
        {
            _builder = builder;
            _indentForFirstLine = indentForFirstLine;
            _indentForRest = indentForRest;
        }

        public RenderContext WithNoIndentation(int amount = 0)
        {
            return new RenderContext(_builder, 0, _indentForRest + amount);
        }

        public RenderContext WithIndentationForRest(int amount = 0)
        {
            return new RenderContext(_builder, _indentForRest + amount, _indentForRest + amount);
        }

        public void IndentForFirstLine()
        {
            Indent(_indentForFirstLine);
        }

        public void IndentForRest()
        {
            Indent(_indentForRest);
        }

        private void Indent(int indent)
        {
            for (var i = 0; i < indent; i++)
            {
                _builder.Append(' ');
            }
        }

        public void Append(string value = null)
        {
            _builder.Append(value);
        }

        public void Append(char value)
        {
            _builder.Append(value);
        }

        public void AppendLine(string value = null)
        {
            _builder.Append(value);
            _builder.Append('\n');
        }

        public override string ToString()
        {
            return _builder.ToString();
        }
    }

    internal class TextNode : MarkdownNode
    {
        public TextNode(string value, DelimiterNode startedWith, DelimiterNode endedWith)
        {
            Value = value;
            StartedWith = startedWith;
            EndedWith = endedWith;
        }

        public string Value { get; }
        public DelimiterNode StartedWith { get; }
        public DelimiterNode EndedWith { get; }

        internal override void RenderInternal(RenderContext context)
        {
            RenderIndented(context.WithNoIndentation(), Value);
        }

        public override string ToString()
        {
            return $"{StartedWith}{Value}{EndedWith}";
        }
    }

    internal sealed class ParagraphNode : MarkdownNode
    {
        internal override bool IsBlock => true;
        internal override DelimiterNode DelimiterAfterBlock => DelimiterNode.HardNewLine;

        private readonly MarkdownNode _value;

        public ParagraphNode(MarkdownNode value)
        {
            _value = value;
        }

        internal override void RenderInternal(RenderContext context)
        {
            _value.RenderInternal(context);
        }
    }

    internal sealed class InlineHtmlNode : MarkdownNode
    {
        private readonly string _value;

        public InlineHtmlNode(string value)
        {
            _value = value;
        }

        internal override void RenderInternal(RenderContext context)
        {
            RenderIndented(context.WithNoIndentation(), _value);
        }
    }

    internal sealed class HtmlBlockNode : MarkdownNode
    {
        internal override bool IsBlock => true;

        private readonly string _value;

        public HtmlBlockNode(string value)
        {
            _value = value;
        }

        internal override void RenderInternal(RenderContext context)
        {
            RenderIndented(context, _value);
        }
    }

    internal sealed class AutoLinkNode : MarkdownNode
    {
        private readonly string _url;

        public AutoLinkNode(string url)
        {
            _url = url;
        }

        internal override void RenderInternal(RenderContext context)
        {
            context.Append('<');
            context.Append(_url);
            context.Append('>');
        }
    }

    internal sealed class LinkNode : MarkdownNode
    {
        private readonly string _url;
        private readonly MarkdownNode _text;

        public LinkNode(MarkdownNode text, string url)
        {
            _text = text;
            _url = url;
        }

        public string Title { get; set; }

        internal override void RenderInternal(RenderContext context)
        {
            context.Append('[');
            RenderText(context, _text.Render());
            context.Append(']');

            context.Append('(');
            RenderUrl(context, _url);
            if (Title != null)
            {
                context.Append(' ');
                RenderTitle(context, Title);
            }

            context.Append(')');
        }

        private static void RenderText(RenderContext context, string text)
        {
            context.Append(text.Replace("\\", "\\\\").Replace("[", "\\[").Replace("]", "\\]"));
        }

        private static void RenderUrl(RenderContext context, string url)
        {
            var chars = url.ToCharArray();
            var containsNonPrintable = chars.Any(c => char.IsWhiteSpace(c) || char.IsControl(c));
            if (containsNonPrintable)
            {
                context.Append('<');
                context.Append(url.Replace("\\", "\\\\").Replace("<", "\\>").Replace(">", "\\>"));
                context.Append('>');
            }
            else
            {
                context.Append(url.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)"));
            }
        }

        private static void RenderTitle(RenderContext context, string title)
        {
            // title can contain newlines. So use RenderIndented
            var chars = title.ToCharArray();
            var singleQuotes = chars.Count(c => c == '\'');
            var doubleQuotes = chars.Count(c => c == '\"');
            // We can use "()" for quoting, but we use only single/double quotes for the sake of simplicity.
            if (singleQuotes < doubleQuotes)
            {
                context.Append('\'');
                RenderIndented(context.WithNoIndentation(), title.Replace("'", "\\'"));
                context.Append('\'');
            }
            else
            {
                context.Append('"');
                RenderIndented(context.WithNoIndentation(), title.Replace("\"", "\\\""));
                context.Append('"');
            }
        }
    }

    internal sealed class CodeNode : MarkdownNode
    {
        private readonly string _value;

        public CodeNode(string value)
        {
            _value = value;
        }

        internal override void RenderInternal(RenderContext context)
        {
            var quote = GetQuote(_value);
            var pad = _value.StartsWith("`") || _value.EndsWith("`");
            context.Append(quote);
            if (pad)
            {
                context.Append(' ');
            }

            RenderIndented(context.WithNoIndentation(), _value);
            if (pad)
            {
                context.Append(' ');
            }

            context.Append(quote);
        }

        /// <summary>
        /// Find shortest code span quote.
        /// According to the CommonMark spec, code span ends with a backtick string of equal length.
        /// So it finds a shortest backticks that does not appears in the code block
        /// </summary>
        /// <example>
        /// <code>
        /// GetQuote("` `` ````") == "```"
        /// </code>
        /// <code>
        /// GetQuote("`` ````") == "`"
        /// </code>
        /// </example>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string GetQuote(string value)
        {
            var quoteLengthSet = new SortedSet<int>();

            foreach (Match quote in Regex.Matches(value, "`+"))
            {
                quoteLengthSet.Add(quote.Length);
            }

            if (quoteLengthSet.Count == 0)
            {
                return "`";
            }

            var quoteLengths = quoteLengthSet.ToList();
            var selectedLength = 1;
            foreach (var quoteLength in quoteLengths)
            {
                if (quoteLength != selectedLength)
                {
                    break;
                }

                selectedLength++;
            }

            return string.Join("", Enumerable.Repeat("`", selectedLength));
        }
    }

    internal sealed class CodeBlockNode : MarkdownNode
    {
        internal override bool IsBlock => true;

        private readonly string _value;

        public CodeBlockNode(string value)
        {
            _value = value;
        }

        public string Info { get; set; }

        internal override void RenderInternal(RenderContext context)
        {
            var fence = GetFence(_value);
            context.IndentForFirstLine();
            context.Append(fence);
            if (Info != null)
            {
                context.Append(Info);
            }

            context.AppendLine();

            if (!string.IsNullOrEmpty(_value))
            {
                RenderIndented(context.WithIndentationForRest(), _value);
                context.AppendLine();
            }

            context.IndentForRest();
            context.Append(fence);
        }

        /// <summary>
        /// Find shortest code block fence.
        /// According to the CommonMark spec, code block ends with a fence of equal length.
        /// So it finds a shortest fence that does not appears in the code block
        /// </summary>
        /// <example>
        /// </example>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string GetFence(string value)
        {
            var fenceLengthSet = new SortedSet<int>();
            var lines = value.Split(
                new[] { "\r\n", "\n\r", "\n", "\r" },
                StringSplitOptions.None);
            foreach (var line in lines)
            {
                var trimmed = line.Trim(' ').ToCharArray();
                if (trimmed.Length < 3 || trimmed.Any(x => x != '`')) // Not a closing fence
                {
                    continue;
                }

                var spaces = line.Length - line.TrimStart(' ').Length;
                if (spaces >= 4) // It indented too much. It is not a fence.
                {
                    continue;
                }

                fenceLengthSet.Add(trimmed.Length);
            }

            if (fenceLengthSet.Count == 0)
            {
                return "```";
            }

            var fenceLengths = fenceLengthSet.ToList();
            var selectedLength = 3;
            foreach (var fenceLength in fenceLengths)
            {
                if (fenceLength != selectedLength)
                {
                    break;
                }

                selectedLength++;
            }

            return string.Join("", Enumerable.Repeat("`", selectedLength));
        }
    }

    internal class NodeCollection<T> : MarkdownNode, IEnumerable<T>
        where T : MarkdownNode
    {
        protected readonly IList<T> nodes;

        public NodeCollection()
        {
            nodes = new List<T>();
        }

        protected NodeCollection(IEnumerable<T> nodes)
        {
            this.nodes = nodes.ToList();
        }

        internal override void RenderInternal(RenderContext context)
        {
            DelimiterNode lastDelimiterNode = null;
            if (nodes.Count > 0)
            {
                nodes[0].RenderInternal(context);
                lastDelimiterNode = nodes[0] as DelimiterNode;
            }

            foreach (var node in nodes.Skip(1))
            {
                if (
                    (Equals(lastDelimiterNode, DelimiterNode.NewLine) ||
                     Equals(lastDelimiterNode, DelimiterNode.HardNewLine)) &&
                    node.IsInline)
                {
                    // HACK: compensate indentation for inline block right after the newline.
                    context.IndentForRest();
                    node.RenderInternal(context.WithNoIndentation());
                }
                else if (Equals(lastDelimiterNode, DelimiterNode.None) ||
                         Equals(lastDelimiterNode, DelimiterNode.Space) ||
                         node is DelimiterNode
                )
                {
                    node.RenderInternal(context.WithNoIndentation());
                }
                else // IsBlock || Newline || HardNewline
                {
                    node.RenderInternal(context.WithIndentationForRest());
                }

                lastDelimiterNode = node as DelimiterNode;
            }
        }

        public void Add(T node)
        {
            nodes.Add(node);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    // pseudo node for syntactic decoration 
    internal class DelimiterNode : MarkdownNode
    {
        public static readonly DelimiterNode None = new DelimiterNode(null);
        public static readonly DelimiterNode Space = new DelimiterNode(" ");
        public static readonly DelimiterNode NewLine = new DelimiterNode("\n");
        public static readonly DelimiterNode HardNewLine = new DelimiterNode("\n\n");

        internal override bool IsBlock => false;

        private readonly string _delimiter;

        private DelimiterNode(string delimiter)
        {
            _delimiter = delimiter;
        }

        internal override void RenderInternal(RenderContext context)
        {
            context.Append(_delimiter);
        }

        public override bool Equals(object obj)
        {
            return obj is DelimiterNode delimiterNode && _delimiter == delimiterNode._delimiter;
        }

        public override int GetHashCode()
        {
            return _delimiter.GetHashCode();
        }
    }

    internal abstract class ListNode : NodeCollection<ListItemNode>
    {
        internal override bool IsBlock => true;

        internal override DelimiterNode DelimiterAfterBlock => DelimiterNode.HardNewLine;

        public abstract void Add(params MarkdownNode[] nodes);

        internal override void RenderInternal(RenderContext context)
        {
            if (nodes.Count > 0)
            {
                nodes[0].RenderInternal(context);
            }

            foreach (var node in nodes.Skip(1))
            {
                context.AppendLine();
                node.RenderInternal(context.WithIndentationForRest());
            }
        }
    }

    internal sealed class BulletListNode : ListNode
    {
        public override void Add(params MarkdownNode[] nodes)
        {
            var node = new ListItemNode("* ", nodes);
            this.nodes.Add(node);
        }
    }

    internal sealed class NumberedListNode : ListNode
    {
        public override void Add(params MarkdownNode[] nodes)
        {
            var label = this.nodes.Count + 1;
            var node = new ListItemNode($"{label}. ", nodes);
            this.nodes.Add(node);
        }
    }

    internal sealed class ListItemNode : NodeCollection<MarkdownNode>
    {
        private readonly string _marker;

        public ListItemNode(string marker, params MarkdownNode[] nodes)
            : base(nodes)
        {
            _marker = marker;
        }

        internal override void RenderInternal(RenderContext context)
        {
            context.IndentForFirstLine();
            context.Append(_marker);
            if (nodes.Count > 0)
            {
                nodes[0].RenderInternal(context.WithNoIndentation(_marker.Length));
            }

            foreach (var value in nodes.Skip(1))
            {
                context.AppendLine();
                value.RenderInternal(context.WithIndentationForRest(_marker.Length));
            }
        }
    }

    internal sealed class TableNode : MarkdownNode
    {
        internal override bool IsBlock => true;
        internal override DelimiterNode DelimiterAfterBlock => DelimiterNode.HardNewLine;

        private readonly List<string> _labels = new List<string>();
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();

        private readonly List<Dictionary<string, NodeCollection<MarkdownNode>>> _rows =
            new List<Dictionary<string, NodeCollection<MarkdownNode>>>();

        internal override void RenderInternal(RenderContext context)
        {
            if (_labels.Count == 0)
                return;

            context.IndentForFirstLine();
            context.Append("| ");
            context.Append(_headers[_labels[0]]);
            foreach (var label in _labels.Skip(1))
            {
                context.Append(" | ");
                context.Append(_headers[label]);
            }

            context.Append(" |");

            context.AppendLine();
            context.IndentForRest();
            context.Append('|');
            // Can't easily decide width of non-latin strings (e.g. CJK, emoji, NJW, etc.)
            context.Append(string.Join("|", Enumerable.Repeat("---", _labels.Count)));
            context.Append('|');

            foreach (var row in _rows)
            {
                context.AppendLine();
                context.IndentForRest();
                context.Append("| ");
                row[_labels[0]].RenderInternal(context.WithNoIndentation());
                foreach (var label in _labels.Skip(1))
                {
                    context.Append(" | ");
                    row[label].RenderInternal(context.WithNoIndentation());
                }

                context.Append(" |");
            }
        }

        public void AddHeader(string label, string header)
        {
            if (!_labels.Contains(label))
            {
                _labels.Add(label);
            }

            _headers.Add(label, header);
        }

        public IDictionary<string, NodeCollection<MarkdownNode>> CreateRow()
        {
            var row = new Dictionary<string, NodeCollection<MarkdownNode>>();
            _rows.Add(row);
            return row;
        }
    }
}