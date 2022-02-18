namespace Namotion.Reflection
{
    /// <summary>
    /// Contains constants for element and attribute names use in XML-docs.
    /// </summary>
    internal static class XmlDocsKeys
    {
        /// <summary>
        /// Name of the summary element.
        /// </summary>
        public const string SummaryElement = "summary";

        /// <summary>
        /// Name of the remarks element.
        /// </summary>
        public const string RemarksElement = "remarks";

        /// <summary>
        /// Name of the param element.
        /// </summary>
        public const string ParamElement = "param";

        /// <summary>
        /// Name of the name attribute used in conjunction with the <see cref="ParamElement"/>.
        /// </summary>
        public const string ParamNameAttribute = "name";

        /// <summary>
        /// Name of the paramref element.
        /// </summary>
        public const string ParamRefElement = "paramref";

        /// <summary>
        /// Name of the name attribute used in conjunction with the <see cref="ParamRefElement"/>.
        /// </summary>
        public const string ParamRefNameAttribute = "name";

        /// <summary>
        /// Name of the see element.
        /// </summary>
        public const string SeeElement = "see";

        /// <summary>
        /// Name of the langword attribute used in conjunction with the <see cref="SeeElement"/>.
        /// </summary>
        public const string SeeLangwordAttribute = "langword";

        /// <summary>
        /// Name of the cref attribute used in conjunction with the <see cref="SeeElement"/>.
        /// </summary>
        public const string SeeCrefAttribute = "cref";

        /// <summary>
        /// Name of the href attribute used in conjunction with the <see cref="SeeElement"/>.
        /// </summary>
        public const string SeeHrefAttribute = "href";

        /// <summary>
        /// Name of the returns element.
        /// </summary>
        public const string ReturnsElement = "returns";

        /// <summary>
        /// Name of the inheritdoc element.
        /// </summary>
        public const string InheritDocElement = "inheritdoc";
    }
}
