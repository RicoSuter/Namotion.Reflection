using System.Collections.Generic;
using System.Xml.Linq;

namespace Namotion.Reflection
{
    /// <summary>
    /// Caching layer hiding the details of accessing DLL documentation.
    /// </summary>
    internal sealed class CachingXDocument
    {
        private static readonly XName XNameDoc = "doc";
        private static readonly XName XNameMembers = "members";
        private static readonly XName XNameMember = "member";
        private static readonly XName XNameName = "name";

        private readonly object _lock = new();
        private readonly Dictionary<string, XElement?> _elementByNameCache = new();
        private readonly XDocument _document;

        internal CachingXDocument(string? pathToXmlFile)
        {
            // can later change to document streaming if needed
            var doc = XDocument.Load(pathToXmlFile, LoadOptions.PreserveWhitespace);
            _document = doc;
        }

        internal XElement? GetXmlDocsElement(string name)
        {
            lock (_lock)
            {
                if (!_elementByNameCache.TryGetValue(name, out var element))
                {
                    element = GetXmlDocsElement(_document, name);

                    _elementByNameCache[name] = element;
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