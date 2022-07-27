namespace Namotion.Reflection
{
    /// <summary>
    /// Contains all options to control generation of XML-docs.
    /// </summary>
    public class XmlDocOptions
    {
        /// <summary>
        /// Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.
        /// </summary>
        public bool ResolveExternalXmlDocs { get; set; } = true;

        /// <summary>
        /// Specifies how formatting tags should be processed.
        /// </summary>
        public XmlDocsFormattingMode FormattingMode { get; set; } = XmlDocsFormattingMode.Unformatted;

        /// <summary>
        /// Creates and initializes an instance of <see cref="XmlDocOptions"/> based on the flag <see cref="ResolveExternalXmlDocs"/>.
        /// </summary>
        /// <param name="resolveExternalXmlDocs">Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.</param>
        /// <returns>Instance of <see cref="XmlDocOptions"/> with initialized flag <see cref="ResolveExternalXmlDocs"/>.</returns>
        internal static XmlDocOptions Create(bool resolveExternalXmlDocs)
        {
            return new XmlDocOptions() { ResolveExternalXmlDocs = resolveExternalXmlDocs };
        }
    }
}
