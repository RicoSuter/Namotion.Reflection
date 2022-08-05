//-----------------------------------------------------------------------
// <copyright file="XmlDocumentationExtensions.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using Namotion.Reflection.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("Namotion.Reflection.Cecil, PublicKey=0024000004800000940000000602000000240000525341310004000001000100337d8a0b73ac39048dc55d8e48dd86dcebd0af16aa514c73fbf5f283a8e94d7075b4152e5621e18d234bf7a5aafcb6683091f79d87b80c3be3e806f688e6f940adf92b28cedf1f8f69aa443699c235fa049204b56b83d94f599dd9800171f28e45ab74351acab17d889cd65961354d2f6405bddb9e896956e69e60033c2574f1")]

namespace Namotion.Reflection
{
    /// <summary>Provides extension methods for reading XML comments from reflected members.</summary>
    public static class XmlDocs
    {
        /// <summary>
        /// Clears the cache.
        /// </summary>
        public static void ClearCache()
        {
            XmlDocsExtensions.ClearCache();
        }
    }

    /// <summary>Provides extension methods for reading XML comments from reflected members.</summary>
    public static class XmlDocsExtensions
    {
        private static readonly ConcurrentDictionary<string, CachingXDocument?> Cache =
            new(StringComparer.OrdinalIgnoreCase);

        internal static void ClearCache()
        {
            Cache.Clear();
        }

        #region Contextual

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <param name="resolveExternalXmlDocs">Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsSummary(this CachedType type, bool resolveExternalXmlDocs = true)
        {
            return type.Type.GetXmlDocsSummary(resolveExternalXmlDocs);
        }

