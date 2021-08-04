using System.Collections.Generic;
using System.Xml.Linq;

namespace Namotion.Reflection
{
    /// <summary>
    /// Caching layer hiding the details of accessing DLL documentation.
    /// </summary>
    internal class CachingXDocument
    {
        private static readonly object Lock = new();
        private static readonly Dictionary<string, XElement?> ElementByNameCache = new();

        private static readonly XName XNameDoc = "doc";
        private static readonly XName XNameMembers = "members";
        private static readonly XName XNameMember = "member";
        private static readonly XName XNameName = "name";

        private readonly XDocument _document;

        internal CachingXDocument(string? pathToXmlFile)
        {
            // can later change to document streaming if needed
            var doc = XDocument.Load(pathToXmlFile, LoadOptions.PreserveWhitespace);
            _document = doc;
        }

        internal XElement? GetXmlDocsElement(string name)
        {
            lock (Lock)
            {
                if (!ElementByNameCache.TryGetValue(name, out var element))
                {
                    element = GetXmlDocsElement(_document, name);

                    ElementByNameCache[name] = element;
                }
                return element;
            }
        }

        internal static XElement? GetXmlDocsElement(XDocument document, string name)
        {
            foreach (var e in document.Element(XNameDoc).Element(XNameMembers).Elements(XNameMember))
            {
                if (e.Attribute(XNameName)?.Value == name)
                {
                    return e;
                }
            }

            return null;
        }
    }
}