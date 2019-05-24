using Mono.Cecil;
using System.Linq;
using Xunit;

namespace Namotion.Reflection.Cecil.Tests
{
    public class XmlDocsExtensionsTests
    {
        /// <summary>
        /// My class.
        /// </summary>
        public class MyTest
        {
            /// <summary>
            /// My property.
            /// </summary>
            public string MyProperty { get; set; }

            /// <summary>
            /// My method.
            /// </summary>
            /// <param name="myParam">My param.</param>
            /// <returns>My return.</returns>
            public string MyMethod(string myParam)
            {
                return string.Empty;
            }
        }

        [Fact]
        public void When_xml_docs_is_read_for_cecil_type_then_it_works()
        {
            // Arranage
            var assemblyPath = typeof(XmlDocsExtensionsTests).Assembly.CodeBase.Replace("file:///", string.Empty);
            var xmlPath = assemblyPath.Replace(".dll", ".xml");

            var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
            var module = assembly.Modules.Last();
            var type = module.GetTypes().Single(t => t.Name.Contains(nameof(MyTest)));
            var method = type.Methods.First(m => m.Name == nameof(MyTest.MyMethod));
            var document = XmlDocsExtensions.LoadDocumentAsync(xmlPath);

            // Act
            var typeSummary = type.GetXmlDocsTag("summary", document);
            var methodSummary = method.GetXmlDocsTag("summary", document);
            var parameter = method.Parameters.Last().GetXmlDocs(document);
            var returnParameter = method.MethodReturnType.GetXmlDocs(document);

            // Assert
            Assert.Equal("My class.", typeSummary);
            Assert.Equal("My method.", methodSummary);
            Assert.Equal("My param.", parameter);
            Assert.Equal("My return.", returnParameter);
        }
    }
}
