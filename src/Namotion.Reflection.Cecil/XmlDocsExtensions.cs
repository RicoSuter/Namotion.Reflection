using System;
using Mono.Cecil;
using Namotion.Reflection.Infrastructure;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

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

            var name = CecilReflection.GetMemberElementName(member);
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

            var name = CecilReflection.GetMemberElementName(member);
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
            var name = CecilReflection.GetMemberElementName(parameter.Method);
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
            var name = CecilReflection.GetMemberElementName(parameter.Method);
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
        
        
        private static class CecilReflection
        {

        /// <exception cref="ArgumentException">Unknown member type.</exception>
        internal static string GetMemberElementName(dynamic member)
        {
            char prefixCode;
            string memberName;
            string memberTypeName;

            var memberType = ((object)member).GetType();
            if (memberType.FullName.Contains(".Cecil."))
            {
                memberName = TypeExtensions.IsAssignableToTypeName(memberType, "TypeDefinition", TypeNameStyle.Name) ?
                    member.FullName : member.DeclaringType.FullName + "." + member.Name;

                memberName = memberName
                    .Replace("/", ".")
                    .Replace('+', '.');

                memberTypeName =
                    TypeExtensions.IsAssignableToTypeName(memberType, "MethodDefinition", TypeNameStyle.Name) ? (memberName.EndsWith("..ctor") ? "Constructor" : "Method") :
                    TypeExtensions.IsAssignableToTypeName(memberType, "PropertyDefinition", TypeNameStyle.Name) ? "Property" :
                    "TypeInfo";
            }
            else
            {
                memberName = member is Type type && !string.IsNullOrEmpty(memberType.FullName) ?
                    type.FullName : member.DeclaringType.FullName + "." + member.Name;

                memberTypeName = (string)member.MemberType.ToString();
            }

            switch (memberTypeName)
            {
                case "Constructor":
                    memberName = memberName.Replace(".ctor", "#ctor");
                    goto case "Method";

                case "Method":
                    prefixCode = 'M';

                    Func<dynamic, string> parameterTypeSelector = p => (string)p.ParameterType.FullName;

                    var parameters = member is MethodBase ?
                        ((MethodBase)member).GetParameters().Select(x => x.ParameterType.FullName) :
                        (IEnumerable<string>)System.Linq.Enumerable.Select<dynamic, string>(member.Parameters, parameterTypeSelector);

                    var paramTypesList = string.Join(",", parameters
                        .Select(x => Regex
                            .Replace(x, "(`[0-9]+)|(, .*?PublicKeyToken=[0-9a-z]*)", string.Empty)
                            .Replace("[[", "{")
                            .Replace("]]", "}"))
                        .ToArray());

                    if (!string.IsNullOrEmpty(paramTypesList))
                    {
                        memberName += "(" + paramTypesList + ")";
                    }

                    break;

                case "Event":
                    prefixCode = 'E';
                    break;

                case "Field":
                    prefixCode = 'F';
                    break;

                case "NestedType":
                    memberName = memberName.Replace('+', '.');
                    goto case "TypeInfo";

                case "TypeInfo":
                    prefixCode = 'T';
                    break;

                case "Property":
                    prefixCode = 'P';
                    break;

                default:
                    throw new ArgumentException("Unknown member type.", "member");
            }

            return string.Format("{0}:{1}", prefixCode, memberName.Replace("+", "."));
        }
        }
    }
}
