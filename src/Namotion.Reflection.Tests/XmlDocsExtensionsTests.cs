using Microsoft.AspNetCore.Mvc;
using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Namotion.Reflection.Tests
{
    public class XmlDocsExtensionsTests
    {
        /// <summary>WithSummary</summary>
        public class SummaryXmlDoc
        {
            /// <summary>Foo</summary>
            public string Foo { get; set; }
        }

        public class CrefInheritXmlDoc
        {
            /// <inheritdoc cref="SummaryXmlDoc.Foo"/>
            public string Bar { get; set; }
        }

        [Fact]
        public void When_xml_doc_with_cref_inheritdoc_on_property_is_read_then_inherited_type_is_valid()
        {
            // Arrange
            XmlDocs.ClearCache();
            
            // Act
            var fooElement = typeof(SummaryXmlDoc).GetProperty("Foo").GetXmlDocsElement();
            var fooResponse = fooElement.Elements("summary");
            var fooValue = fooResponse.Single().Value;
            
            var barElement = typeof(CrefInheritXmlDoc).GetProperty("Bar").GetXmlDocsElement();
            var barResponse = barElement.Elements("summary");
            var varValue = barResponse.Single().Value;

            Assert.Equal(fooValue, varValue);
        }
        
        public class CrefInheritXmlDocForType
        {
            /// <inheritdoc cref="SummaryXmlDoc"/>
            public string Bar { get; set; }
        }
        
        [Fact]
        public void When_xml_doc_with_cref_inheritdoc_on_type_is_read_then_inherited_type_is_valid()
        {
            // Arrange
            XmlDocs.ClearCache();
            
            // Act
            var summaryElement = typeof(SummaryXmlDoc).GetXmlDocsElement();
            var summaryResponse = summaryElement.Elements("summary");
            var summaryValue = summaryResponse.Single().Value;
            
            var barElement = typeof(CrefInheritXmlDocForType).GetProperty("Bar").GetXmlDocsElement();
            var barResponse = barElement.Elements("summary");
            var barValue = barResponse.Single().Value;

            Assert.Equal(summaryValue, barValue);
        }
        
        /// <summary>WithSummary</summary>
        public class SummaryXmlDocGeneric<T>
        {
            /// <summary>FooGeneric</summary>
            public List<T> Foo { get; set; }
        }

        public class CrefInheritXmlDocGeneric
        {
            /// <inheritdoc cref="SummaryXmlDocGeneric{T}.Foo"/>
            public string Bar { get; set; }
        }
        
        [Fact]
        public void When_xml_doc_with_cref_inheritdoc_on_generic_property_is_read_then_inherited_type_is_valid()
        {
            // Arrange
            XmlDocs.ClearCache();
            
            // Act
            var fooElement = typeof(SummaryXmlDocGeneric<object>).GetProperty("Foo").GetXmlDocsElement();
            var fooResponse = fooElement.Elements("summary");
            var fooValue = fooResponse.Single().Value;
            
            var barElement = typeof(CrefInheritXmlDocGeneric).GetProperty("Bar").GetXmlDocsElement();
            var barResponse = barElement.Elements("summary");
            var barValue = barResponse.Single().Value;

            Assert.Equal(fooValue, barValue);
        }
        
        /// <summary>WithSummary</summary>
        public class SummaryXmlDocParent
        {
            public class SummaryXmlDocChild
            {
                /// <summary>Foo</summary>
                public string Foo { get; set; }
            }
        }
        
        public class CrefInheritNestedXmlDoc
        {
            /// <inheritdoc cref="SummaryXmlDocParent.SummaryXmlDocChild.Foo"/>
            public string Bar { get; set; }
        }
        
        [Fact]
        public void When_xml_doc_with_cref_inheritdoc_on_nested_class_property_is_read_then_inherited_type_is_valid()
        {
            // Arrange
            XmlDocs.ClearCache();
            
            // Act
            var fooElement = typeof(SummaryXmlDocParent.SummaryXmlDocChild).GetProperty("Foo").GetXmlDocsElement();
            var fooResponse = fooElement.Elements("summary");
            var fooValue = fooResponse.Single().Value;
            
            var barElement = typeof(CrefInheritNestedXmlDoc).GetProperty("Bar").GetXmlDocsElement();
            var barResponse = barElement.Elements("summary");
            var barValue = barResponse.Single().Value;

            Assert.Equal(fooValue, barValue);
        }
        
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

        public class WithParamrefTagInXmlDoc
        {
            /// <summary>Returns <paramref name="name"/>.</summary>
            /// <param name="name">The name to return.</param>
            public string Foo(string name) => name;
        }

        [Fact]
        public void When_summary_has_paramref_tag_then_it_is_converted()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var summary = typeof(WithParamrefTagInXmlDoc).GetMethod("Foo").GetXmlDocsSummary();

            //// Assert
            Assert.Equal("Returns name.", summary);
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
        public void When_summary_has_generic_tags_then_it_is_converted_to_html()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            XmlDocsOptions options = new XmlDocsOptions()
            {
                FormattingMode = XmlDocsFormattingMode.Html
            };
            var summary = typeof(WithGenericTagsInXmlDoc).GetProperty("Foo").GetXmlDocsSummary(options);

            //// Assert
            Assert.Equal("This <pre>are</pre> <strong>some</strong> tags.", summary);
        }

        [Fact]
        public void When_summary_has_generic_tags_then_it_is_converted_to_markdown()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            XmlDocsOptions options = new XmlDocsOptions()
            {
                FormattingMode = XmlDocsFormattingMode.Markdown
            };
            var summary = typeof(WithGenericTagsInXmlDoc).GetProperty("Foo").GetXmlDocsSummary(options);

            //// Assert
            Assert.Equal("This `are` **some** tags.", summary);
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
            /// MultiGenericParameter
            /// </summary>
            public IDictionary<string, string> MultiGenericParameter(IDictionary<string, string> input)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// NestedGenericParameter
            /// </summary>
            public IDictionary<string, IDictionary<string, IDictionary<string, string>>> NestedGenericParameter(IDictionary<string, IDictionary<string, IDictionary<string, string>>> input)
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

            /// <summary>
            /// MultiGenericParameterAsync
            /// </summary>
            public Task<IDictionary<string, string>> MultiGenericParameterAsync(IDictionary<string, string> input)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// NestedGenericParameterAsync
            /// </summary>
            public Task<IDictionary<string, IDictionary<string, IDictionary<string, string>>>> NestedGenericParameterAsync(IDictionary<string, IDictionary<string, IDictionary<string, string>>> input)
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
            var multiGenericParameterSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.MultiGenericParameter)).GetXmlDocsSummary();
            var nestedGenericParameterSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.NestedGenericParameter)).GetXmlDocsSummary();
            var singleAsyncSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.SingleAsync)).GetXmlDocsSummary();
            var multiAsyncSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.MultiAsync)).GetXmlDocsSummary();
            var multiGenericParameterAsyncSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.MultiGenericParameterAsync)).GetXmlDocsSummary();
            var nestedGenericParameterAsyncSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.NestedGenericParameterAsync)).GetXmlDocsSummary();

            //// Assert
            Assert.Equal("Single", singleSummary);
            Assert.Equal("Multi", multiSummary);
            Assert.Equal("MultiGenericParameter", multiGenericParameterSummary);
            Assert.Equal("NestedGenericParameter", nestedGenericParameterSummary);
            Assert.Equal("SingleAsync", singleAsyncSummary);
            Assert.Equal("MultiAsync", multiAsyncSummary);
            Assert.Equal("MultiGenericParameterAsync", multiGenericParameterAsyncSummary);
            Assert.Equal("NestedGenericParameterAsync", nestedGenericParameterAsyncSummary);
        }

        public class BusinessProcessSearchResult : SearchBehaviorBaseResult<BusinessProcess>
        {
        }

        public class BusinessProcess
        {
        }

        public class SearchBehaviorBaseResult<T> : BaseResult<T>, ISearchBehaviorResult
        {
            /// <inheritdoc />
            public string SearchString { get; set; }

            /// <inheritdoc />
            public bool IsSearchStringRewritten { get; set; }
        }

        public interface ISearchBehaviorResult
        {
            /// <summary>
            /// The search string used to query the data
            /// </summary>
            string SearchString { get; set; }

            /// <summary>
            /// Flag to notify if the SearchString was modified compared to the original requested one
            /// </summary>
            bool IsSearchStringRewritten { get; set; }
        }

        /// <summary>
        /// Base class for search results
        /// </summary>
        /// <typeparam name="T">Type of the results</typeparam>
        public class BaseResult<T> : IPagedSearchResult
        {
            /// <inheritdoc />
            public string PageToken { get; set; }
        }

        public interface IPagedSearchResult
        {
            /// <summary>
            /// An optional token to access the next page of results for those endpoints that support a backend scrolling logic.
            /// </summary>
            string PageToken { get; set; }
        }

        [Fact]
        public void When_inheritdocs_is_availble_in_inheritance_chain_then_it_is_resolved()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var searchStringProperty = typeof(BusinessProcessSearchResult).GetRuntimeProperty("SearchString").GetXmlDocsSummary();
            var isSearchStringRewrittenProperty = typeof(BusinessProcessSearchResult).GetRuntimeProperty("IsSearchStringRewritten").GetXmlDocsSummary();
            var pageTokenProperty = typeof(BusinessProcessSearchResult).GetRuntimeProperty("PageToken").GetXmlDocsSummary();

            //// Assert
            Assert.True(!string.IsNullOrWhiteSpace(searchStringProperty));
            Assert.True(!string.IsNullOrWhiteSpace(isSearchStringRewrittenProperty));
            Assert.True(!string.IsNullOrWhiteSpace(pageTokenProperty));
        }

        /// <summary>
        /// The publisher.
        /// </summary>
        public class Publisher
        {
            /// <summary>
            /// The name of the publisher.
            /// </summary>
            public string Name { get; set; }
        }

        [Fact]
        public void When_type_has_summary_then_it_is_read()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var summary = typeof(Publisher).GetXmlDocsSummary();

            //// Assert
            Assert.False(string.IsNullOrWhiteSpace(summary));
        }

        // TODO doesn't pass on full framework currently
