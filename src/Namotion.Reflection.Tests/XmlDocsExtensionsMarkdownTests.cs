using Xunit;

namespace Namotion.Reflection.Tests
{
    public class XmlDocsExtensionsMarkdownTests
    {
        /// <remarks>
        /// Markdown support within remarks!
        /// 
        /// * OpenAPI 2 and 3 supports Markdown in `description`
        /// * So you should be able to write documents in Markdown
        ///     * Now you can write Markdown with NSwag!
        ///     * You can
        ///       <list type="bullet">
        ///         <item>mix <c>XML</c> in <c>Markdown</c>
        ///         </item>
        ///         <item>It's cool</item>
        ///         <item>
        ///            Even you can
        ///            <list type="number">
        ///              <item>nest</item>
        ///              <item>items</item>
        ///            </list>
        ///            yay!
        ///         </item>
        ///         <item>
        ///         <code>
        ///           you can even
        ///           put code here
        ///         </code>
        ///         </item>
        ///       </list>
        ///     * return to the markdown
        /// </remarks>
        public class WithComplexMarkdownDoc
        {
        }

        [Fact]
        public void When_complex_xml_remarks_is_read_then_they_are_converted_to_markdown()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var summary = typeof(WithComplexMarkdownDoc).GetXmlDocsRemarks();

            //// Assert
            var expected = @"Markdown support within remarks!

* OpenAPI 2 and 3 supports Markdown in `description`
* So you should be able to write documents in Markdown
    * Now you can write Markdown with NSwag!
    * You can
      * mix `XML` in `Markdown`
      * It's cool
      * Even you can
        1. nest
        2. items

        yay!
      * ```
        you can even
        put code here
        ```
    * return to the markdown".Replace("\r", "");
            Assert.Equal(expected, summary);
        }

        /// <remarks>
        /// <para>
        ///   This <see cref="Assert"/> should be a code.
        ///   This <see langword="null"/> is also a code.
        ///   This <see href="http://github.com"/> is an autolink.
        ///   <a href="http://github.com" title="Github">This</a> is a link.
        /// </para>
        /// </remarks>
        public class SeeAndA
        {
        }

        [Fact]
        public void When_remarks_contains_see_and_a_then_they_are_converted_to_code_or_link()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var summary = typeof(SeeAndA).GetXmlDocsRemarks();

            //// Assert
            var expected = @"This `Assert` should be a code.
This `null` is also a code.
This <http://github.com> is an autolink.
[This](http://github.com ""Github"") is a link.".Replace("\r", "");
            Assert.Equal(expected, summary);
        }

        /// <remarks>
        /// <para>
        ///   Inline <c>code</c> is usually doesn't contain newlines.
        ///   However Markdown allows <c>newline
        ///     characters! and `some ```backticks </c>
        ///   Although they are joined while they are rendered.
        /// </para>
        /// </remarks>
        public class InlineCode
        {
        }

        [Fact]
        public void When_remarks_contains_c_then_they_are_converted_to_inline_code()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var summary = typeof(InlineCode).GetXmlDocsRemarks();

            //// Assert
            var expected = @"Inline `code` is usually doesn't contain newlines.
However Markdown allows ``newline
characters! and `some ```backticks ``
Although they are joined while they are rendered.".Replace("\r", "");
            Assert.Equal(expected, summary);
        }

        /// <remarks>
        /// <para>
        ///   <code lang="csharp">
        ///      for (var i=0; i&lt;10; i++)
        ///      {
        ///        Console.WriteLine("Hello, World!");
        ///      }
        ///   </code>
        /// 
        ///   <code>
        ///      Lang is an optional
        ///   </code>
        /// </para>
        /// </remarks>
        public class CodeBlock
        {
        }

        [Fact]
        public void When_remarks_contains_code_then_they_are_converted_to_code_block()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var summary = typeof(CodeBlock).GetXmlDocsRemarks();

            //// Assert
            var expected = @"```csharp
