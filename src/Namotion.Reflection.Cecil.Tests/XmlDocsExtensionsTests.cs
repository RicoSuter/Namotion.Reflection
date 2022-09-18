using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            /// My constructor.
            /// </summary>
            public MyTest()
            {
            }

            /// <summary>
            /// My property.
            /// </summary>
            public string MyProperty { get; set; } = default!;

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
            var assemblyPath = typeof(XmlDocsExtensionsTests).Assembly.CodeBase!.Replace("file:///", string.Empty);
            var xmlPath = assemblyPath.Replace(".dll", ".xml");

            var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
            var module = assembly.Modules.Last();
            var type = module.GetTypes().Single(t => t.Name.Contains(nameof(MyTest)));
            var method = type.Methods.First(m => m.Name == nameof(MyTest.MyMethod));
            var document = XmlDocs.LoadDocument(xmlPath);

            // Act
            var typeSummary = type.GetXmlDocsTag("summary", document, XmlDocsOptions.Default);
            var constructorSummary = type.Methods.First(m => m.IsConstructor).GetXmlDocsTag("summary", document, XmlDocsOptions.Default);
            var property = type.Properties.First().GetXmlDocsSummary(document, XmlDocsOptions.Default);
            var methodSummary = method.GetXmlDocsTag("summary", document, XmlDocsOptions.Default);
            var parameter = method.Parameters.Last().GetXmlDocs(document, XmlDocsOptions.Default);
            var returnParameter = method.MethodReturnType.GetXmlDocs(document, XmlDocsOptions.Default);

            // Assert
            Assert.Equal("My class.", typeSummary);
            Assert.Equal("My constructor.", constructorSummary);
            Assert.Equal("My property.", property);
            Assert.Equal("My method.", methodSummary);
            Assert.Equal("My param.", parameter);
            Assert.Equal("My return.", returnParameter);
        }

        public class BaseGenericClass<T1, T2>
        {
            /// <summary>
            /// SingleAsync
            /// </summary>
            public Task<T1> SingleAsync(T2 foo, T1 bar)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// MultiAsync
            /// </summary>
            public Task<ICollection<T1>> MultiAsync(ICollection<T2> input)
            {
                throw new NotImplementedException();
            }

            /// <summary>Baz</summary>
            public T2 Baz { get; set; } = default!;
        }

        public class InheritedGenericClass : BaseGenericClass<string, int>
        {
        }

        [Fact]
        public void WhenTypeInheritsFromGenericType_ThenMethodAndPropertyWithGenericParametersResolvesCorrectXml()
        {
            // Arranage
            var assemblyPath = typeof(XmlDocsExtensionsTests).Assembly.CodeBase!.Replace("file:///", string.Empty);
            var xmlPath = assemblyPath.Replace(".dll", ".xml");

            var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
            var module = assembly.Modules.Last();
            var type = module.GetTypes().Single(t => t.Name.Contains("BaseGenericClass"));
            var method = type.Methods.First(m => m.Name == nameof(InheritedGenericClass.SingleAsync));
            var method2 = type.Methods.First(m => m.Name == nameof(InheritedGenericClass.MultiAsync));
            var document = XmlDocs.LoadDocument(xmlPath);

            //// Act
            var summaryMethod = method.GetXmlDocsTag("summary", document, XmlDocsOptions.Default);
            var summaryMethod2 = method2.GetXmlDocsTag("summary", document, XmlDocsOptions.Default);
            var summaryProperty = type.Properties.First().GetXmlDocsSummary(document, XmlDocsOptions.Default);

            //// Assert
            Assert.Equal("SingleAsync", summaryMethod);
            Assert.Equal("MultiAsync", summaryMethod2);
            Assert.Equal("Baz", summaryProperty);
        }
    }
}
