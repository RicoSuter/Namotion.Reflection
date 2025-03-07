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
        /// <param name="options">The XML docs reading and formatting options.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsSummary(this CachedType type, XmlDocsOptions? options = null)
        {
            return type.Type.GetXmlDocsSummary(options);
        }

        /// <summary>Returns the contents of the "remarks" XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <param name="options">The XML docs reading and formatting options.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsRemarks(this CachedType type, XmlDocsOptions? options = null)
        {
            return type.Type.GetXmlDocsRemarks(options);
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="options">The XML docs reading and formatting options.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsTag(this CachedType type, string tagName, XmlDocsOptions? options = null)
        {
            return type.Type.GetXmlDocsTag(tagName, options);
        }

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="options">The XML docs reading and formatting options.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsSummary(this ContextualMemberInfo member, XmlDocsOptions? options = null)
        {
            return member.MemberInfo.GetXmlDocsSummary(options);
        }

        /// <summary>Returns the contents of the "remarks" XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="options">The XML docs reading and formatting options.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsRemarks(this ContextualMemberInfo member, XmlDocsOptions? options = null)
        {
            return member.MemberInfo.GetXmlDocsRemarks(options);
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="options">The XML docs reading and formatting options.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsTag(this ContextualMemberInfo member, string tagName, XmlDocsOptions? options = null)
        {
            return member.MemberInfo.GetXmlDocsTag(tagName, options);
        }

        /// <summary>Returns the contents of the "returns" or "param" XML documentation tag for the specified parameter.</summary>
        /// <param name="parameter">The reflected parameter or return info.</param>
        /// <param name="options">The XML docs reading and formatting options.</param>
        /// <returns>The contents of the "returns" or "param" tag.</returns>
        public static string GetXmlDocs(this ContextualParameterInfo parameter, XmlDocsOptions? options = null)
        {
            return parameter.ParameterInfo.GetXmlDocs(options);
        }

        #endregion

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <param name="options">The XML docs reading and formatting options.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsSummary(this Type type, XmlDocsOptions? options = null)
        {
            return GetXmlDocsTag((MemberInfo)type.GetTypeInfo(), XmlDocsKeys.SummaryElement, options);
        }

        /// <summary>Returns the contents of the "remarks" XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <param name="options">The XML docs reading and formatting options.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsRemarks(this Type type, XmlDocsOptions? options = null)
        {
            return GetXmlDocsTag((MemberInfo)type.GetTypeInfo(), XmlDocsKeys.RemarksElement, options);
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="options">The XML docs reading and formatting options.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsTag(this Type type, string tagName, XmlDocsOptions? options = null)
        {
            return GetXmlDocsTag((MemberInfo)type.GetTypeInfo(), tagName, options);
        }

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="options">The XML docs reading and formatting options.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsSummary(this MemberInfo member, XmlDocsOptions? options = null)
        {
            var docs = GetXmlDocsTag(member, XmlDocsKeys.SummaryElement, options);

            if (string.IsNullOrEmpty(docs) && member is PropertyInfo propertyInfo)
            {
                return propertyInfo.GetXmlDocsRecordPropertySummary(options);
            }

            return docs;
        }

        /// <summary>Returns the contents of the "remarks" XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="options">The XML docs reading and formatting options.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsRemarks(this MemberInfo member, XmlDocsOptions? options = null)
        {
            return GetXmlDocsTag(member, XmlDocsKeys.RemarksElement, options);
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="options">The XML docs reading and formatting options.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static XElement? GetXmlDocsElement(this MemberInfo member, XmlDocsOptions? options = null)
        {
            options = options ?? XmlDocsOptions.Default;

            if (DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
            {
                return null;
            }

            var assemblyName = member.Module.Assembly.GetName();
            if (IsAssemblyIgnored(assemblyName, options.ResolveExternalXmlDocs))
            {
                return null;
            }

            var documentationPath = GetXmlDocsPath(member.Module.Assembly, options);
            return GetXmlDocsElement(member, documentationPath!, options);
        }

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="pathToXmlFile">The path to the XML documentation file.</param>
        /// <param name="options">The XML docs reading and formatting options.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static XElement? GetXmlDocsElement(this MemberInfo member, string pathToXmlFile, XmlDocsOptions? options = null)
        {
            try
            {
                options = options ?? XmlDocsOptions.Default;

                if (DynamicApis.SupportsXPathApis == false
                    || DynamicApis.SupportsFileApis == false
                    || DynamicApis.SupportsPathApis == false)
                {
                    return null;
                }

                var assemblyName = member.Module.Assembly.GetName();
                var document = TryGetXmlDocsDocument(assemblyName, pathToXmlFile, options.ResolveExternalXmlDocs);
                if (document == null)
                {
                    return null;
                }

                var element = GetXmlDocsElement(member, document);
                ReplaceInheritdocElements(member, element, options);
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
        /// <param name="options">The XML docs reading and formatting options.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsTag(this MemberInfo member, string tagName, XmlDocsOptions? options = null)
        {
            options = options ?? XmlDocsOptions.Default;

            if (DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
            {
                return string.Empty;
            }

            _ = member ?? throw new ArgumentNullException(nameof(member));
            _ = tagName ?? throw new ArgumentNullException(nameof(tagName));

            var assemblyName = member.Module.Assembly.GetName();
            if (IsAssemblyIgnored(assemblyName, options.ResolveExternalXmlDocs))
            {
                return string.Empty;
            }

            var documentationPath = GetXmlDocsPath(member.Module.Assembly, options);
            var element = GetXmlDocsElement(member, documentationPath!, options);
            return ToXmlDocsContent(element?.Element(tagName), options);
        }

        /// <summary>Returns the property summary of a Record type which is read from the param tag on the type.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="options">The XML docs reading and formatting options.</param>
        /// <returns>The contents of the "param" tag of the Record property.</returns>
        public static string GetXmlDocsRecordPropertySummary(this PropertyInfo member, XmlDocsOptions? options = null)
        {
            options = options ?? XmlDocsOptions.Default;

            if (DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
            {
                return string.Empty;
            }

            _ = member ?? throw new ArgumentNullException(nameof(member));

            var assemblyName = member.Module.Assembly.GetName();
            if (IsAssemblyIgnored(assemblyName, options.ResolveExternalXmlDocs))
            {
                return string.Empty;
            }

            var documentationPath = GetXmlDocsPath(member.Module.Assembly, options);
            var parentElement = GetXmlDocsElement(member.DeclaringType.GetTypeInfo(), documentationPath!, options);
            var paramElement = parentElement?
                .Elements(XmlDocsKeys.ParamElement)?
                .FirstOrDefault(x => x.Attribute(XmlDocsKeys.ParamNameAttribute)?
                .Value == member.Name);

            return paramElement != null ? ToXmlDocsContent(paramElement, options) : string.Empty;
        }

        /// <summary>Returns the contents of the "returns" or "param" XML documentation tag for the specified parameter.</summary>
        /// <param name="parameter">The reflected parameter or return info.</param>
        /// <param name="options">The XML docs reading and formatting options.</param>
        /// <returns>The contents of the "returns" or "param" tag.</returns>
        public static string GetXmlDocs(this ParameterInfo parameter, XmlDocsOptions? options = null)
        {
            options = options ?? XmlDocsOptions.Default;

            if (DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
            {
                return string.Empty;
            }

            var assemblyName = parameter.Member.Module.Assembly.GetName();
            if (IsAssemblyIgnored(assemblyName, options.ResolveExternalXmlDocs))
            {
                return string.Empty;
            }

            var documentationPath = GetXmlDocsPath(parameter.Member.Module.Assembly, options);
            var element = GetXmlDocs(parameter, documentationPath, options);
            return ToXmlDocsContent(element, options);
        }

        /// <summary>Returns the contents of the "returns" or "param" XML documentation tag for the specified parameter.</summary>
        /// <param name="parameter">The reflected parameter or return info.</param>
        /// <param name="pathToXmlFile">The path to the XML documentation file.</param>
        /// <param name="options">The XML docs reading and formatting options.</param>
        /// <returns>The contents of the "returns" or "param" tag.</returns>
        public static XElement? GetXmlDocsElement(this ParameterInfo parameter, string pathToXmlFile, XmlDocsOptions? options = null)
        {
            try
            {
                if (pathToXmlFile == null || DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
                {
                    return null;
                }

                return GetXmlDocs(parameter, pathToXmlFile, options ?? XmlDocsOptions.Default);
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
        /// <param name="options">The XML docs reading and formatting options.</param>
        /// <returns>The text</returns>
        public static string ToXmlDocsContent(this XElement? element, XmlDocsOptions? options = null)
        {
            options ??= XmlDocsOptions.Default;

            if (element != null)
            {
                var value = new StringBuilder();
                foreach (var node in element.Nodes())
                {
                    if (node is XElement e)
                    {
                        if (e.Name == XmlDocsKeys.SeeElement)
                        {
                            var attribute = e.Attribute(XmlDocsKeys.SeeLangwordAttribute);
                            if (attribute != null)
                            {
                                value.Append(attribute.Value);
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(e.Value))
                                {
                                    value.AppendFormattedElement(e, options.FormattingMode);
                                }
                                else
                                {
                                    attribute = e.Attribute(XmlDocsKeys.SeeCrefAttribute);
                                    if (attribute != null)
                                    {
                                        var trimmed = attribute.Value.Trim(ToXmlDocsContentTrimChars).Trim();
                                        trimmed = trimmed.FirstToken('(');
                                        trimmed = trimmed.LastToken('.');
                                        value.Append(trimmed);
                                    }
                                    else
                                    {
                                        attribute = e.Attribute(XmlDocsKeys.SeeHrefAttribute);
                                        if (attribute != null)
                                        {
                                            value.Append(attribute.Value);
                                        }
                                    }
                                }
                            }
                        }
                        else if (e.Name == XmlDocsKeys.ParamRefElement)
                        {
                            var nameAttribute = e.Attribute(XmlDocsKeys.ParamRefNameAttribute);
                            value.Append(nameAttribute?.Value ?? e.Value);
                        }
                        else
                        {
                            value.AppendFormattedElement(e, options.FormattingMode);
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

        private static XElement? GetXmlDocs(this ParameterInfo parameter, string? pathToXmlFile, XmlDocsOptions options)
        {
            try
            {
                if (DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
                {
                    return null;
                }

                var assemblyName = parameter.Member.Module.Assembly.GetName();
                var document = TryGetXmlDocsDocument(assemblyName, pathToXmlFile, options.ResolveExternalXmlDocs);
                if (document == null)
                {
                    return null;
                }

                return GetXmlDocsElement(parameter, document, options);
            }
            catch
            {
                return null;
            }
        }

        private static CachingXDocument? TryGetXmlDocsDocument(AssemblyName assemblyName, string? pathToXmlFile, bool resolveExternalXmlDocs)
        {
            var cacheKey = GetCacheKey(assemblyName.FullName, resolveExternalXmlDocs);

            if (Cache.TryGetValue(cacheKey, out var document))
            {
                return document;
            }

            if (pathToXmlFile is null)
            {
                return null;
            }

            if (DynamicApis.FileExists(pathToXmlFile) == false)
            {
                Cache[cacheKey] = null;
                return null;
            }

            document = new CachingXDocument(pathToXmlFile);
            Cache[cacheKey] = document;

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

        private static XElement? GetXmlDocsElement(this ParameterInfo parameter, CachingXDocument xml, XmlDocsOptions options)
        {
            var name = GetMemberElementName(parameter.Member);
            var element = xml.GetXmlDocsElement(name);
            if (element != null)
            {
                ReplaceInheritdocElements(parameter.Member, element, options);

                IEnumerable result;
                if (parameter.IsRetval || string.IsNullOrEmpty(parameter.Name))
                {
                    result = element.Elements(XmlDocsKeys.ReturnsElement);
                }
                else
                {
                    result = element.Elements(XmlDocsKeys.ParamElement).Where(x => x.Attribute(XmlDocsKeys.ParamNameAttribute)?.Value == parameter.Name);
                }

                return result.OfType<XElement>().FirstOrDefault();
            }

            return null;
        }

        private static void ReplaceInheritdocElements(this MemberInfo member, XElement? element, XmlDocsOptions options)
        {
            if (element == null)
            {
                return;
            }

            var children = element.Nodes().ToList();
            foreach (var child in children.OfType<XElement>())
            {
                if (child.Name.LocalName.ToLowerInvariant() == XmlDocsKeys.InheritDocElement)
                {
                    // if this a class/type
                    if (child.HasAttributes && (member.MemberType is MemberTypes.TypeInfo or MemberTypes.Property))
                    {
                        ProcessInheritDocTypeElements(member, child, options);
                        continue;
                    }

                    var baseType = member.DeclaringType?.GetTypeInfo().BaseType;
                    var baseMember = baseType?.GetTypeInfo().DeclaredMembers.SingleOrDefault(m => m.Name == member.Name);
                    if (baseMember != null)
                    {
                        var baseDoc = baseMember.GetXmlDocsElement(options);
                        if (baseDoc != null)
                        {
                            var nodes = baseDoc.Nodes().OfType<object>().ToArray();
                            child.ReplaceWith(nodes);
                        }
                        else
                        {
                            ProcessInheritdocInterfaceElements(member, child, options);
                        }
                    }
                    else
                    {
                        ProcessInheritdocInterfaceElements(member, child, options);
                    }
                }
            }
        }

        private static void ProcessInheritdocInterfaceElements(this MemberInfo member, XElement child, XmlDocsOptions options)
        {
            if (member.DeclaringType?.GetTypeInfo().ImplementedInterfaces == null)
            {
                return;
            }

            foreach (var baseInterface in member.DeclaringType.GetTypeInfo().ImplementedInterfaces)
            {
                var baseMember = baseInterface?.GetTypeInfo().DeclaredMembers.SingleOrDefault(m => m.Name == member.Name);
                if (baseMember != null)
                {
                    var baseDoc = baseMember.GetXmlDocsElement(options);
                    if (baseDoc != null)
                    {
                        var nodes = baseDoc.Nodes().OfType<object>().ToArray();
                        child.ReplaceWith(nodes);
                    }
                }
            }
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
                    TypeExtensions.IsAssignableToTypeName(memberType, "FieldDefinition", TypeNameStyle.Name) ? "Field" :
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
                            .Replace("[]]]", "[]}")
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
        /// <param name="options">The XML docs reading and formatting options.</param>
        /// <returns>The file path or null if not found.</returns>
        public static string? GetXmlDocsPath(Assembly? assembly, XmlDocsOptions options)
        {
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

                var cacheKey = GetCacheKey(assemblyName.FullName, options.ResolveExternalXmlDocs);

                if (Cache.ContainsKey(cacheKey))
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

                    if (options.ResolveExternalXmlDocs)
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

                    Cache[cacheKey] = null;
                    return null;
                }
                catch
                {
                    Cache[cacheKey] = null;
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get Type from a referencing string such as <c>!:MyType</c> or <c>!:MyType.MyProperty</c>
        /// </summary>
        private static void ProcessInheritDocTypeElements(this MemberInfo member, XElement child, XmlDocsOptions options)
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
                    GetXmlDocsPath(docAssembly, options),
                    true)?.GetXmlDocsElement(referencedTypeXmlId);

                /* for Record types ( as opposed to Class types ) the above lookup will fail for parameters defined in
                 * shorthand form on the Constructor. Constructor-defined Properties will show up on the constructor
                 * as <param name="PropertyName">...</param> rather than have the xml doc member element as a typical
                 * property would.
                */
                if (referencedDocs is null && referencedType.MemberType == MemberTypes.Property)
                {
                    var documentationPath = GetXmlDocsPath(member.Module.Assembly, options);
                    if (documentationPath is null)
                    {
                        return;
                    }

                    var parentElement = GetXmlDocsElement(
                        referencedType.DeclaringType.GetTypeInfo(),
                        documentationPath,
                        options);

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

        private static string? GetPathByOs(dynamic? assembly, AssemblyName assemblyName)
        {
#if NETSTANDARD2_0_OR_GREATER
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
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
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
    
            // This works for projects that have the artifacts output layout enabled:
            // https://learn.microsoft.com/en-us/dotnet/core/sdk/artifacts-output
            nuGetCacheFile = DynamicApis.PathCombine(assemblyDirectory.Replace("/bin/", "/obj/"), "../project.nuget.cache");
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
                var matches = Regex.Matches(json, $"\"((.*?){assemblyName.Name}((\\\\\\\\)|/).*?)((\\\\\\\\)|/)(.*?)\"", RegexOptions.IgnoreCase);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetCacheKey(string assemblyFullName, bool resolveExternalXmlDocs)
        {
            return $"{assemblyFullName}:{resolveExternalXmlDocs}";
        }
    }
}
