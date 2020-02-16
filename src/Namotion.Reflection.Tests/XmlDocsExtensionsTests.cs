using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Namotion.Reflection.Tests
{
    public class XmlDocsExtensionsTests
    {
        public class WithComplexXmlDoc
        {
            /// <summary>
            /// Query and manages users.
            /// 
            /// Please note:
            /// * Users ...
            /// * Users ...
            ///     * Users ...
            ///     * Users ...
            ///
            /// You need one of the following role: Owner, Editor, use XYZ to manage permissions.
            /// </summary>
            public string Foo { get; set; }
        }

        [Fact]
        public void When_xml_doc_with_multiple_breaks_is_read_then_they_are_not_stripped_away()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var summary = typeof(WithComplexXmlDoc).GetProperty("Foo").GetXmlDocsSummary();

            //// Assert
            Assert.Contains("\n\n", summary);
            Assert.Contains("    * Users", summary);
            Assert.Equal(summary.Trim(), summary);
        }

        public class WithTagsInXmlDoc
        {
            /// <summary>Gets or sets the foo.</summary>
            /// <response code="201">Account created</response>
            /// <response code="400">Username already in use</response>
            public string Foo { get; set; }
        }

        [Fact]
        public void When_xml_doc_contains_xml_then_it_is_fully_read()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var element = typeof(WithTagsInXmlDoc).GetProperty("Foo").GetXmlDocsElement();
            var responses = element.Elements("response");

            //// Assert
            Assert.Equal(2, responses.Count());

            Assert.Equal("Account created", responses.First().Value);
            Assert.Equal("201", responses.First().Attribute("code").Value);

            Assert.Equal("Username already in use", responses.Last().Value);
            Assert.Equal("400", responses.Last().Attribute("code").Value);
        }

        public class WithSeeTagInXmlDoc
        {
            /// <summary><see langword="null"/> for the default <see cref="Record"/>. See <see cref="Record">this</see> and <see href="https://github.com/rsuter/njsonschema">this</see> at <see href="https://github.com/rsuter/njsonschema"/>.</summary>
            public string Foo { get; set; }
        }

        [Fact]
        public void When_summary_has_see_tag_then_it_is_converted()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var summary = typeof(WithSeeTagInXmlDoc).GetProperty("Foo").GetXmlDocsSummary();

            //// Assert
            Assert.Equal("null for the default Record. See this and this at https://github.com/rsuter/njsonschema.", summary);
        }

        public class WithGenericTagsInXmlDoc
        {
            /// <summary>This <c>are</c> <strong>some</strong> tags.</summary>
            public string Foo { get; set; }
        }

        [Fact]
        public void When_summary_has_generic_tags_then_it_is_converted()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var summary = typeof(WithGenericTagsInXmlDoc).GetProperty("Foo").GetXmlDocsSummary();

            //// Assert
            Assert.Equal("This are some tags.", summary);
        }

        [Fact]
        public void When_xml_doc_is_missing_then_summary_is_missing()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var summary = typeof(Point).GetXmlDocsSummary();
            var summary2 = typeof(Point).GetXmlDocsSummary();

            //// Assert
            Assert.Empty(summary);
        }

        public abstract class BaseBaseClass
        {
            /// <summary>Foo.</summary>
            public abstract string Foo { get; }

            /// <summary>Bar.</summary>
            /// <param name="baz">Baz.</param>
            public abstract void Bar(string baz);
        }

        public abstract class BaseClass : BaseBaseClass
        {
            /// <inheritdoc />
            public override string Foo { get; }

            /// <inheritdoc />
            public override void Bar(string baz) { }
        }

        public class ClassWithInheritdoc : BaseClass
        {
            /// <inheritdoc />
            public override string Foo { get; }

            /// <inheritdoc />
            public override void Bar(string baz) { }
        }

        [Fact]
        public void When_parameter_has_inheritdoc_then_it_is_resolved()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var parameterXml = typeof(ClassWithInheritdoc).GetMethod("Bar").GetParameters()
                .Single(p => p.Name == "baz").GetXmlDocs();

            //// Assert
            Assert.Equal("Baz.", parameterXml);
        }

        [Fact]
        public void When_property_has_inheritdoc_then_it_is_resolved()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var propertySummary = typeof(ClassWithInheritdoc).GetProperty("Foo").GetXmlDocsSummary();

            //// Assert
            Assert.Equal("Foo.", propertySummary);
        }

        [Fact]
        public void When_method_has_inheritdoc_then_it_is_resolved()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var methodSummary = typeof(ClassWithInheritdoc).GetMethod("Bar").GetXmlDocsSummary();

            //// Assert
            Assert.Equal("Bar.", methodSummary);
        }

        public interface IBaseBaseInterface
        {
            /// <summary>Foo.</summary>
            string Foo { get; }

            /// <summary>Bar.</summary>
            /// <param name="baz">Baz.</param>
            void Bar(string baz);
        }

        public interface IBaseInterface : IBaseBaseInterface
        {
        }

        public class ClassWithInheritdocOnInterface : IBaseInterface
        {
            /// <inheritdoc />
            public string Foo { get; }

            /// <inheritdoc />
            public void Bar(string baz) { }
        }

        [Fact]
        public void When_parameter_has_inheritdoc_on_interface_then_it_is_resolved()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var parameterXml = typeof(ClassWithInheritdocOnInterface).GetMethod("Bar").GetParameters()
                .Single(p => p.Name == "baz").GetXmlDocs();

            //// Assert
            Assert.Equal("Baz.", parameterXml);
        }

        [Fact]
        public void When_property_has_inheritdoc_on_interface_then_it_is_resolved()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var propertySummary = typeof(ClassWithInheritdocOnInterface).GetProperty("Foo").GetXmlDocsSummary();

            //// Assert
            Assert.Equal("Foo.", propertySummary);
        }

        [Fact]
        public void When_method_has_inheritdoc_then_on_interface_it_is_resolved()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var methodSummary = typeof(ClassWithInheritdocOnInterface).GetMethod("Bar").GetXmlDocsSummary();

            //// Assert
            Assert.Equal("Bar.", methodSummary);
        }

        public abstract class MyBaseClass
        {
            /// <summary>
            /// Foo
            /// </summary>
            public void Foo(int p)
            {
            }
        }

        public class MyClass : MyBaseClass
        {
            /// <summary>
            /// Bar
            /// </summary>
            public void Bar()
            {
            }
        }

        [Fact]
        public void When_method_is_inherited_then_xml_docs_are_correct()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var fooSummary = typeof(MyClass).GetMethod(nameof(MyClass.Foo)).GetXmlDocsSummary();
            var barSummary = typeof(MyClass).GetMethod(nameof(MyClass.Bar)).GetXmlDocsSummary();

            //// Assert
            Assert.Equal("Foo", fooSummary);
            Assert.Equal("Bar", barSummary);
        }

        public abstract class BaseController<T>
        {
            /// <summary>Base method.</summary>
            public string Test()
            {
                return null;
            }
        }

        public class MyController : BaseController<string>
        {
        }

        [Fact]
        public void WhenTypeInheritsFromGenericType_ThenXmlDocsIsFound()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var fooSummary = typeof(MyController).GetMethod(nameof(BaseController<string>.Test)).GetXmlDocsSummary();

            //// Assert
            Assert.Equal("Base method.", fooSummary);
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

            /// <summary>Baz</summary>
            public T2 Baz { get; set; }
        }

        public class InheritedGenericClass : BaseGenericClass<string, int>
        {
        }

        [Fact]
        public void WhenTypeInheritsFromGenericType_ThenMethodAndPropertyWithGenericParametersResolvesCorrectXml()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var summaryMethod = typeof(InheritedGenericClass).GetMethod("SingleAsync").GetXmlDocsSummary();
            var summaryProperty = typeof(InheritedGenericClass).GetProperty("Baz").GetXmlDocsSummary();

            //// Assert
            Assert.Equal("SingleAsync", summaryMethod);
            Assert.Equal("Baz", summaryProperty);
        }

        public class BaseGenericClass<T>
        {
            /// <summary>
            /// Single
            /// </summary>
            public T Single(T input)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Multi
            /// </summary>
            public ICollection<T> Multi(ICollection<T> input)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// SingleAsync
            /// </summary>
            public Task<T> SingleAsync(T input)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// MultiAsync
            /// </summary>
            public Task<ICollection<T>> MultiAsync(ICollection<T> input)
            {
                throw new NotImplementedException();
            }
        }

        public class InheritedGenericClass2 : BaseGenericClass<string>
        {
        }

        [Fact]
        public void When_method_is_inherited_from_generic_class_then_xml_docs_are_correct()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var singleSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.Single)).GetXmlDocsSummary();
            var multiSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.Multi)).GetXmlDocsSummary();
            var singleAsyncSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.SingleAsync)).GetXmlDocsSummary();
            var multiAsyncSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.MultiAsync)).GetXmlDocsSummary();

            //// Assert
            Assert.Equal("Single", singleSummary);
            Assert.Equal("Multi", multiSummary);
            Assert.Equal("SingleAsync", singleAsyncSummary);
            Assert.Equal("MultiAsync", multiAsyncSummary);
        }
    }
}
