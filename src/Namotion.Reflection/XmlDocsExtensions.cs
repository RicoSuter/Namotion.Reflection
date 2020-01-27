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
        private static readonly Dictionary<string, XDocument> Cache = new Dictionary<string, XDocument>(StringComparer.OrdinalIgnoreCase);

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
        public static XElement GetXmlDocsElement(this ContextualMemberInfo member)
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
            return GetXmlDocsTag(member, "summary");
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
        public static XElement GetXmlDocsElement(this Type type, string pathToXmlFile)
        {
            lock (Lock)
            {
                return ((MemberInfo)type.GetTypeInfo()).GetXmlDocsWithoutLock(pathToXmlFile);
            }
        }

        /// <summary>Returns the contents of an XML documentation tag for the specified member.</summary>
        /// <param name="member">The reflected member.</param>
        /// <returns>The contents of the "summary" tag for the member.</returns>
        public static XElement GetXmlDocsElement(this MemberInfo member)
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
        public static XElement GetXmlDocsElement(this MemberInfo member, string pathToXmlFile)
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
        public static XElement GetXmlDocsElement(this ParameterInfo parameter, string pathToXmlFile)
        {
            try
            {
                if (pathToXmlFile == null || DynamicApis.SupportsXPathApis == false || DynamicApis.SupportsFileApis == false || DynamicApis.SupportsPathApis == false)
                {
                    return null;
                }

                var assemblyName = parameter.Member.Module.Assembly.GetName();
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

        private static XElement GetXmlDocumentationWithoutLock(this ParameterInfo parameter, string pathToXmlFile)
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

        private static XElement GetXmlDocsWithoutLock(this MemberInfo member)
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

        private static XElement GetXmlDocsWithoutLock(this MemberInfo member, string pathToXmlFile)
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

        private static XDocument TryGetXmlDocsDocument(AssemblyName assemblyName, string pathToXmlFile)
        {
            if (!Cache.ContainsKey(assemblyName.FullName))
            {
                if (DynamicApis.FileExists(pathToXmlFile) == false)
                {
                    Cache[assemblyName.FullName] = null;
                    return null;
                }

                Cache[assemblyName.FullName] = XDocument.Load(pathToXmlFile, LoadOptions.PreserveWhitespace);
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
            var name = DotNetReflection.GetMemberElementName(member);
            return GetXmlDocsElement(xml, name);
        }

        internal static XElement GetXmlDocsElement(this XDocument xml, string name)
        {
            var result = (IEnumerable)DynamicApis.XPathEvaluate(xml, $"/doc/members/member[@name='{name}']");
            return result.OfType<XElement>().FirstOrDefault();
        }

        private static XElement GetXmlDocsElement(this ParameterInfo parameter, XDocument xml)
        {
            var name = DotNetReflection.GetMemberElementName(parameter.Member);
            var result = (IEnumerable)DynamicApis.XPathEvaluate(xml, $"/doc/members/member[@name='{name}']");

            var element = result.OfType<XElement>().FirstOrDefault();
            if (element != null)
            {
                ReplaceInheritdocElements(parameter.Member, element);

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

        private static void ReplaceInheritdocElements(this MemberInfo member, XElement element)
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
        
        private static string GetXmlDocsPath(dynamic assembly)
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

                var currentDomain = Type.GetType("System.AppDomain")?.GetRuntimeProperty("CurrentDomain").GetValue(null);
                if (currentDomain?.HasProperty("BaseDirectory") == true)
                {
                    var baseDirectory = currentDomain.TryGetPropertyValue("BaseDirectory", "");
                    if (!string.IsNullOrEmpty(baseDirectory))
                    {
                        path = DynamicApis.PathCombine(baseDirectory, assemblyName.Name + ".xml");
                        if (DynamicApis.FileExists(path))
                        {
                            return path;
                        }

                        return DynamicApis.PathCombine(baseDirectory, "bin\\" + assemblyName.Name + ".xml");
                    }
                }

                var currentDirectory = DynamicApis.DirectoryGetCurrentDirectory();
                path = DynamicApis.PathCombine(currentDirectory, assembly.GetName().Name + ".xml");
                if (DynamicApis.FileExists(path))
                {
                    return path;
                }

                path = DynamicApis.PathCombine(currentDirectory, "bin\\" + assembly.GetName().Name + ".xml");
                if (DynamicApis.FileExists(path))
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

        private static class DotNetReflection
        {

            /// <exception cref="ArgumentException">Unknown member type.</exception>
            internal static string GetMemberElementName(MemberInfo member)
            {
                member = ResolveClosedGenerics(member);

                var asType = (object)member as Type;
                if (asType != null)
                    return GetTypeXmlDocName(asType);

                var asField = member as FieldInfo;
                if (asField != null)
                    return GetFieldDocName(asField);

                var asProperty = member as PropertyInfo;
                if (asProperty != null)
                    return GetPropertyDocName(asProperty);

                var asEvent = member as EventInfo;
                if (asEvent != null)
                    return GetEventDocName(asEvent);

                var asMethod = member as MethodBase;
                if (asMethod != null)
                    return GetMethodXmlDocName((MethodBase) member);

                throw new ArgumentException(
                    String.Format(
                        "Unknown member info type '{0}' ({1}.{2})",
                        member.GetType().Name, member.DeclaringType?.Name, member.Name),
                    "member");
            }

            /// <summary>
            /// Resolve closed generic types or members of closed generic types to their open
            /// generic base definition.
            /// </summary>
            /// <param name="member">The member or type to try to resolve.</param>
            /// <returns>
            /// <paramref name="member"/> if <paramref name="member"/> has nothing do to with closed generics;
            /// The real definition of <paramref name="member"/> if <paramref name="member"/> is a closed generic
            /// type or it's <see cref="MemberInfo.DeclaringType"/> is a closed generic.
            /// </returns>
            private static MemberInfo ResolveClosedGenerics(dynamic member)
            {
#if !NETSTANDARD1_0
                // Handles closed generic types
                if (member is Type type && type.IsGenericType)
                    return type.Module.ResolveType(type.MetadataToken);

                // Handles closed generic members
                if (member.DeclaringType?.IsGenericType == true)
                    return member.DeclaringType.Module.ResolveMember(member.MetadataToken);
#endif
                // It's OK, there is nothing to do here
                return member;
            }

            private static string GetTypeXmlDocName(Type type)
            {
                return "T:" + FormatSimpleMemberXmlDocString(type, null);
            }

            private static string GetPropertyDocName(PropertyInfo property)
            {
                return "P:" + FormatSimpleMemberXmlDocString(property.DeclaringType, property.Name);
            }

            private static string GetFieldDocName(FieldInfo field)
            {
                return "P:" + FormatSimpleMemberXmlDocString(field.DeclaringType, field.Name);
            }

            private static string GetEventDocName(EventInfo @event)
            {
                return "E:" + FormatSimpleMemberXmlDocString(@event.DeclaringType, @event.Name);
            }

            private static string GetMethodXmlDocName(MethodBase method)
            {
                var genericArgsMap = new GenericArgsMap(method);
                var parameters = method.GetParameters();

                StringBuilder nameBuilder = new StringBuilder();

                nameBuilder.Append("M:");
                nameBuilder.Append(GetTypeXmlDocString(method.DeclaringType, false, genericArgsMap));
                nameBuilder.Append(".");
                nameBuilder.Append(method.IsConstructor ? "#ctor" : method.Name);

                if (genericArgsMap.MethodMap.Count > 0)
                {
                    nameBuilder.Append("``").Append(genericArgsMap.MethodMap.Count);
                }

                if (parameters.Length > 0)
                {
                    char sep = '(';
                    foreach (var parameter in parameters)
                    {
                        nameBuilder.Append(sep);
                        nameBuilder.Append(GetTypeXmlDocString(parameter.ParameterType, true, genericArgsMap));
                        sep = ',';
                    }

                    nameBuilder.Append(')');
                }

                if (method is MethodInfo methodInfo && (method.Name == "op_Implicit" || method.Name == "op_Explicit"))
                {
                    nameBuilder.Append("~");
                    nameBuilder.Append(GetTypeXmlDocString(methodInfo.ReturnType, true, genericArgsMap));
                }

                return nameBuilder.ToString();
            }

            private static string FormatSimpleMemberXmlDocString(Type type, string memberName)
            {
                var docStr = type.FullName;
                if (memberName != null)
                    docStr = docStr + "." + memberName;
                if (docStr != null)
                    docStr = Regex.Replace(docStr, @"\[.*\]", "").Replace('+', '.');
                return docStr;
            }

            private static string GetTypeXmlDocString(
                Type type,
                bool isMethodParameter,
                GenericArgsMap genericArgsMap)
            {
                if (type.IsGenericParameter)
                {
                    int index;
                    if (genericArgsMap.MethodMap.TryGetValue(type, out index))
                        return "``" + index;
                    if (genericArgsMap.TypeMap.TryGetValue(type, out index))
                        return "`" + index;
                    throw new InvalidOperationException(
                        "No matching generic argument mapping found for generic parameter " + type);
                }

                if (type.HasElementType)
                {
                    var elementTypeStr = GetTypeXmlDocString(
                        type.GetElementType(),
                        isMethodParameter,
                        genericArgsMap);

                    if (type.IsPointer)
                    {
                        return elementTypeStr + "*";
                    }

                    if (type.IsArray)
                    {
                        var arrayRank = type.GetArrayRank();
                        return arrayRank == 1 ? "[]" : "[" + string.Join(",", Enumerable.Repeat("0:", arrayRank)) + "]";
                    }

                    if (type.IsByRef)
                    {
                        return elementTypeStr + "@";
                    }

                    throw new NotSupportedException("Unhandled element type for type " + type);
                }

                StringBuilder docStr = new StringBuilder();

                docStr.Append(type.IsNested
                    ? GetTypeXmlDocString(type.DeclaringType, isMethodParameter, genericArgsMap)
                    : type.Namespace);

                docStr.Append(".");

                docStr.Append(isMethodParameter ? Regex.Replace(type.Name, @"`\d+", "") : type.Name);

#if !NETSTANDARD1_0
                if (type.GetTypeInfo().IsGenericType && isMethodParameter)
                {
                    var genericArgsStrList = type.GetGenericArguments()
                        .Select(arg => GetTypeXmlDocString(arg, isMethodParameter, genericArgsMap));
                    docStr.Append("{").Append(string.Join(",", genericArgsStrList)).Append("}");
                }
#endif
                return docStr.ToString();
            }

            private class GenericArgsMap
            {
                public GenericArgsMap(MethodBase method)
                {
                    MethodMap = new Dictionary<Type, int>();
                    TypeMap = new Dictionary<Type, int>();

#if !NETSTANDARD1_0
                    var methodGenericArgs = method.GetGenericArguments();
                    for (int i = 0; i < methodGenericArgs.Length; i++)
                    {
                        MethodMap.Add(methodGenericArgs[i], i);
                    }

                    if (method.DeclaringType != null)
                    {
                        var typeGenericArgs = method.DeclaringType.GetGenericArguments();
                        for (int i = 0; i < typeGenericArgs.Length; i++)
                        {
                            TypeMap.Add(typeGenericArgs[i], i);
                        }
                    }
#endif
                }

                public Dictionary<Type, int> MethodMap { get; }
                public Dictionary<Type, int> TypeMap { get; }
            }
        }
    }
}