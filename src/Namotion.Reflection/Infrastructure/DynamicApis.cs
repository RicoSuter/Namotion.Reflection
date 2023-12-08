//-----------------------------------------------------------------------
// <copyright file="DynamicApis.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.IO;
using System.Xml.XPath;
using System.Xml.Linq;

namespace Namotion.Reflection.Infrastructure
{
    /// <summary>Provides dynamic access to framework APIs.</summary>
    internal static class DynamicApis
    {
        /// <summary>Gets a value indicating whether file APIs are available.</summary>
        public const bool SupportsFileApis = true;

        /// <summary>Gets a value indicating whether path APIs are available.</summary>
        public const bool SupportsPathApis = true;

        /// <summary>Gets a value indicating whether path APIs are available.</summary>
        public const bool SupportsDirectoryApis = true;

        /// <summary>Gets a value indicating whether XPath APIs are available.</summary>
        public const bool SupportsXPathApis = true;
        
        /// <summary>Gets the current working directory.</summary>
        /// <returns>The directory path.</returns>
        /// <exception cref="NotSupportedException">The System.IO.Directory API is not available on this platform.</exception>
        public static string DirectoryGetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        /// <summary>Checks whether a file exists.</summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>true or false</returns>
        /// <exception cref="NotSupportedException">The System.IO.File API is not available on this platform.</exception>
        public static bool FileExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            return File.Exists(filePath);
        }

        /// <summary>Read a file.</summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>The file content.</returns>
        /// <exception cref="NotSupportedException">The System.IO.File API is not available on this platform.</exception>
        public static string FileReadAllText(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        /// <summary>Checks whether a directory exists.</summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <returns>true or false</returns>
        /// <exception cref="NotSupportedException">The System.IO.File API is not available on this platform.</exception>
        public static bool DirectoryExists(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath))
                return false;

            return Directory.Exists(directoryPath);
        }

        /// <summary>Gets all files of directory and its sub-directories.</summary>
        /// <param name="path">The directory path.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <returns>true or false</returns>
        /// <exception cref="NotSupportedException">The System.IO.Directory API is not available on this platform.</exception>
        public static string[] DirectoryGetAllFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
        }

        /// <summary>Gets all files of directory.</summary>
        /// <param name="path">The directory path.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <returns>true or false</returns>
        /// <exception cref="NotSupportedException">The System.IO.Directory API is not available on this platform.</exception>
        public static string[] DirectoryGetFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern);
        }

        /// <summary>Combines two paths.</summary>
        /// <param name="path1">The path1.</param>
        /// <param name="path2">The path2.</param>
        /// <returns>The combined path.</returns>
        /// <exception cref="NotSupportedException">The System.IO.Path API is not available on this platform.</exception>
        public static string PathCombine(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        /// <summary>Gets the directory path of a file path.</summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>The directory name.</returns>
        /// <exception cref="NotSupportedException">The System.IO.Path API is not available on this platform.</exception>
        public static string PathGetDirectoryName(string filePath)
        {
            return Path.GetDirectoryName(filePath);
        }

        /// <summary>Evaluates the XPath for a given XML document.</summary>
        /// <param name="document">The document.</param>
        /// <param name="path">The path.</param>
        /// <returns>The value.</returns>
        /// <exception cref="NotSupportedException">The System.Xml.XPath.Extensions API is not available on this platform.</exception>
        public static object? XPathEvaluate(XDocument document, string path)
        {
            return document.XPathEvaluate(path);
        }
    }
}