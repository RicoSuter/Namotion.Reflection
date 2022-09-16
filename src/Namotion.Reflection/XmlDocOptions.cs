namespace Namotion.Reflection
{
    /// <summary>
    /// Contains all options to control generation of XML-docs.
    /// </summary>
    public class XmlDocOptions
    {
        /// <summary>
        /// The default options.
        /// </summary>
        public static XmlDocOptions Default { get; } = new XmlDocOptions { };

        /// <summary>
        /// Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.
        /// </summary>
        public bool ResolveExternalXmlDocs { get; set; } = true;

        /// <summary>
        /// Specifies how formatting tags should be processed.
        /// </summary>
        public XmlDocsFormattingMode FormattingMode { get; set; } = XmlDocsFormattingMode.None;
    }
}