for (var i=0; i<10; i++)
{
  Console.WriteLine(""Hello, World!"");
}
```

```
Lang is an optional
```".Replace("\r", "");
            Assert.Equal(expected, summary);
        }

        /// <remarks>
        /// <para>
        ///   <para> A para block makes block paragraph node.</para>
        ///   <para>
        ///      Subsequent paragraphs make hard newlines.
        ///   </para>
        /// </para>
        /// </remarks>
        public class Para
        {
        }

        [Fact]
        public void When_remarks_contains_para_then_they_are_converted_to_paragraph_node()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var summary = typeof(Para).GetXmlDocsRemarks();

            //// Assert
            var expected = @"A para block makes block paragraph node.

Subsequent paragraphs make hard newlines.".Replace("\r", "");
            Assert.Equal(expected, summary);
        }

        /// <remarks>
        /// <para>
        ///   <list type="table">
        ///     <listheader>
        ///       <term>Term</term>
        ///       <description>Description</description>
        ///     </listheader>
        ///     <item>
        ///       <term>Some term</term>
        ///       <description>Some description with <c>code</c></description>
        ///     </item>
        ///   </list>
        /// </para>
        /// </remarks>
        public class Table
        {
        }

        [Fact]
        public void When_remarks_contains_list_table_then_they_are_converted_to_table()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var summary = typeof(Table).GetXmlDocsRemarks();

            //// Assert
            var expected = @"| Term | Description |
|---|---|
| Some term | Some description with `code` |".Replace("\r", "");
            Assert.Equal(expected, summary);
        }

        /// <remarks>
        /// <para>
        ///   <para>hard block</para><para>hard block</para>
        ///   <para>hard block</para><code>soft block</code>
        ///   <para>hard block</para><c>inline</c>
        /// 
        ///   <code>soft block</code><para>hard block</para>
        ///   <code>soft block</code><code>soft block</code>
        ///   <code>soft block</code><c>inline</c>
        /// 
        ///   <c>inline</c><para>hard block</para>
        ///   <c>inline</c><code>soft block</code>
        ///   <c>inline</c><c>inline</c>
        /// </para>
        /// </remarks>
        public class BlockAndInlineAdjacent
        {
        }

        [Fact]
        public void When_block_inlines_adjacent_then_they_are_repels_heuristically()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var summary = typeof(BlockAndInlineAdjacent).GetXmlDocsRemarks();

            //// Assert
            var expected = @"hard block

hard block

hard block

```
soft block
```
hard block

`inline`

```
soft block
```
hard block

```
soft block
```
```
soft block
```
```
soft block
```
`inline`

`inline`
hard block

`inline`
```
soft block
```
`inline``inline`".Replace("\r", "");
            Assert.Equal(expected, summary);
        }

        /// <remarks>
        /// <para>
        ///   <para>hard block</para> <para>hard block</para>
        ///   <para>hard block</para> <code>soft block</code>
        ///   <para>hard block</para> <c>inline</c>
        /// 
        ///   <code>soft block</code> <para>hard block</para>
        ///   <code>soft block</code> <code>soft block</code>
        ///   <code>soft block</code> <c>inline</c>
        /// 
        ///   <c>inline</c> <para>hard block</para>
        ///   <c>inline</c> <code>soft block</code>
        ///   <c>inline</c> <c>inline</c>
        /// </para>
        /// </remarks>
        public class BlockAndInlineAdjacentBySpace
        {
        }

        [Fact]
        public void When_block_inlines_adjacent_by_space_then_they_are_repels_heuristically()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var summary = typeof(BlockAndInlineAdjacentBySpace).GetXmlDocsRemarks();

            //// Assert
            var expected = @"hard block

hard block

hard block

```
soft block
```
hard block

`inline`

```
soft block
```
hard block

```
soft block
```
```
soft block
```
```
soft block
```
`inline`

`inline`
hard block

`inline`
```
soft block
```
`inline` `inline`".Replace("\r", "");
            Assert.Equal(expected, summary);
        }

        /// <remarks>
        /// <para>
        /// <para>hard block</para>
        /// <para>hard block</para>
        ///
        /// <para>hard block</para>
        /// <code>soft block</code>
        /// 
        /// <para>hard block</para>
        /// <c>inline</c>
        ///
        /// 
        /// <code>soft block</code>
        /// <para>hard block</para>
        /// 
        /// <code>soft block</code>
        /// <code>soft block</code>
        /// 
        /// <code>soft block</code>
        /// <c>inline</c>
        ///
        /// 
        /// <c>inline</c>
        /// <para>hard block</para>
        /// 
        /// <c>inline</c>
        /// <code>soft block</code>
        /// 
        /// <c>inline</c>
        /// <c>inline</c>
        /// </para>
        /// </remarks>
        public class BlockAndInlineNewlines
        {
        }

        [Fact]
        public void When_block_inlines_adjacent_by_newline_then_they_respects_newline_heuristically()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var summary = typeof(BlockAndInlineNewlines).GetXmlDocsRemarks();

            //// Assert
            var expected = @"hard block

hard block


hard block

```
soft block
```

hard block

`inline`


```
soft block
```
hard block


```
soft block
```
```
soft block
```

```
soft block
```
`inline`


`inline`
hard block


`inline`
```
soft block
```

`inline`
`inline`".Replace("\r", "");
            Assert.Equal(expected, summary);
        }
    }
}