#if NET6_0_OR_GREATER
        [Fact]
        public void When_xml_doc_is_in_working_dir_then_it_is_found()
        {
            //// Arrange
            XmlDocs.ClearCache();

            _ = Directory.CreateDirectory("./wd");
            File.WriteAllText("./wd/System.Drawing.Primitives.xml", @"<?xml version=""1.0""?>
                <doc>
                    <assembly><name>System.Drawing.Primitives</name></assembly>
                    <members>
                        <member name=""T:System.Drawing.Point"">
                            <summary>A point.</summary>
                        </member>
                    </members>
                </doc>");
            Directory.SetCurrentDirectory("./wd");

            //// Act
            var summary = typeof(Point).GetXmlDocsSummary();

            //// Clean up
            Directory.SetCurrentDirectory("..");
            Directory.Delete("./wd", recursive: true);

            //// Assert
            Assert.Equal("A point.", summary);
        }
#endif

        /// <summary>
        /// Returns a list of items.
        /// </summary>
        /// <param name="PageNumber">The page number.</param>
        /// <param name="PageSize">The page size.</param>
        public record ListItems(int PageNumber, int PageSize);

        [Fact]
        public void When_record_has_param_properties_then_xml_docs_is_read()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var listItemsSummary = typeof(ListItems).GetXmlDocsSummary();
            var pageNumberSummary = typeof(ListItems).GetRuntimeProperty("PageNumber").GetXmlDocsSummary();

            //// Assert
            Assert.Equal("Returns a list of items.", listItemsSummary);
            Assert.Equal("The page number.", pageNumberSummary);
        }

        // TODO currently failing on full framework
