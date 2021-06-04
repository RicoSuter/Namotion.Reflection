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
using System.IO;

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
        private static readonly object Lock = new object();
        private static readonly Dictionary<string, CachingXDocument?> Cache = new Dictionary<string, CachingXDocument?>(StringComparer.OrdinalIgnoreCase);

        internal static void ClearCache()
        {
            lock (Lock)
            {
                Cache.Clear();
            }
        }

        #region Contextual

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsSummary(this CachedType type)
        {
            return type.Type.GetXmlDocsSummary();
        }

        /// <summary>Returns the contents of the "remarks" XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsRemarks(this CachedType type)
        {
            return type.Type.GetXmlDocsRemarks();
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsTag(this CachedType type, string tagName)
        {
            return type.Type.GetXmlDocsTag(tagName);
        }

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsSummary(this ContextualMemberInfo member)
        {
            return member.MemberInfo.GetXmlDocsSummary();
        }

        /// <summary>Returns the contents of the "remarks" XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsRemarks(this ContextualMemberInfo member)
        {
            return member.MemberInfo.GetXmlDocsRemarks();
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsTag(this ContextualMemberInfo member, string tagName)
        {
            return member.MemberInfo.GetXmlDocsTag(tagName);
        }

        /// <summary>Returns the contents of the "returns" or "param" XML documentation tag for the specified parameter.</summary>
        /// <param name="parameter">The reflected parameter or return info.</param>
        /// <returns>The contents of the "returns" or "param" tag.</returns>
        public static string GetXmlDocs(this ContextualParameterInfo parameter)
        {
            return parameter.ParameterInfo.GetXmlDocs();
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static XElement? GetXmlDocsElement(this ContextualMemberInfo member)
        {
            return member.MemberInfo.GetXmlDocsElement();
        }

        #endregion

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsSummary(this Type type)
        {
            return GetXmlDocsTag((MemberInfo)type.GetTypeInfo(), "summary");
        }

        /// <summary>Returns the contents of the "remarks" XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsRemarks(this Type type)
        {
            return GetXmlDocsTag((MemberInfo)type.GetTypeInfo(), "remarks");
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsTag(this Type type, string tagName)
        {
            return GetXmlDocsTag((MemberInfo)type.GetTypeInfo(), tagName);
        }

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsSummary(this MemberInfo member)
        {
            var docs = GetXmlDocsTag(member, "summary");

            if (string.IsNullOrEmpty(docs) && member is PropertyInfo propertyInfo)
            {
                return propertyInfo.GetXmlDocsRecordPropertySummary();
            }

            return docs;
        }

        /// <summary>Returns the contents of the "remarks" XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsRemarks(this MemberInfo member)
        {
            return GetXmlDocsTag(member, "remarks");
        }

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <param name="pathToXmlFile">The path to the XML documentation file.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static XElement? GetXmlDocsElement(this Type type, string pathToXmlFile)
        {
            lock (Lock)
            {
                return type.GetTypeInfo().GetXmlDocsWithoutLock(pathToXmlFile);
            }
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static XElement? GetXmlDocsElement(this MemberInfo member)
        {
            lock (Lock)
            {
                return GetXmlDocsWithoutLock(member);
            }
        }

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="pathToXmlFile">The path to the XML documentation file.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static XElement? GetXmlDocsElement(this MemberInfo member, string pathToXmlFile)
        {
            lock (Lock)
            {
                return GetXmlDocsWithoutLock(member, pathToXmlFile);
            }
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static string GetXmlDocsTag(this MemberInfo member, string tagName)
        {
            if (DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
            {
                return string.Empty;
            }

            _ = member ?? throw new ArgumentNullException(nameof(member));
            _ = tagName ?? throw new ArgumentNullException(nameof(tagName));

            var assemblyName = member.Module.Assembly.GetName();
            lock (Lock)
            {
                if (IsAssemblyIgnored(assemblyName))
                {
                    return string.Empty;
                }

                var documentationPath = GetXmlDocsPath(member.Module.Assembly);
                var element = GetXmlDocsWithoutLock(member, documentationPath);
                return ToXmlDocsContent(element?.Element(tagName));
            }
        }

        /// <summary>Returns the property summary of a Record type which is read from the param tag on the type.</summary>
        /// <param name="member">The reflected member.</param>
        /// <returns>The contents of the "param" tag of the Record property.</returns>
        public static string GetXmlDocsRecordPropertySummary(this PropertyInfo member)
        {
            if (DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
            {
                return string.Empty;
            }

            _ = member ?? throw new ArgumentNullException(nameof(member));

            var assemblyName = member.Module.Assembly.GetName();
            lock (Lock)
            {
                if (IsAssemblyIgnored(assemblyName))
                {
                    return string.Empty;
                }

                var documentationPath = GetXmlDocsPath(member.Module.Assembly);
                var parentElement = GetXmlDocsWithoutLock(member.DeclaringType.GetTypeInfo(), documentationPath);
                var paramElement = parentElement?
                    .Elements("param")?
                    .FirstOrDefault(x => x.Attribute("name")?
                    .Value == member.Name);

                return paramElement != null ? ToXmlDocsContent(paramElement) : string.Empty;
            }
        }

        /// <summary>Returns the contents of the "returns" or "param" XML documentation tag for the specified parameter.</summary>
        /// <param name="parameter">The reflected parameter or return info.</param>
        /// <returns>The contents of the "returns" or "param" tag.</returns>
        public static string GetXmlDocs(this ParameterInfo parameter)
        {
            if (DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
            {
                return string.Empty;
            }

            var assemblyName = parameter.Member.Module.Assembly.GetName();
            lock (Lock)
            {
                if (IsAssemblyIgnored(assemblyName))
                {
                    return string.Empty;
                }

                var documentationPath = GetXmlDocsPath(parameter.Member.Module.Assembly);
                var element = GetXmlDocumentationWithoutLock(parameter, documentationPath);
                return ToXmlDocsContent(element);
            }
        }

        /// <summary>Returns the contents of the "returns" or "param" XML documentation tag for the specified parameter.</summary>
        /// <param name="parameter">The reflected parameter or return info.</param>
        /// <param name="pathToXmlFile">The path to the XML documentation file.</param>
        /// <returns>The contents of the "returns" or "param" tag.</returns>
        public static XElement? GetXmlDocsElement(this ParameterInfo parameter, string pathToXmlFile)
        {
            try
            {
                if (pathToXmlFile == null || DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
                {
                    return null;
                }

                lock (Lock)
                {
                    return GetXmlDocumentationWithoutLock(parameter, pathToXmlFile);
                }
            }
            catch
            {
                return null;
            }
        }

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
                                        value.Append(attribute.Value.Trim('!', ':').Trim().Split('.').Last());
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
                    else
                    {
                        value.Append(node);
                    }
                }

                return RemoveLineBreakWhiteSpaces(value.ToString());
            }

            return string.Empty;
        }

        private static XElement? GetXmlDocumentationWithoutLock(this ParameterInfo parameter, string? pathToXmlFile)
        {
            try
            {
                if (DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
                {
                    return null;
                }

                var assemblyName = parameter.Member.Module.Assembly.GetName();
                var document = TryGetXmlDocsDocument(assemblyName, pathToXmlFile);
                if (document == null)
                {
                    return null;
                }

                return GetXmlDocsElement(parameter, document);
            }
            catch
            {
                return null;
            }
        }

        private static XElement? GetXmlDocsWithoutLock(this MemberInfo member)
        {
            if (DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
            {
                return null;
            }

            var assemblyName = member.Module.Assembly.GetName();
            if (IsAssemblyIgnored(assemblyName))
            {
                return null;
            }

            var documentationPath = GetXmlDocsPath(member.Module.Assembly);
            return GetXmlDocsWithoutLock(member, documentationPath);
        }

        private static XElement? GetXmlDocsWithoutLock(this MemberInfo member, string? pathToXmlFile)
        {
            try
            {
                if (DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
                {
                    return null;
                }

                var assemblyName = member.Module.Assembly.GetName();
                var document = TryGetXmlDocsDocument(assemblyName, pathToXmlFile);
                if (document == null)
                {
                    return null;
                }

                var element = GetXmlDocsElement(member, document);
                ReplaceInheritdocElements(member, element);
                return element;
            }
            catch
            {
                return null;
            }
        }

        private static CachingXDocument? TryGetXmlDocsDocument(AssemblyName assemblyName, string? pathToXmlFile)
        {
            if (Cache.TryGetValue(assemblyName.FullName, out var document))
            {
                return document;
            }

            if (pathToXmlFile is null)
            {
                return null;
            }

            if (DynamicApis.FileExists(pathToXmlFile) == false)
            {
                Cache[assemblyName.FullName] = null;
                return null;
            }

            document = new CachingXDocument(pathToXmlFile);
            Cache[assemblyName.FullName] = document;

            return document;
        }

        private static bool IsAssemblyIgnored(AssemblyName assemblyName)
        {
            return Cache.TryGetValue(assemblyName.FullName, out var document) && document == null;
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

        private static XElement? GetXmlDocsElement(this ParameterInfo parameter, CachingXDocument xml)
        {
            var name = GetMemberElementName(parameter.Member);
            var element = xml.GetXmlDocsElement(name);
            if (element != null)
            {
                ReplaceInheritdocElements(parameter.Member, element);

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

        private static void ReplaceInheritdocElements(this MemberInfo member, XElement? element)
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
                    var baseType = member.DeclaringType.GetTypeInfo().BaseType;
                    var baseMember = baseType?.GetTypeInfo().DeclaredMembers.SingleOrDefault(m => m.Name == member.Name);
                    if (baseMember != null)
                    {
                        var baseDoc = baseMember.GetXmlDocsWithoutLock();
                        if (baseDoc != null)
                        {
                            var nodes = baseDoc.Nodes().OfType<object>().ToArray();
                            child.ReplaceWith(nodes);
                        }
                        else
                        {
                            ProcessInheritdocInterfaceElements(member, child);
                        }
                    }
                    else
                    {
                        ProcessInheritdocInterfaceElements(member, child);
                    }
                }
            }
        }

        private static void ProcessInheritdocInterfaceElements(this MemberInfo member, XElement child)
        {
            foreach (var baseInterface in member.DeclaringType.GetTypeInfo().ImplementedInterfaces)
            {
                var baseMember = baseInterface?.GetTypeInfo().DeclaredMembers.SingleOrDefault(m => m.Name == member.Name);
                if (baseMember != null)
                {
                    var baseDoc = baseMember.GetXmlDocsWithoutLock();
                    if (baseDoc != null)
                    {
                        var nodes = baseDoc.Nodes().OfType<object>().ToArray();
                        child.ReplaceWith(nodes);
                    }
                }
            }
#endif
        }

        private static string RemoveLineBreakWhiteSpaces(string? documentation)
        {
            if (string.IsNullOrEmpty(documentation))
            {
                return string.Empty;
            }

            documentation = "\n" + documentation!.Replace("\r", string.Empty).Trim('\n');

            var whitespace = Regex.Match(documentation, "(\\n[ \\t]*)").Value;
            documentation = documentation.Replace(whitespace, "\n");

            return documentation.Trim('\n');
        }

        /// <exception cref="ArgumentException">Unknown member type.</exception>
        internal static string GetMemberElementName(dynamic member)
        {
            char prefixCode;
            string memberName;
            string memberTypeName;

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
                    type.FullName.Split('[')[0] : ((string)member.DeclaringType.FullName).Split('[')[0] + "." + member.Name;

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
                            ((string)p.ParameterType.FullName).Split('`')[0] + "{" + string.Join(",", ((ICollection)p.ParameterType.GenericArguments).Cast<dynamic>().Select(u => "||" + u.Position)) + "}" :
                            "||" + p.ParameterType.Position) :
                        (string)p.ParameterType.FullName;

                    var parameters = member is MethodBase ?
                        ((MethodBase)member).GetParameters().Select(x =>
                            x.ParameterType.FullName ??
                            (((dynamic)x.ParameterType).GenericTypeArguments.Length > 0 ?
                                x.ParameterType.Namespace + "." + x.ParameterType.Name.Split('`')[0] +
                                    "{" + string.Join(",", ((Type[])((dynamic)x.ParameterType).GenericTypeArguments).Select(a => "||" + a.GenericParameterPosition.ToString())) + "}" :
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
        /// Retrieve path to XML documentation for <paramref name="assembly"/>
        /// </summary>

#if NETSTANDARD1_0
        public static string? GetXmlDocsPath( dynamic? assembly ) {
#else
            
        public static string? GetXmlDocsPath(Assembly? assembly)
        {
#endif
            try {
                if ( assembly == null ) {
                    return null;
                }

                AssemblyName assemblyName = assembly.GetName();
                if ( string.IsNullOrEmpty( assemblyName.Name ) ) {
                    return null;
                }

                var assemblyFullName = assemblyName.FullName;
                if ( Cache.ContainsKey( assemblyFullName ) ) {
                    return null;
                }

                try {
                    string? path = assembly?.Location;
                    if ( path is not null && !String.IsNullOrEmpty( path ) ) {
                        var assemblyDirectory = DynamicApis.PathGetDirectoryName(path);
                        path = DynamicApis.PathCombine( assemblyDirectory, assemblyName.Name + ".xml" );
                        if ( DynamicApis.FileExists( path ) ) {
                            return path;
                        }
                    }

                    if ( ObjectExtensions.HasProperty( assembly, "CodeBase" ) ) {
                        var codeBase = (string)assembly?.CodeBase;
                        if ( !string.IsNullOrEmpty( codeBase ) ) {
                            path = DynamicApis.PathCombine( DynamicApis.PathGetDirectoryName( codeBase
                                .Replace( "file:///", string.Empty ) ), assemblyName.Name + ".xml" )
                                .Replace( "file:\\", string.Empty );

                            if ( DynamicApis.FileExists( path ) ) {
                                return path;
                            }
                        }
                    }

                    var currentDomain = Type.GetType("System.AppDomain")?.GetRuntimeProperty("CurrentDomain")?.GetValue(null);
                    if ( currentDomain?.HasProperty( "BaseDirectory" ) == true ) {
                        var baseDirectory = currentDomain.TryGetPropertyValue("BaseDirectory", "");
                        if ( !string.IsNullOrEmpty( baseDirectory ) ) {
                            path = DynamicApis.PathCombine( baseDirectory, assemblyName.Name + ".xml" );
                            if ( DynamicApis.FileExists( path ) ) {
                                return path;
                            }

                            path = DynamicApis.PathCombine( baseDirectory, "bin/" + assemblyName.Name + ".xml" );
                            if ( DynamicApis.FileExists( path ) ) {
                                return path;
                            }
                        }
                    }

                    var currentDirectory = DynamicApis.DirectoryGetCurrentDirectory();
                    path = DynamicApis.PathCombine( currentDirectory, assembly.GetName().Name + ".xml" );
                    if ( DynamicApis.FileExists( path ) ) {
                        return path;
                    }

                    path = DynamicApis.PathCombine( currentDirectory, "bin/" + assembly.GetName().Name + ".xml" );
                    if ( DynamicApis.FileExists( path ) ) {
                        return path;
                    }

                    dynamic? executingAssembly = typeof(Assembly)
                        .GetRuntimeMethod("GetExecutingAssembly", new Type[0])?
                        .Invoke(null, new object[0]);
                    if ( !string.IsNullOrEmpty( executingAssembly?.Location ) ) {
                        var assemblyDirectory = DynamicApis.PathGetDirectoryName((string)executingAssembly!.Location);
                        path = GetXmlDocsPathFromNuGetCacheOrDotNetSdk( assemblyDirectory, assemblyName );
                        if ( path != null && DynamicApis.FileExists( path ) ) {
                            return path;
                        }
                    }

                    Cache[ assemblyFullName ] = null;
                    return null;
                } catch {
                    Cache[ assemblyFullName ] = null;
                    return null;
                }
            } catch {
                return null;
            }
        }

        private static string? GetXmlDocsPathFromNuGetCacheOrDotNetSdk(string assemblyDirectory, AssemblyName assemblyName)
        {
            var configs = DynamicApis.DirectoryGetAllFiles(assemblyDirectory, "*.runtimeconfig.dev.json");
            if (configs.Any())
            {
                try
                {
                    // Retrieve NuGet package cache directories from *.runtimeconfig.dev.json
                    var json = DynamicApis.FileReadAllText(configs.First());
                    var matches = Regex.Matches(json, $"\"((.*?)((\\\\\\\\)|(////))(.*?))\"", RegexOptions.IgnoreCase);
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
                                        var files = DynamicApis.DirectoryGetAllFiles(packagePath, assemblyName.Name + ".xml")
                                            .OrderBy(f => f)
                                            .ToArray();

                                        if (files.Any())
                                        {
                                            return files.Last();
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
                                            var files = DynamicApis.DirectoryGetAllFiles(path, assemblyName.Name + ".xml")
                                               .OrderBy(f => f)
                                               .Where(f => f.Replace('\\', '/').Contains("/" + assemblyName.Version.ToString(2)))
                                               .ToArray();

                                            if (files.Any())
                                            {
                                                return files.Last();
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
    }
}
