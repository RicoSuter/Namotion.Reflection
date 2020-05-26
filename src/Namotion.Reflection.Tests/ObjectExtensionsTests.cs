using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;

#nullable enable

namespace Namotion.Reflection.Tests
{
    public class ObjectExtensionsTests
    {
        [Fact]
        public void When_calling_has_property_then_result_is_correct()
        {
            //// Arrage
            var uri = new Uri("https://rsuter.com");

            //// Act & Assert
            Assert.True(uri.HasProperty(nameof(Uri.Host)));
            Assert.False(uri.HasProperty("Foo"));
        }

        [Fact]
        public void When_setting_a_property_then_it_is_set()
        {
            //// Arrange
            var uri = new Uri("https://rsuter.com");

            //// Act
            var host = uri.TryGetPropertyValue<string>(nameof(Uri.Host), null);

            //// Assert
            Assert.Equal(uri.Host, host);
        }

        public class Person
        {
            public string FirstName { get; set; }

            public string? MiddleName { get; set; }

            public string LastName { get; set; }
        }

        [Fact]
        public void When_object_has_null_properties_then_errors_are_set()
        {
            //// Arrange
            var person = JsonConvert.DeserializeObject<Person>("{}");

            //// Act
            var errors = person.ValidateNullability();
            var valid = person.HasValidNullability();

            //// Assert
            Assert.False(valid);
            Assert.Contains("FirstName", errors);
            Assert.Contains("LastName", errors);
            Assert.Throws<InvalidOperationException>(() => person.EnsureValidNullability());
        }

        [Fact]
        public void When_object_has_no_null_properties_then_errors_is_empty()
        {
            //// Arrange
            var person = JsonConvert.DeserializeObject<Person>(@"{ ""FirstName"": ""Abc"", ""LastName"": ""Def"" }");

            //// Act
            var errors = person.ValidateNullability();
            var valid = person.HasValidNullability();
            person.EnsureValidNullability(); // Does not throw

            //// Assert
            Assert.True(valid);
            Assert.Empty(errors);
        }

        ///// <summary>
        ///// One of the reproduce cases for issue 39 - should throw InvalidOperationException as 'nullable enable' is set for this file
        ///// </summary>
        //[Fact]
        //public void When_array_of_non_nullable_strings_contains_null_then_EnsureValidNullability_should_throw_InvalidOperationException()
        //{
        //    //// Arrange
        //    var arrayOfNonNulllableStrings = JsonConvert.DeserializeObject<string[]>("[ null ]");

        //    //// Act and assert
        //    Assert.Throws<InvalidOperationException>(() => arrayOfNonNulllableStrings.EnsureValidNullability());

              // TODO: This here is not possible because string[] (without property or parameter) has no context and nullability information.
        //}

        /// <summary>
        /// One of the reproduce cases for issue 39 - should not throw anything because a non-null string value is being deserialised into an array
        /// </summary>
        [Fact]
        public void When_array_of_non_nullable_strings_contains_single_non_null_string_then_EnsureValidNullability_should_not_throw()
        {
            //// Arrange
            var arrayOfNonNulllableStrings = JsonConvert.DeserializeObject<string[]>("[ \"ok\" ]");

            //// Act and assert
            arrayOfNonNulllableStrings.EnsureValidNullability();
        }

        /// <summary>
        /// One of the reproduce cases for issue 39 - should not throw anything because a null string value is being deserialised into an array of strings that are allowed to be null
        /// </summary>
        [Fact]
        public void When_array_of_nullable_strings_contains_null_then_EnsureValidNullability_should_not_throw()
        {
            //// Arrange
            var arrayOfNonNulllableStrings = JsonConvert.DeserializeObject<string?[]>("[ null ]");

            //// Act and assert
            arrayOfNonNulllableStrings.EnsureValidNullability();
        }

        /// <summary>
        /// One of the reproduce cases for issue 39 - should not throw anything because a non-null string value is being deserialised into an array of strings (that are allowed to be null)
        /// </summary>
        [Fact]
        public void When_array_of_nullable_strings_contains_single_non_null_string_then_EnsureValidNullability_should_not_throw()
        {
            //// Arrange
            var arrayOfNonNulllableStrings = JsonConvert.DeserializeObject<string?[]>("[ \"ok\" ]");

            //// Act and assert
            arrayOfNonNulllableStrings.EnsureValidNullability();
        }

        /// <summary>
        /// One of the reproduce cases for issue 39 - should not throw anything because a non-null string value is being deserialised into a property that is a list of non-nullable strings
        /// </summary>
        [Fact]
        public void When_property_list_of_non_nullable_strings_contains_single_non_null_string_then_EnsureValidNullability_should_not_throw()
        {
            //// Arrange
            var objectWithItemsListContainingSingleNonNullEntry = JsonConvert.DeserializeObject<SomethingWithListOfNonNullableStringItems>("{ \"Items\": [ \"ok\" ] }");

            //// Act and assert
            objectWithItemsListContainingSingleNonNullEntry.EnsureValidNullability();
        }