#if NET6_0_OR_GREATER
        [Fact]
        public void When_type_is_in_NuGet_then_xml_docs_should_be_found()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var summary = typeof(JsonSchema).GetXmlDocsSummary();

            //// Assert
            Assert.False(string.IsNullOrWhiteSpace(summary));
        }

        [Fact]
        public void When_type_is_in_AspNetCore_then_docs_should_be_found()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var summary = typeof(ProblemDetails).GetXmlDocsSummary();

            //// Assert
            Assert.False(string.IsNullOrWhiteSpace(summary));
        }
#endif

        public abstract class WithSeeTagForMethodInParamXmlDoc
        {
            /// <summary>Foo.</summary>
            string Foo { get; }

            /// <summary>Bar.</summary>
            /// <param name="baz">Boolean returned from method <see cref="int.TryParse(string?, out int)"/>.</param>
            public abstract void Bar(bool baz);
        }

        [Fact]
        public void When_param_has_see_tag_for_method_XML_docs_gets_method_name()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var parameterXml = typeof(WithSeeTagForMethodInParamXmlDoc).GetMethod("Bar").GetParameters()
                .Single(p => p.Name == "baz").GetXmlDocs();

            //// Assert
            Assert.Equal("Boolean returned from method TryParse.", parameterXml);
        }

        /// <summary>
        /// A test class
        /// </summary>
        public class TestClass<TModel>
        {
            /// <summary>
            /// Summary for TestMethod
            /// </summary>
            /// <param name="expr">An expression</param>
            public void TestMethod(Expression<Func<TModel>> expr)
            {
            }
        }

        [Fact]
        public void When_parameter_is_expression_then_xmldocs_are_available()
        {
            //// Arrange
            var t = typeof(TestClass<string>);

            //// Act
            var m = t.GetMethods().First();
            var s = m.GetXmlDocsSummary();

            //// Assert
            Assert.False(string.IsNullOrWhiteSpace(s));
        }

        public class FieldClass
        {
            /// <summary>
            /// My field.
            /// </summary>
            public string MyField = string.Empty;
        }

        [Fact]
        public void When_parameter_has_xml_docs_then_it_is_found()
        {
            //// Arrange
            var t = typeof(FieldClass);

            //// Act
            var f = t.GetFields().First();
            var s = f.GetXmlDocsSummary();

            //// Assert
            Assert.False(string.IsNullOrWhiteSpace(s));
        }

        public enum MyEnum
        {
            /// <summary>
            /// My foo.
            /// </summary>
            Foo,

            /// <summary>
            /// My bar.
            /// </summary>
            Bar
        }

        [Fact]
        public void When_enum_values_have_xml_docs_then_they_are_read()
        {
            //// Arrange
            var t = typeof(MyEnum);

            //// Act
            var f1 = t.GetFields()[1];
            var f2 = t.GetFields()[2];
            var s1 = f1.GetXmlDocsSummary();
            var s2 = f2.GetXmlDocsSummary();

            //// Assert
            Assert.Equal("My foo.", s1);
            Assert.Equal("My bar.", s2);
        }
    }
}
