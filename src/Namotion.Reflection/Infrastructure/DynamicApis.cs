//-----------------------------------------------------------------------
// <copyright file="DynamicApis.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Reflection;
using System.Xml.Linq;

namespace Namotion.Reflection.Infrastructure
{
    /// <summary>Provides dynamic access to framework APIs.</summary>
    internal static class DynamicApis
    {
        private static readonly Type XPathExtensionsType;
        private static readonly Type FileType;
        private static readonly Type DirectoryType;
        private static readonly Type PathType;

        static DynamicApis()
        {
            XPathExtensionsType = TryLoadType(
                "System.Xml.XPath.Extensions, System.Xml.XPath.XDocument",
                "System.Xml.XPath.Extensions, System.Xml.Linq, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

            FileType = TryLoadType("System.IO.File, System.IO.FileSystem", "System.IO.File");
            DirectoryType = TryLoadType("System.IO.Directory, System.IO.FileSystem", "System.IO.Directory");
            PathType = TryLoadType("System.IO.Path, System.IO.FileSystem", "System.IO.Path");
        }

        /// <summary>Gets a value indicating whether file APIs are available.</summary>
        public static bool SupportsFileApis => FileType != null;

        /// <summary>Gets a value indicating whether path APIs are available.</summary>
        public static bool SupportsPathApis => PathType != null;

        /// <summary>Gets a value indicating whether path APIs are available.</summary>
        public static bool SupportsDirectoryApis => DirectoryType != null;

        /// <summary>Gets a value indicating whether XPath APIs are available.</summary>
        public static bool SupportsXPathApis => XPathExtensionsType != null;

        /// <summary>Gets the current working directory.</summary>
        /// <returns>The directory path.</returns>
        /// <exception cref="NotSupportedException">The System.IO.Directory API is not available on this platform.</exception>
        public static string DirectoryGetCurrentDirectory()
        {
            if (!SupportsDirectoryApis)
                throw new NotSupportedException("The System.IO.Directory API is not available on this platform.");

            return (string)DirectoryType.GetRuntimeMethod("GetCurrentDirectory", new Type[] { }).Invoke(null, new object[] { });
        }

        /// <summary>Checks whether a file exists.</summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>true or false</returns>
        /// <exception cref="NotSupportedException">The System.IO.File API is not available on this platform.</exception>
        public static bool FileExists(string filePath)
        {
            if (!SupportsFileApis)
                throw new NotSupportedException("The System.IO.File API is not available on this platform.");

            if (string.IsNullOrEmpty(filePath))
                return false;

            return (bool)FileType.GetRuntimeMethod("Exists", new[] { typeof(string) }).Invoke(null, new object[] { filePath });
        }

        /// <summary>Combines two paths.</summary>
        /// <param name="path1">The path1.</param>
        /// <param name="path2">The path2.</param>
        /// <returns>The combined path.</returns>
        /// <exception cref="NotSupportedException">The System.IO.Path API is not available on this platform.</exception>
        public static string PathCombine(string path1, string path2)
        {
            if (!SupportsPathApis)
                throw new NotSupportedException("The System.IO.Path API is not available on this platform.");

            return (string)PathType.GetRuntimeMethod("Combine", new[] { typeof(string), typeof(string) }).Invoke(null, new object[] { path1, path2 });
        }

        /// <summary>Gets the directory path of a file path.</summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>The directory name.</returns>
        /// <exception cref="NotSupportedException">The System.IO.Path API is not available on this platform.</exception>
        public static string PathGetDirectoryName(string filePath)
        {
            if (!SupportsPathApis)
                throw new NotSupportedException("The System.IO.Path API is not available on this platform.");

            return (string)PathType.GetRuntimeMethod("GetDirectoryName", new[] { typeof(string) }).Invoke(null, new object[] { filePath });
        }

        /// <summary>Evaluates the XPath for a given XML document.</summary>
        /// <param name="document">The document.</param>
        /// <param name="path">The path.</param>
        /// <returns>The value.</returns>
        /// <exception cref="NotSupportedException">The System.Xml.XPath.Extensions API is not available on this platform.</exception>
        public static object XPathEvaluate(XDocument document, string path)
        {
            if (!SupportsXPathApis)
                throw new NotSupportedException("The System.Xml.XPath.Extensions API is not available on this platform.");

            return XPathExtensionsType.GetRuntimeMethod("XPathEvaluate", new[] { typeof(XDocument), typeof(string) }).Invoke(null, new object[] { document, path });
        }

        private static Type TryLoadType(params string[] typeNames)
        {
            foreach (var typeName in typeNames)
            {
                try
                {
                    var type = Type.GetType(typeName, false);
                    if (type != null)
                        return type;
                }
                catch
                {
                }
            }
            return null;
        }
    }
}