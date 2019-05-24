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
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Runtime.CompilerServices;

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
        private static readonly AsyncLock Lock = new AsyncLock();
        private static readonly Dictionary<string, XDocument> Cache = new Dictionary<string, XDocument>(StringComparer.OrdinalIgnoreCase);

        internal static void ClearCache()
        {
            using (Lock.Lock())
            {
                Cache.Clear();
            }
        }

        #region Contextual

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static Task<string> GetXmlDocsSummaryAsync(this CachedType type)
        {
            return type.Type.GetXmlDocsSummaryAsync();
        }

        /// <summary>Returns the contents of the "remarks" XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static Task<string> GetXmlDocsRemarksAsync(this CachedType type)
        {
            return type.Type.GetXmlDocsRemarksAsync();
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static Task<string> GetXmlDocsTagAsync(this CachedType type, string tagName)
        {
            return type.Type.GetXmlDocsTagAsync(tagName);
        }

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static Task<string> GetXmlDocsSummaryAsync(this ContextualMemberInfo member)
        {
            return member.MemberInfo.GetXmlDocsSummaryAsync();
        }

        /// <summary>Returns the contents of the "remarks" XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static Task<string> GetXmlDocsRemarksAsync(this ContextualMemberInfo member)
        {
            return member.MemberInfo.GetXmlDocsRemarksAsync();
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static Task<string> GetXmlDocsTagAsync(this ContextualMemberInfo member, string tagName)
        {
            return member.MemberInfo.GetXmlDocsTagAsync(tagName);
        }

        /// <summary>Returns the contents of the "returns" or "param" XML documentation tag for the specified parameter.</summary>
        /// <param name="parameter">The reflected parameter or return info.</param>
        /// <returns>The contents of the "returns" or "param" tag.</returns>
        public static Task<string> GetXmlDocsAsync(this ContextualParameterInfo parameter)
        {
            return parameter.ParameterInfo.GetXmlDocsAsync();
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static Task<XElement> GetXmlDocsElementAsync(this ContextualMemberInfo member)
        {
            return member.MemberInfo.GetXmlDocsElementAsync();
        }

        #endregion

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static Task<string> GetXmlDocsSummaryAsync(this Type type)
        {
            return GetXmlDocsTagAsync((MemberInfo)type.GetTypeInfo(), "summary");
        }

        /// <summary>Returns the contents of the "remarks" XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static Task<string> GetXmlDocsRemarksAsync(this Type type)
        {
            return GetXmlDocsTagAsync((MemberInfo)type.GetTypeInfo(), "remarks");
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static Task<string> GetXmlDocsTagAsync(this Type type, string tagName)
        {
            return GetXmlDocsTagAsync((MemberInfo)type.GetTypeInfo(), tagName);
        }

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static async Task<string> GetXmlDocsSummaryAsync(this MemberInfo member)
        {
            return await GetXmlDocsTagAsync(member, "summary").ConfigureAwait(false);
        }

        /// <summary>Returns the contents of the "remarks" XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static async Task<string> GetXmlDocsRemarksAsync(this MemberInfo member)
        {
            return await GetXmlDocsTagAsync(member, "remarks").ConfigureAwait(false);
        }

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="type">The type.</param>
        /// <param name="pathToXmlFile">The path to the XML documentation file.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static async Task<XElement> GetXmlDocsElementAsync(this Type type, string pathToXmlFile)
        {
            using (Lock.Lock())
            {
                return await ((MemberInfo)type.GetTypeInfo()).GetXmlDocsWithoutLockAsync(pathToXmlFile).ConfigureAwait(false);
            }
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static async Task<XElement> GetXmlDocsElementAsync(this MemberInfo member)
        {
            using (Lock.Lock())
            {
                return await GetXmlDocsWithoutLockAsync(member).ConfigureAwait(false);
            }
        }

        /// <summary>Returns the contents of the "summary" XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="pathToXmlFile">The path to the XML documentation file.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static async Task<XElement> GetXmlDocsElementAsync(this MemberInfo member, string pathToXmlFile)
        {
            using (Lock.Lock())
            {
                return await GetXmlDocsWithoutLockAsync(member, pathToXmlFile).ConfigureAwait(false);
            }
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static async Task<string> GetXmlDocsTagAsync(this MemberInfo member, string tagName)
        {
            if (DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
            {
                return string.Empty;
            }

            var assemblyName = member.Module.Assembly.GetName();
            using (Lock.Lock())
            {
                if (IsAssemblyIgnored(assemblyName))
                {
                    return string.Empty;
                }

                var documentationPath = await GetXmlDocsPathAsync(member.Module.Assembly).ConfigureAwait(false);
                var element = await GetXmlDocsWithoutLockAsync(member, documentationPath).ConfigureAwait(false);
                return ToXmlDocsContent(element?.Element(tagName));
            }
        }

        /// <summary>Returns the contents of the "returns" or "param" XML documentation tag for the specified parameter.</summary>
        /// <param name="parameter">The reflected parameter or return info.</param>
        /// <returns>The contents of the "returns" or "param" tag.</returns>
        public static async Task<string> GetXmlDocsAsync(this ParameterInfo parameter)
        {
            if (DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
            {
                return string.Empty;
            }

            var assemblyName = parameter.Member.Module.Assembly.GetName();
            using (Lock.Lock())
            {
                if (IsAssemblyIgnored(assemblyName))
                {
                    return string.Empty;
                }

                var documentationPath = await GetXmlDocsPathAsync(parameter.Member.Module.Assembly).ConfigureAwait(false);
                var element = await GetXmlDocumentationWithoutLockAsync(parameter, documentationPath).ConfigureAwait(false);
                return ToXmlDocsContent(element);
            }
        }

        /// <summary>Returns the contents of the "returns" or "param" XML documentation tag for the specified parameter.</summary>
        /// <param name="parameter">The reflected parameter or return info.</param>
        /// <param name="pathToXmlFile">The path to the XML documentation file.</param>
        /// <returns>The contents of the "returns" or "param" tag.</returns>
        public static async Task<XElement> GetXmlDocsElementAsync(this ParameterInfo parameter, string pathToXmlFile)
        {
            try
            {
                if (pathToXmlFile == null || DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
                {
                    return null;
                }

                var assemblyName = parameter.Member.Module.Assembly.GetName();
                using (Lock.Lock())
                {
                    return await GetXmlDocumentationWithoutLockAsync(parameter, pathToXmlFile).ConfigureAwait(false);
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
        public static string ToXmlDocsContent(this XElement element)
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

        private static async Task<XElement> GetXmlDocumentationWithoutLockAsync(this ParameterInfo parameter, string pathToXmlFile)
        {
            try
            {
                if (DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
                {
                    return null;
                }

                var assemblyName = parameter.Member.Module.Assembly.GetName();
                var document = await TryGetXmlDocsDocumentAsync(assemblyName, pathToXmlFile).ConfigureAwait(false);
                if (document == null)
                {
                    return null;
                }

                return await GetXmlDocsElementAsync(parameter, document).ConfigureAwait(false);
            }
            catch
            {
                return null;
            }
        }

        private static async Task<XElement> GetXmlDocsWithoutLockAsync(this MemberInfo member)
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

            var documentationPath = await GetXmlDocsPathAsync(member.Module.Assembly).ConfigureAwait(false);
            return await GetXmlDocsWithoutLockAsync(member, documentationPath).ConfigureAwait(false);
        }

        private static async Task<XElement> GetXmlDocsWithoutLockAsync(this MemberInfo member, string pathToXmlFile)
        {
            try
            {
                if (DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
                {
                    return null;
                }

                var assemblyName = member.Module.Assembly.GetName();
                var document = await TryGetXmlDocsDocumentAsync(assemblyName, pathToXmlFile).ConfigureAwait(false);
                if (document == null)
                {
                    return null;
                }

                var element = GetXmlDocsElement(member, document);
                await ReplaceInheritdocElementsAsync(member, element).ConfigureAwait(false);
                return element;
            }
            catch
            {
                return null;
            }
        }

        private static async Task<XDocument> TryGetXmlDocsDocumentAsync(AssemblyName assemblyName, string pathToXmlFile)
        {
            if (!Cache.ContainsKey(assemblyName.FullName))
            {
                if (await DynamicApis.FileExistsAsync(pathToXmlFile).ConfigureAwait(false) == false)
                {
                    Cache[assemblyName.FullName] = null;
                    return null;
                }

                Cache[assemblyName.FullName] = await Task.Factory.StartNew(() => XDocument.Load(pathToXmlFile, LoadOptions.PreserveWhitespace)).ConfigureAwait(false);
            }

            return Cache[assemblyName.FullName];
        }

        private static bool IsAssemblyIgnored(AssemblyName assemblyName)
        {
            if (Cache.ContainsKey(assemblyName.FullName) && Cache[assemblyName.FullName] == null)
            {
                return true;
            }

            return false;
        }

        private static XElement GetXmlDocsElement(this MemberInfo member, XDocument xml)
        {
            var name = GetMemberElementName(member);
            return GetXmlDocsElement(xml, name);
        }

        internal static XElement GetXmlDocsElement(this XDocument xml, string name)
        {
            var result = (IEnumerable)DynamicApis.XPathEvaluate(xml, $"/doc/members/member[@name='{name}']");
            return result.OfType<XElement>().FirstOrDefault();
        }

        private static async Task<XElement> GetXmlDocsElementAsync(this ParameterInfo parameter, XDocument xml)
        {
            var name = GetMemberElementName(parameter.Member);
            var result = (IEnumerable)DynamicApis.XPathEvaluate(xml, $"/doc/members/member[@name='{name}']");

            var element = result.OfType<XElement>().FirstOrDefault();
            if (element != null)
            {
                await ReplaceInheritdocElementsAsync(parameter.Member, element).ConfigureAwait(false);

                if (parameter.IsRetval || string.IsNullOrEmpty(parameter.Name))
                {
                    result = (IEnumerable)DynamicApis.XPathEvaluate(xml, $"/doc/members/member[@name='{name}']/returns");
                }
                else
                {
                    result = (IEnumerable)DynamicApis.XPathEvaluate(xml, $"/doc/members/member[@name='{name}']/param[@name='{parameter.Name}']");
                }

                return result.OfType<XElement>().FirstOrDefault();
            }

            return null;
        }

        private static async Task ReplaceInheritdocElementsAsync(this MemberInfo member, XElement element)
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
                        var baseDoc = await baseMember.GetXmlDocsWithoutLockAsync().ConfigureAwait(false);
                        if (baseDoc != null)
                        {
                            var nodes = baseDoc.Nodes().OfType<object>().ToArray();
                            child.ReplaceWith(nodes);
                        }
                        else
                        {
                            await ProcessInheritdocInterfaceElementsAsync(member, child).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await ProcessInheritdocInterfaceElementsAsync(member, child).ConfigureAwait(false);
                    }
                }
            }
        }

        private static async Task ProcessInheritdocInterfaceElementsAsync(this MemberInfo member, XElement child)
        {
            foreach (var baseInterface in member.DeclaringType.GetTypeInfo().ImplementedInterfaces)
            {
                var baseMember = baseInterface?.GetTypeInfo().DeclaredMembers.SingleOrDefault(m => m.Name == member.Name);
                if (baseMember != null)
                {
                    var baseDoc = await baseMember.GetXmlDocsWithoutLockAsync().ConfigureAwait(false);
                    if (baseDoc != null)
                    {
                        var nodes = baseDoc.Nodes().OfType<object>().ToArray();
                        child.ReplaceWith(nodes);
                    }
                }
            }
#endif
        }

        private static string RemoveLineBreakWhiteSpaces(string documentation)
        {
            if (string.IsNullOrEmpty(documentation))
            {
                return string.Empty;
            }

            documentation = "\n" + documentation.Replace("\r", string.Empty).Trim('\n');

            var whitespace = Regex.Match(documentation, "(\\n[ \\t]*)").Value;
            documentation = documentation.Replace(whitespace, "\n");

            return documentation.Trim('\n');
        }

        /// <exception cref="ArgumentException">Unknown member type.</exception>
        internal static string GetMemberElementName(dynamic member)
        {
            char prefixCode;

            string memberName = member is Type memberType && !string.IsNullOrEmpty(memberType.FullName) ?
                   memberType.FullName :
                   member.DeclaringType.FullName + "." + member.Name;

            memberName = memberName.Replace("/", ".");

            var type = ObjectExtensions.HasProperty(member, "MemberType") ? (string)member.MemberType.ToString() :
                TypeExtensions.IsAssignableToTypeName(member.GetType(), "MethodDefinition", TypeNameStyle.Name) ? "Method" :
                "TypeInfo";

            switch (type)
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

        private static async Task<string> GetXmlDocsPathAsync(dynamic assembly)
        {
            string path;
            try
            {
                if (assembly == null)
                {
                    return null;
                }

                var assemblyName = assembly.GetName();
                if (string.IsNullOrEmpty(assemblyName.Name))
                {
                    return null;
                }

                if (Cache.ContainsKey(assemblyName.FullName))
                {
                    return null;
                }

                if (!string.IsNullOrEmpty(assembly.Location))
                {
                    var assemblyDirectory = DynamicApis.PathGetDirectoryName((string)assembly.Location);
                    path = DynamicApis.PathCombine(assemblyDirectory, (string)assemblyName.Name + ".xml");
                    if (await DynamicApis.FileExistsAsync(path).ConfigureAwait(false))
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

                        if (await DynamicApis.FileExistsAsync(path).ConfigureAwait(false))
                        {
                            return path;
                        }
                    }
                }

                var currentDomain = Type.GetType("System.AppDomain")?.GetRuntimeProperty("CurrentDomain").GetValue(null);
                if (currentDomain?.HasProperty("BaseDirectory") == true)
                {
                    var baseDirectory = currentDomain.TryGetPropertyValue("BaseDirectory", "");
                    if (!string.IsNullOrEmpty(baseDirectory))
                    {
                        path = DynamicApis.PathCombine(baseDirectory, assemblyName.Name + ".xml");
                        if (await DynamicApis.FileExistsAsync(path).ConfigureAwait(false))
                        {
                            return path;
                        }

                        return DynamicApis.PathCombine(baseDirectory, "bin\\" + assemblyName.Name + ".xml");
                    }
                }

                var currentDirectory = await DynamicApis.DirectoryGetCurrentDirectoryAsync();
                path = DynamicApis.PathCombine(currentDirectory, assembly.GetName().Name + ".xml");
                if (await DynamicApis.FileExistsAsync(path).ConfigureAwait(false))
                {
                    return path;
                }

                path = DynamicApis.PathCombine(currentDirectory, "bin\\" + assembly.GetName().Name + ".xml");
                if (await DynamicApis.FileExistsAsync(path).ConfigureAwait(false))
                {
                    return path;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private class AsyncLock : IDisposable
        {
            private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

            public AsyncLock Lock()
            {
                _semaphoreSlim.Wait();
                return this;
            }

            public void Dispose()
            {
                _semaphoreSlim.Release();
            }
        }
    }
}