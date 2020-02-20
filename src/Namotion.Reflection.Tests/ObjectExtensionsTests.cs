using System;
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