        /// <summary>Returns the contents of the "remarks" XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <param name="resolveExternalXmlDocs">Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsRemarks(this CachedType type, bool resolveExternalXmlDocs = true)
        {
            return type.Type.GetXmlDocsRemarks(resolveExternalXmlDocs);
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="resolveExternalXmlDocs">Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsTag(this CachedType type, string tagName, bool resolveExternalXmlDocs = true)
        {
            return type.Type.GetXmlDocsTag(tagName, resolveExternalXmlDocs);
        }

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="resolveExternalXmlDocs">Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsSummary(this ContextualMemberInfo member, bool resolveExternalXmlDocs = true)
        {
            return member.MemberInfo.GetXmlDocsSummary(resolveExternalXmlDocs);
        }

        /// <summary>Returns the contents of the "remarks" XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="resolveExternalXmlDocs">Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsRemarks(this ContextualMemberInfo member, bool resolveExternalXmlDocs = true)
        {
            return member.MemberInfo.GetXmlDocsRemarks(resolveExternalXmlDocs);
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="resolveExternalXmlDocs">Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsTag(this ContextualMemberInfo member, string tagName, bool resolveExternalXmlDocs = true)
        {
            return member.MemberInfo.GetXmlDocsTag(tagName, resolveExternalXmlDocs);
        }

        /// <summary>Returns the contents of the "returns" or "param" XML documentation tag for the specified parameter.</summary>
        /// <param name="parameter">The reflected parameter or return info.</param>
        /// <param name="resolveExternalXmlDocs">Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.</param>
        /// <returns>The contents of the "returns" or "param" tag.</returns>
        public static string GetXmlDocs(this ContextualParameterInfo parameter, bool resolveExternalXmlDocs = true)
        {
            return parameter.ParameterInfo.GetXmlDocs(resolveExternalXmlDocs);
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="resolveExternalXmlDocs">Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static XElement? GetXmlDocsElement(this ContextualMemberInfo member, bool resolveExternalXmlDocs = true)
        {
            return member.MemberInfo.GetXmlDocsElement(resolveExternalXmlDocs);
        }

        #endregion

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <param name="resolveExternalXmlDocs">Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsSummary(this Type type, bool resolveExternalXmlDocs = true)
        {
            return GetXmlDocsTag((MemberInfo)type.GetTypeInfo(), "summary", resolveExternalXmlDocs);
        }

        /// <summary>Returns the contents of the "remarks" XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <param name="resolveExternalXmlDocs">Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsRemarks(this Type type, bool resolveExternalXmlDocs = true)
        {
            return GetXmlDocsTag((MemberInfo)type.GetTypeInfo(), "remarks", resolveExternalXmlDocs);
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="resolveExternalXmlDocs">Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsTag(this Type type, string tagName, bool resolveExternalXmlDocs = true)
        {
            return GetXmlDocsTag((MemberInfo)type.GetTypeInfo(), tagName, resolveExternalXmlDocs);
        }

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="resolveExternalXmlDocs">Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsSummary(this MemberInfo member, bool resolveExternalXmlDocs = true)
        {
            var docs = GetXmlDocsTag(member, "summary", resolveExternalXmlDocs);

            if (string.IsNullOrEmpty(docs) && member is PropertyInfo propertyInfo)
            {
                return propertyInfo.GetXmlDocsRecordPropertySummary(resolveExternalXmlDocs);
            }

            return docs;
        }

        /// <summary>Returns the contents of the "remarks" XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="resolveExternalXmlDocs">Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsRemarks(this MemberInfo member, bool resolveExternalXmlDocs = true)
        {
            return GetXmlDocsTag(member, "remarks", resolveExternalXmlDocs);
        }

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <param name="pathToXmlFile">The path to the XML documentation file.</param>
        /// <param name="resolveExternalXmlDocs">Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static XElement? GetXmlDocsElement(this Type type, string pathToXmlFile, bool resolveExternalXmlDocs = true)
        {
            return ((MemberInfo)type.GetTypeInfo()).GetXmlDocsElement(pathToXmlFile, resolveExternalXmlDocs);
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="resolveExternalXmlDocs">Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static XElement? GetXmlDocsElement(this MemberInfo member, bool resolveExternalXmlDocs = true)
        {
            if (DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
            {
                return null;
            }

            var assemblyName = member.Module.Assembly.GetName();
            if (IsAssemblyIgnored(assemblyName, resolveExternalXmlDocs))
            {
                return null;
            }

            var documentationPath = GetXmlDocsPath(member.Module.Assembly, resolveExternalXmlDocs);
            return GetXmlDocsElement(member, documentationPath!, resolveExternalXmlDocs);
        }

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="pathToXmlFile">The path to the XML documentation file.</param>
        /// <param name="resolveExternalXmlDocs">Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static XElement? GetXmlDocsElement(this MemberInfo member, string pathToXmlFile, bool resolveExternalXmlDocs = true)
        {
            try
            {
                if (DynamicApis.SupportsXPathApis == false
                    || DynamicApis.SupportsFileApis == false
                    || DynamicApis.SupportsPathApis == false)
                {
                    return null;
                }

                var assemblyName = member.Module.Assembly.GetName();
                var document = TryGetXmlDocsDocument(assemblyName, pathToXmlFile, resolveExternalXmlDocs);
                if (document == null)
                {
                    return null;
                }

                var element = GetXmlDocsElement(member, document);
                ReplaceInheritdocElements(member, element, resolveExternalXmlDocs);
                return element;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="resolveExternalXmlDocs">Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsTag(this MemberInfo member, string tagName, bool resolveExternalXmlDocs = true)
        {
            if (DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
            {
                return string.Empty;
            }

            _ = member ?? throw new ArgumentNullException(nameof(member));
            _ = tagName ?? throw new ArgumentNullException(nameof(tagName));

            var assemblyName = member.Module.Assembly.GetName();
            if (IsAssemblyIgnored(assemblyName, resolveExternalXmlDocs))
            {
                return string.Empty;
            }

            var documentationPath = GetXmlDocsPath(member.Module.Assembly, resolveExternalXmlDocs);
            var element = GetXmlDocsElement(member, documentationPath!, resolveExternalXmlDocs);
            return ToXmlDocsContent(element?.Element(tagName));
        }

        /// <summary>Returns the property summary of a Record type which is read from the param tag on the type.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="resolveExternalXmlDocs">Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.</param>
        /// <returns>The contents of the "param" tag of the Record property.</returns>
        public static string GetXmlDocsRecordPropertySummary(this PropertyInfo member, bool resolveExternalXmlDocs = true)
        {
            if (DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
            {
                return string.Empty;
            }

            _ = member ?? throw new ArgumentNullException(nameof(member));

            var assemblyName = member.Module.Assembly.GetName();
            if (IsAssemblyIgnored(assemblyName, resolveExternalXmlDocs))
            {
                return string.Empty;
            }

            var documentationPath = GetXmlDocsPath(member.Module.Assembly, resolveExternalXmlDocs);
            var parentElement = GetXmlDocsElement(member.DeclaringType.GetTypeInfo(), documentationPath!, resolveExternalXmlDocs);
            var paramElement = parentElement?
                .Elements("param")?
                .FirstOrDefault(x => x.Attribute("name")?
                .Value == member.Name);

            return paramElement != null ? ToXmlDocsContent(paramElement) : string.Empty;
        }

        /// <summary>Returns the contents of the "returns" or "param" XML documentation tag for the specified parameter.</summary>
        /// <param name="parameter">The reflected parameter or return info.</param>
        /// <param name="resolveExternalXmlDocs">Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.</param>
        /// <returns>The contents of the "returns" or "param" tag.</returns>
        public static string GetXmlDocs(this ParameterInfo parameter, bool resolveExternalXmlDocs = true)
        {
            if (DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
            {
                return string.Empty;
            }

            var assemblyName = parameter.Member.Module.Assembly.GetName();
            if (IsAssemblyIgnored(assemblyName, resolveExternalXmlDocs))
            {
                return string.Empty;
            }

            var documentationPath = GetXmlDocsPath(parameter.Member.Module.Assembly, resolveExternalXmlDocs);
            var element = GetXmlDocs(parameter, documentationPath, resolveExternalXmlDocs);
            return ToXmlDocsContent(element);
        }

        /// <summary>Returns the contents of the "returns" or "param" XML documentation tag for the specified parameter.</summary>
        /// <param name="parameter">The reflected parameter or return info.</param>
        /// <param name="pathToXmlFile">The path to the XML documentation file.</param>
        /// <param name="resolveExternalXmlDocs">Specifies whether tho resolve the XML Docs from the NuGet cache or .NET SDK directory.</param>
        /// <returns>The contents of the "returns" or "param" tag.</returns>
        public static XElement? GetXmlDocsElement(this ParameterInfo parameter, string pathToXmlFile, bool resolveExternalXmlDocs = true)
        {
            try
            {
                if (pathToXmlFile == null || DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
                {
                    return null;
                }

                return GetXmlDocs(parameter, pathToXmlFile, resolveExternalXmlDocs);
            }
            catch
            {
                return null;
            }
        }

        // prevent array allocations on old runtimes
        private static readonly char[] ToXmlDocsContentTrimChars = { '!', ':' };

        /// <summary>Converts the given XML documentation <see cref="XElement"/> to text.</summary>
        /// <param name="element">The XML element.</param>
        /// <returns>The text</returns>
        public static string ToXmlDocsContent(this XElement? element)
        {
            if (element != null)
            {
                var value = new StringBuilder();
                foreach (var node in element.Nodes())
                {
                    if (node is XElement e)
                    {
                        if (e.Name == "see")
                        {
                            var attribute = e.Attribute("langword");
                            if (attribute != null)
                            {
                                value.Append(attribute.Value);
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(e.Value))
                                {
                                    value.Append(e.Value);
                                }
                                else
                                {
                                    attribute = e.Attribute("cref");
                                    if (attribute != null)
                                    {
                                        var trimmed = attribute.Value.Trim(ToXmlDocsContentTrimChars).Trim();
                                        trimmed = trimmed.FirstToken('(');
                                        trimmed = trimmed.LastToken('.');
                                        value.Append(trimmed);
                                    }
                                    else
                                    {
                                        attribute = e.Attribute("href");
                                        if (attribute != null)
                                        {
                                            value.Append(attribute.Value);
                                        }
                                    }
                                }
                            }
                        }
                        else if (e.Name == "paramref")
                        {
                            var nameAttribute = e.Attribute("name");
                            value.Append(nameAttribute?.Value ?? e.Value);
                        }
                        else
                        {
                            value.Append(e.Value);
                        }
                    }
                    else if (node is XText text)
                    {
                        // take value directly without costly ToString()
                        value.Append(text.Value);
                    }
                    else
                    {
                        value.Append(node);
                    }
                }

                return RemoveLineBreakWhiteSpaces(value.ToString());
            }

            return string.Empty;
        }

        private static XElement? GetXmlDocs(this ParameterInfo parameter, string? pathToXmlFile, bool resolveExternalXmlDocs = true)
        {
            try
            {
                if (DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
                {
                    return null;
                }

                var assemblyName = parameter.Member.Module.Assembly.GetName();
                var document = TryGetXmlDocsDocument(assemblyName, pathToXmlFile, resolveExternalXmlDocs);
                if (document == null)
                {
                    return null;
                }

                return GetXmlDocsElement(parameter, document, resolveExternalXmlDocs);
            }
            catch
            {
                return null;
            }
        }

        private static CachingXDocument? TryGetXmlDocsDocument(AssemblyName assemblyName, string? pathToXmlFile, bool resolveExternalXmlDocs)
        {
            if (Cache.TryGetValue(GetCacheKey(assemblyName.FullName, resolveExternalXmlDocs), out var document))
            {
                return document;
            }

            if (pathToXmlFile is null)
            {
                return null;
            }

            if (DynamicApis.FileExists(pathToXmlFile) == false)
            {
                Cache[GetCacheKey(assemblyName.FullName, resolveExternalXmlDocs)] = null;
                return null;
            }

            document = new CachingXDocument(pathToXmlFile);
            Cache[GetCacheKey(assemblyName.FullName, resolveExternalXmlDocs)] = document;

            return document;
        }

        private static bool IsAssemblyIgnored(AssemblyName assemblyName, bool resolveExternalXmlDocs)
        {
            return Cache.TryGetValue(GetCacheKey(assemblyName.FullName, resolveExternalXmlDocs), out var document) && document == null;
        }

        private static XElement? GetXmlDocsElement(this MemberInfo member, CachingXDocument xml)
        {
            var name = GetMemberElementName(member);
            return xml.GetXmlDocsElement(name);
        }

        internal static XElement? GetXmlDocsElement(this XDocument xml, string name)
        {
            return CachingXDocument.GetXmlDocsElement(xml, name);
        }

        private static XElement? GetXmlDocsElement(this ParameterInfo parameter, CachingXDocument xml, bool resolveExternalXmlDocs = true)
        {
            var name = GetMemberElementName(parameter.Member);
            var element = xml.GetXmlDocsElement(name);
            if (element != null)
            {
                ReplaceInheritdocElements(parameter.Member, element, resolveExternalXmlDocs);

                IEnumerable result;
                if (parameter.IsRetval || string.IsNullOrEmpty(parameter.Name))
                {
                    result = element.Elements("returns");
                }
                else
                {
                    result = element.Elements("param").Where(x => x.Attribute("name")?.Value == parameter.Name);
                }

                return result.OfType<XElement>().FirstOrDefault();
            }

            return null;
        }

        private static void ReplaceInheritdocElements(
            this MemberInfo member,
            XElement? element,
            bool resolveExternalXmlDocs = true)
        {
#if !NET40
            if (element == null)
            {
                return;
            }

            var children = element.Nodes().ToList();
            foreach (var child in children.OfType<XElement>())
            {
                if (child.Name.LocalName.ToLowerInvariant() == "inheritdoc")
                {
#if !NETSTANDARD1_0
                    // if this a class/type
                    if (child.HasAttributes && (member.MemberType is MemberTypes.TypeInfo or MemberTypes.Property))
                    {
                        ProcessInheritDocTypeElements(member, child);
                        continue;
                    }
#endif
                    var baseType = member.DeclaringType.GetTypeInfo().BaseType;
                    var baseMember = baseType?.GetTypeInfo().DeclaredMembers.SingleOrDefault(m => m.Name == member.Name);
                    if (baseMember != null)
                    {
                        var baseDoc = baseMember.GetXmlDocsElement(resolveExternalXmlDocs);
                        if (baseDoc != null)
                        {
                            var nodes = baseDoc.Nodes().OfType<object>().ToArray();
                            child.ReplaceWith(nodes);
                        }
                        else
                        {
                            ProcessInheritdocInterfaceElements(member, child, resolveExternalXmlDocs);
                        }
                    }
                    else
                    {
                        ProcessInheritdocInterfaceElements(member, child, resolveExternalXmlDocs);
                    }
                }
            }
        }

        private static void ProcessInheritdocInterfaceElements(this MemberInfo member, XElement child, bool resolveExternalXmlDocs = true)
        {
            if (member.DeclaringType.GetTypeInfo().ImplementedInterfaces == null)
            {
                return;
            }

            foreach (var baseInterface in member.DeclaringType.GetTypeInfo().ImplementedInterfaces)
            {
                var baseMember = baseInterface?.GetTypeInfo().DeclaredMembers.SingleOrDefault(m => m.Name == member.Name);
                if (baseMember != null)
                {
                    var baseDoc = baseMember.GetXmlDocsElement(resolveExternalXmlDocs);
                    if (baseDoc != null)
                    {
                        var nodes = baseDoc.Nodes().OfType<object>().ToArray();
                        child.ReplaceWith(nodes);
                    }
                }
            }
#endif
        }

        private static readonly char[] RemoveLineBreakWhiteSpacesTrimChars = { '\n' };
        private static readonly Regex LineBreakRegex = new("(\\n[ \\t]*)", (RegexOptions)8); // Compiled

        private static string RemoveLineBreakWhiteSpaces(string? documentation)
        {
            if (string.IsNullOrEmpty(documentation))
            {
                return string.Empty;
            }

            documentation = "\n" + documentation!.Replace("\r", string.Empty).Trim(RemoveLineBreakWhiteSpacesTrimChars);

            var whitespace = LineBreakRegex.Match(documentation).Value;
            documentation = documentation.Replace(whitespace, "\n");

            return documentation.Trim(RemoveLineBreakWhiteSpacesTrimChars);
        }

        /// <exception cref="ArgumentException">Unknown member type.</exception>
        internal static string GetMemberElementName(dynamic member)
        {
            char prefixCode;
            string memberName;
            string memberTypeName;

            if (member is null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            if (member is MemberInfo memberInfo &&
                memberInfo.DeclaringType != null &&
                memberInfo.DeclaringType.GetTypeInfo().IsGenericType)
            {
                // Resolve member with generic arguments (Ts instead of actual types)
                if (member is PropertyInfo propertyInfo)
                {
                    member = propertyInfo.DeclaringType!.GetRuntimeProperty(propertyInfo.Name)!;
                }
                else
                {
                    member = ((dynamic)member).Module.ResolveMember(((dynamic)member).MetadataToken);
                }
            }

            var memberType = ((object)member).GetType();
            if (memberType.FullName!.Contains(".Cecil."))
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
                    type.FullName.FirstToken('[') : ((string)member.DeclaringType.FullName).FirstToken('[') + "." + member.Name;

                memberTypeName = (string)member.MemberType.ToString();
            }

            switch (memberTypeName)
            {
                case "Constructor":
                    memberName = memberName.Replace(".ctor", "#ctor");
                    goto case "Method";

                case "Method":
                    prefixCode = 'M';

                    Func<dynamic, string> parameterTypeSelector = p =>
                        p.ParameterType.ContainsGenericParameter ?
                        (ObjectExtensions.HasProperty(p.ParameterType, "GenericArguments") && p.ParameterType.GenericArguments.Count > 0 ?
                            ((string)p.ParameterType.FullName).FirstToken('`') + "{" + string.Join(",", ((ICollection)p.ParameterType.GenericArguments).Cast<dynamic>().Select(u => "||" + u.Position)) + "}" :
                            "||" + p.ParameterType.Position) :
                        (string)p.ParameterType.FullName;

                    var parameters = member is MethodBase ?
                        ((MethodBase)member).GetParameters().Select(x =>
                            x.ParameterType.FullName ??
                            (((dynamic)x.ParameterType).GenericTypeArguments.Length > 0 ?
                                x.ParameterType.Namespace + "." + x.ParameterType.Name.FirstToken('`') +
                                    "{" + string.Join(",", ((Type[])((dynamic)x.ParameterType).GenericTypeArguments)
                                        .Select(a => a.IsGenericParameter ?
                                            "||" + a.GenericParameterPosition.ToString() :
                                            a.Namespace + "." + a.Name + "[[||0]]")) // special case for Expression<Func...>>
                                    + "}" :
                                "||" + x.ParameterType.GenericParameterPosition)) :
                        (IEnumerable<string>)System.Linq.Enumerable.Select<dynamic, string>(member.Parameters, parameterTypeSelector);

                    var paramTypesList = string.Join(",", parameters
                        .Select(x => Regex
                            .Replace(x, "(`[0-9]+)|(, .*?PublicKeyToken=[0-9a-z]*)", string.Empty)
                            .Replace("],[", ",")
                            .Replace("||", "`")
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

        /// <summary>
        /// Gets the file path to the XML docs for the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="resolveExternalXmlDocs">Specifies whether to resolve external XML docs.</param>
        /// <returns>The file path or null if not found.</returns>
#if NETSTANDARD1_0
        // ReSharper disable once MemberCanBePrivate.Global
        public static string? GetXmlDocsPath(dynamic? assembly, bool resolveExternalXmlDocs = true)
        {
#else
        public static string? GetXmlDocsPath(Assembly? assembly, bool resolveExternalXmlDocs = true)
        {
#endif
            try
            {
                if (assembly == null)
                {
                    return null;
                }

                AssemblyName assemblyName = assembly.GetName();
                if (string.IsNullOrEmpty(assemblyName.Name))
                {
                    return null;
                }

                var assemblyFullName = assemblyName.FullName;
                if (Cache.ContainsKey(GetCacheKey(assemblyFullName, resolveExternalXmlDocs)))
                {
                    // Path not needed as document already in cache
                    return null;
                }

                try
                {
                    string? path;
                    if (!string.IsNullOrEmpty(assembly.Location))
                    {
                        path = GetPathByOs(assembly, assemblyName);

                        if (DynamicApis.FileExists(path))
                        {
                            return path;
                        }
                    }

                    if (ObjectExtensions.HasProperty(assembly, "CodeBase"))
                    {
                        var codeBase = (string)assembly.CodeBase;
                        if (!string.IsNullOrEmpty(codeBase))
                        {
                            path = DynamicApis.PathCombine(DynamicApis.PathGetDirectoryName(codeBase
                                .Replace("file:///", string.Empty)), assemblyName.Name + ".xml")
                                .Replace("file:\\", string.Empty);

                            if (DynamicApis.FileExists(path))
                            {
                                return path;
                            }
                        }
                    }

                    var currentDomain = Type.GetType("System.AppDomain")?.GetRuntimeProperty("CurrentDomain")?.GetValue(null);
                    if (currentDomain?.HasProperty("BaseDirectory") == true)
                    {
                        var baseDirectory = currentDomain.TryGetPropertyValue("BaseDirectory", "");
                        if (!string.IsNullOrEmpty(baseDirectory))
                        {
                            path = DynamicApis.PathCombine(baseDirectory!, assemblyName.Name + ".xml");
                            if (DynamicApis.FileExists(path))
                            {
                                return path;
                            }

                            path = DynamicApis.PathCombine(baseDirectory!, "bin/" + assemblyName.Name + ".xml");
                            if (DynamicApis.FileExists(path))
                            {
                                return path;
                            }
                        }
                    }

                    var currentDirectory = DynamicApis.DirectoryGetCurrentDirectory();
                    path = DynamicApis.PathCombine(currentDirectory, assembly.GetName().Name + ".xml");
                    if (DynamicApis.FileExists(path))
                    {
                        return path;
                    }

                    path = DynamicApis.PathCombine(currentDirectory, "bin/" + assembly.GetName().Name + ".xml");
                    if (DynamicApis.FileExists(path))
                    {
                        return path;
                    }

                    if (resolveExternalXmlDocs)
                    {
                        dynamic? executingAssembly = typeof(Assembly)
                            .GetRuntimeMethod("GetExecutingAssembly", new Type[0])?
                            .Invoke(null, new object[0]);

                        if (!string.IsNullOrEmpty(executingAssembly?.Location))
                        {
                            var assemblyDirectory = DynamicApis.PathGetDirectoryName((string)executingAssembly!.Location);
                            path = GetXmlDocsPathFromNuGetCacheOrDotNetSdk(assemblyDirectory, assemblyName);
                            if (path != null && DynamicApis.FileExists(path))
                            {
                                return path;
                            }
                        }
                    }

                    Cache[GetCacheKey(assemblyFullName, resolveExternalXmlDocs)] = null;
                    return null;
                }
                catch
                {
                    Cache[GetCacheKey(assemblyFullName, resolveExternalXmlDocs)] = null;
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

#if !NETSTANDARD1_0

        /// <summary>
        /// Get Type from a referencing string such as <c>!:MyType</c> or <c>!:MyType.MyProperty</c>
        /// </summary>
        private static void ProcessInheritDocTypeElements(this MemberInfo member, XElement child)
        {
            var referencedTypeXmlId = child.Attribute("cref")?.Value;
            if (referencedTypeXmlId is not null)
            {
                Match? matches;
                string? referencedTypeName;
                MemberInfo? referencedType = null;
                Assembly? docAssembly = null;
                switch (referencedTypeXmlId[0])
                {
                    case 'P':
                        matches = Regex.Match(
                            referencedTypeXmlId,
                            @"(?<FullName>(?<FullTypeName>(?<AssemblyName>[a-zA-Z.]*)\.(?<TypeName>[a-zA-Z]*))\.(?<MemberName>[a-zA-Z]*))");
                        referencedTypeName = matches.Groups["FullTypeName"].Value;
                        break;

                    default:
                        matches = Regex.Match(
                            referencedTypeXmlId,
                            @"[A-Z]:(?<FullName>(?<Namespace>[a-zA-Z.]*)\.(?<TypeName>[a-zA-Z]*))");
                        referencedTypeName = matches.Groups["FullName"].Value;
                        break;
                }

                if (docAssembly is null && referencedTypeName is not null)
                {
                    docAssembly = member.Module.Assembly;
                    referencedType = docAssembly.GetType(referencedTypeName);
                    // check member's assembly first
                    if (referencedType is null)
                    {
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            // limit the Assemblies that are searched by doing a basic name check.
                            if (referencedTypeXmlId.Contains(assembly.GetName().Name))
                            {
                                referencedType = GetTypeByXmlDocTypeName(referencedTypeName, assembly);
                                if (referencedType != null)
                                {
                                    docAssembly = assembly;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (referencedType is null ||
                    docAssembly is null)
                {
                    return;
                }

                var referencedDocs = TryGetXmlDocsDocument(
                    docAssembly.GetName(),
                    GetXmlDocsPath(docAssembly),
                    true)?.GetXmlDocsElement(referencedTypeXmlId);

                /* for Record types ( as opposed to Class types ) the above lookup will fail for parameters defined in
                 * shorthand form on the Constructor. Constructor-defined Properties will show up on the constructor
                 * as <param name="PropertyName">...</param> rather than have the xml doc member element as a typical
                 * property would.
                */
                if (referencedDocs is null && referencedType.MemberType == MemberTypes.Property)
                {
                    var documentationPath = GetXmlDocsPath(member.Module.Assembly);
                    if (documentationPath is null)
                    {
                        return;
                    }

                    var parentElement = GetXmlDocsElement(
                        referencedType.DeclaringType.GetTypeInfo(),
                        documentationPath);

                    referencedDocs = parentElement?
                        .Elements("param")?
                        .FirstOrDefault(x => x.Attribute("name")?.Value == referencedType.Name);

                    // for records, replace node with the entirety of the found docs. So the whole <param> tag.
                    child.ReplaceWith(referencedDocs);
                    return;
                }

                if (referencedDocs != null)
                {
                    var nodes = referencedDocs.Nodes().OfType<object>().ToArray();
                    child.ReplaceWith(nodes);
                }
            }
        }

        private static Type? GetTypeByXmlDocTypeName(string xmlDocTypeName, Assembly assembly)
        {
            var assemblyTypeNames = assembly.GetTypes()
                .Select(type => new KeyValuePair<string, Type>(NormalizeTypeName(type.FullName!), type))
                .ToDictionary(x => x.Key, x => x.Value);
            assemblyTypeNames.TryGetValue(NormalizeTypeName(xmlDocTypeName), out var resultType);
            return resultType;
        }

        private static string NormalizeTypeName(string typeName)
        {
            return typeName
                .Replace(".", string.Empty)
                .Replace("+", string.Empty);
        }
#endif

        private static string? GetPathByOs(dynamic? assembly, AssemblyName assemblyName)
        {
#if NETSTANDARD2_0
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var fullAssemblyVersion = (assembly as Assembly)?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
                if (fullAssemblyVersion == null)
                    return null;

                var version = new Version(fullAssemblyVersion);
                // NuGet cache only has the Major.Minor.Build version
                var truncatedVersion = $"{version.Major}.{version.Minor}.{version.Build}";
                // Path is like /Users/usernamehere/.nuget/packages/Microsoft.AspNetCore.Mvc.Core/2.2.1
                var macOsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    ".nuget",
                    "packages",
                    assemblyName.Name,
                    truncatedVersion);

                if (!Directory.Exists(macOsPath))
                    return null;

                var file = Directory.GetFiles(macOsPath, "*.xml", SearchOption.AllDirectories)
                    .OrderByDescending(f => f)
                    .FirstOrDefault();
                return file;
            }
#endif
            return GetXmlAssemblyFilePathForWindows(assembly, assemblyName);
        }

        private static string GetXmlAssemblyFilePathForWindows(dynamic? assembly, AssemblyName assemblyName)
        {
            var assemblyDirectory = DynamicApis.PathGetDirectoryName((string)assembly.Location);
            return DynamicApis.PathCombine(assemblyDirectory, assemblyName.Name + ".xml");
        }

        private static readonly Regex runtimeConfigRegex = new Regex("\"((.*?)((\\\\\\\\)|(////))(.*?))\"", RegexOptions.IgnoreCase);

        private static string? GetXmlDocsPathFromNuGetCacheOrDotNetSdk(string assemblyDirectory, AssemblyName assemblyName)
        {
            var configs = DynamicApis.DirectoryGetFiles(assemblyDirectory, "*.runtimeconfig.dev.json");
            if (configs.Any())
            {
                try
                {
                    // Retrieve NuGet package cache directories from *.runtimeconfig.dev.json
                    var json = DynamicApis.FileReadAllText(configs.First());
                    var matches = runtimeConfigRegex.Matches(json);
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            var path = match.Groups[1].Value
                                .Replace("\\\\", "/")
                                .Replace("//", "/")
                                .Replace("\\|arch|", "")
                                .Replace("\\|tfm|", "")
                                .Replace("/|arch|", "")
                                .Replace("/|tfm|", "");

                            // From NuGet cache
                            if (DynamicApis.DirectoryExists(path))
                            {
                                try
                                {
                                    var packagePath = DynamicApis.PathCombine(path, assemblyName.Name + "/" + assemblyName.Version.ToString(3));
                                    if (DynamicApis.DirectoryExists(packagePath))
                                    {
                                        var file = DynamicApis.DirectoryGetAllFiles(packagePath, assemblyName.Name + ".xml")
                                            .OrderByDescending(f => f)
                                            .FirstOrDefault();

                                        if (file is not null)
                                        {
                                            return file;
                                        }
                                    }
                                }
                                catch
                                {
                                }
                            }

                            // From .NET SDK, e.g. C:\Program Files\dotnet\packs\Microsoft.AspNetCore.App.Ref\3.1.3\ref\netcoreapp3.1
                            if (path.Contains("/dotnet/sdk"))
                            {
                                while ((path = DynamicApis.PathGetDirectoryName(path).Replace('\\', '/')) != null)
                                {
                                    if (path.EndsWith("/dotnet"))
                                    {
                                        try
                                        {
                                            path = DynamicApis.PathCombine(path, "packs");
                                            var search = "/" + assemblyName.Version.ToString(2);
                                            var file = DynamicApis.DirectoryGetAllFiles(path, assemblyName.Name + ".xml")
                                               .Where(f => f.Replace('\\', '/').Contains(search))
                                               .OrderByDescending(f => f)
                                               .FirstOrDefault();

                                            if (file is not null)
                                            {
                                                return file;
                                            }
                                        }
                                        catch
                                        {
                                        }

                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            // Retrieve NuGet packages from project.nuget.cache locations
            var nuGetCacheFile = DynamicApis.PathCombine(assemblyDirectory, "../../obj/project.nuget.cache");
            if (DynamicApis.FileExists(nuGetCacheFile))
            {
                return GetXmlDocsPathFromNuGetCacheFile(nuGetCacheFile, assemblyName);
            }

            nuGetCacheFile = DynamicApis.PathCombine(assemblyDirectory, "../../../obj/project.nuget.cache");
            if (DynamicApis.FileExists(nuGetCacheFile))
            {
                return GetXmlDocsPathFromNuGetCacheFile(nuGetCacheFile, assemblyName);
            }

            return null;
        }

        private static string? GetXmlDocsPathFromNuGetCacheFile(string nuGetCacheFile, AssemblyName assemblyName)
        {
            try
            {
                var json = DynamicApis.FileReadAllText(nuGetCacheFile);
                var matches = Regex.Matches(json, $"\"((.*?){assemblyName.Name}((\\\\\\\\)|(////)){assemblyName.Version.ToString(3)})((\\\\\\\\)|(////))(.*?)\"", RegexOptions.IgnoreCase);
                if (matches.Count > 0)
                {
                    var files = DynamicApis.DirectoryGetAllFiles(matches[0].Groups[1].Value.Replace("\\\\", "\\").Replace("//", "/"), assemblyName.Name + ".xml");
                    if (files.Any())
                    {
                        return files.Last();
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static string GetCacheKey(string assemblyFullName, bool resolveExternalXmlDocs)
        {
            return $"{assemblyFullName}:{resolveExternalXmlDocs}";
        }
    }
}