        /// <summary>
        /// One of the reproduce cases for issue 39 - should not throw anything because a non-null string value is being deserialised into a property that is a list of non-nullable strings
        /// </summary>
        [Fact]
        public void When_property_list_of_non_nullable_strings_contains_null_string_then_EnsureValidNullability_should_throw_InvalidOperationException()
        {
            //// Arrange
            var objectWithItemsListContainingSingleNullEntry = JsonConvert.DeserializeObject<SomethingWithListOfNonNullableStringItems>("{ \"Items\": [ null ] }");

            //// Act and assert
            Assert.Throws<InvalidOperationException>(() => objectWithItemsListContainingSingleNullEntry.EnsureValidNullability());
        }

        /// <summary>
        /// One of the reproduce cases for issue 39 - should not throw anything because a non-null string value is being deserialised into a property that is a list of (non-nullable) strings
        /// </summary>
        [Fact]
        public void When_property_list_of_nullable_strings_contains_single_non_null_string_then_EnsureValidNullability_should_not_throw()
        {
            //// Arrange
            var objectWithItemsListContainingSingleNonNullEntry = JsonConvert.DeserializeObject<SomethingWithListOfNullableStringItems>("{ \"Items\": [ \"ok\" ] }");

            //// Act and assert
            objectWithItemsListContainingSingleNonNullEntry.EnsureValidNullability();
        }

        /// <summary>
        /// One of the reproduce cases for issue 39 - should not throw anything because a non-null string value is being deserialised into a property that is a list of non-nullable strings
        /// </summary>
        [Fact]
        public void When_property_list_of_nullable_strings_contains_null_string_then_EnsureValidNullability_should_not_throw()
        {
            //// Arrange
            var objectWithItemsListContainingSingleNullEntry = JsonConvert.DeserializeObject<SomethingWithListOfNullableStringItems>("{ \"Items\": [ null ] }");

            //// Act and assert
            objectWithItemsListContainingSingleNullEntry.EnsureValidNullability();
        }

        private sealed class SomethingWithListOfNonNullableStringItems
        {
            public List<string> Items { get; set; } = new List<string>();
        }

        private sealed class SomethingWithListOfNullableStringItems
        {
            public List<string?> Items { get; set; } = new List<string?>();
        }

        /// <summary>
        /// This is a reproduce case for Issue 24
        /// </summary>
        [Fact]
        public void When_object_that_is_non_nested_public_type_has_a_null_value_for_a_non_nullable_property_then_EnsureValidNullability_should_throw()
        {
            //// Arrange
            var person = JsonConvert.DeserializeObject<PersonDetails_PublicNonNested>("{ \"ID\": 123, \"Name\": null }");

            //// Act and assert
            Assert.Throws<InvalidOperationException>(() => person.EnsureValidNullability());
        }

        /// <summary>
        /// Variation on Issue 24, related to discussions in that thread
        /// </summary>
        [Fact]
        public void When_object_that_is_a_nested_public_type_has_a_null_value_for_a_non_nullable_property_then_EnsureValidNullability_should_throw()
        {
            //// Arrange
            var person = JsonConvert.DeserializeObject<PersonDetails_PublicNested>("{ \"ID\": 123, \"Name\": null }");

            //// Act and assert
            Assert.Throws<InvalidOperationException>(() => person.EnsureValidNullability());
        }

        /// <summary>
        /// Variation on Issue 24, related to discussions in that thread
        /// </summary>
        [Fact]
        public void When_object_that_is_a_nested_private_type_has_a_null_value_for_a_non_nullable_property_then_EnsureValidNullability_should_throw()
        {
            //// Arrange
            var person = JsonConvert.DeserializeObject<PersonDetails_PrivateNested>("{ \"ID\": 123, \"Name\": null }");

            //// Act and assert
            Assert.Throws<InvalidOperationException>(() => person.EnsureValidNullability());
        }

        public sealed class PersonDetails_PublicNested
        {
            public PersonDetails_PublicNested(int id, string name)
            {
                ID = id;
                Name = name;
            }
            public int ID { get; }
            public string Name { get; }
        }

        private sealed class PersonDetails_PrivateNested
        {
            public PersonDetails_PrivateNested(int id, string name)
            {
                ID = id;
                Name = name;
            }
            public int ID { get; }
            public string Name { get; }
        }
    }

    public sealed class PersonDetails_PublicNonNested
    {
        public PersonDetails_PublicNonNested(int id, string name)
        {
            ID = id;
            Name = name;
        }
        public int ID { get; }
        public string Name { get; }
    }
}