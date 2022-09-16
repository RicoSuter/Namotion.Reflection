namespace Namotion.Reflection
{
    /// <summary>
    /// Contains all options to control generation of XML-docs.
    /// </summary>
    public class XmlDocsOptions
    {
        /// <summary>
        /// The default options.
        /// </summary>
        public static XmlDocsOptions Default { get; } = new XmlDocsOptions { };

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
