using Mono.Cecil;
using Namotion.Reflection.Infrastructure;
using System.Collections;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using static Namotion.Reflection.XmlDocsExtensions;

namespace Namotion.Reflection.Cecil
{
    /// <summary>
    /// Cecil xml docs.
    /// </summary>
    public static class XmlDocs
    {
        /// <summary>Loads the XML docs from a path.</summary>
        /// <param name="pathToXmlFile">The path.</param>
        /// <returns>The document</returns>
        public static XDocument LoadDocument(string pathToXmlFile)
        {
            return XDocument.Load(pathToXmlFile, LoadOptions.PreserveWhitespace);
        }

        /// <summary>Loads the XML docs from a stream.</summary>
        /// <param name="stream">The stream.</param>
        /// <returns>The document</returns>
        public static XDocument LoadDocument(Stream stream)
        {
            return XDocument.Load(stream, LoadOptions.PreserveWhitespace);
        }
    }

    /// <summary>Provides extension methods for reading XML comments from reflected members.</summary>
    public static class XmlDocsExtensions
    {
        /// <summary>Returns the contents of an XML documentation summary for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="document">The document.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsSummary(this IMemberDefinition member, XDocument document)
        {
            return member.GetXmlDocsTag("summary", document);
        }

        /// <summary>Returns the contents of an XML documentation remarks for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="document">The document.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsRemarks(this IMemberDefinition member, XDocument document)
        {
            return member.GetXmlDocsTag("remarks", document);
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="document">The document.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsTag(this IMemberDefinition member, string tagName, XDocument document)
        {
            if (DynamicApis.SupportsXPathApis == false)
            {
                return string.Empty;
            }

            var name = GetMemberElementName(member);
            var element = document.GetXmlDocsElement(name);
            return element?.Element(tagName).ToXmlDocsContent();
        }

        /// <summary>Returns the contents of an XML documentation for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="document">The document.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static XElement GetXmlDocsElement(this IMemberDefinition member, XDocument document)
        {
            if (DynamicApis.SupportsXPathApis == false)
            {
                return null;
            }

            var name = GetMemberElementName(member);
            return document.GetXmlDocsElement(name);
        }

        /// <summary>Returns the contents of an XML documentation for the specified member.</summary>
        /// <param name="parameter">The reflected member.</param>
        /// <param name="document">The document.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocs(this ParameterDefinition parameter, XDocument document)
        {
            if (DynamicApis.SupportsXPathApis == false)
            {
                return string.Empty;
            }

            var element = parameter.GetXmlDocsElement(document);
            return element.ToXmlDocsContent();
        }

        /// <summary>Returns the contents of an XML documentation for the specified member.</summary>
        /// <param name="methodReturnType">The reflected member.</param>
        /// <param name="document">The document.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocs(this MethodReturnType methodReturnType, XDocument document)
        {
            if (DynamicApis.SupportsXPathApis == false)
            {
                return string.Empty;
            }

            var element = methodReturnType.GetXmlDocsElement(document);
            return element.ToXmlDocsContent();
        }

        private static XElement GetXmlDocsElement(this MethodReturnType parameter, XDocument xml)
        {
            var name = GetMemberElementName(parameter.Method);
            var result = (IEnumerable)DynamicApis.XPathEvaluate(xml, $"/doc/members/member[@name='{name}']");

            var element = result.OfType<XElement>().FirstOrDefault();
            if (element != null)
            {
                //await ReplaceInheritdocElementsAsync(parameter.Member, element).ConfigureAwait(false);

                var elements = (IEnumerable)DynamicApis.XPathEvaluate(xml, $"/doc/members/member[@name='{name}']/returns");
                return elements.OfType<XElement>().FirstOrDefault();
            }

            return null;
        }

        private static XElement GetXmlDocsElement(this ParameterDefinition parameter, XDocument xml)
        {
            var name = GetMemberElementName(parameter.Method);
            var result = (IEnumerable)DynamicApis.XPathEvaluate(xml, $"/doc/members/member[@name='{name}']");

            var element = result.OfType<XElement>().FirstOrDefault();
            if (element != null)
            {
                //await ReplaceInheritdocElementsAsync(parameter.Member, element).ConfigureAwait(false);

                var elements = (IEnumerable)DynamicApis.XPathEvaluate(xml, $"/doc/members/member[@name='{name}']/param[@name='{parameter.Name}']");
                return elements.OfType<XElement>().FirstOrDefault();
            }

            return null;
        }
    }
}
