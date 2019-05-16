using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Namotion.Reflection.Tests
{
    public class TypeExtensionsTests
    {
        [Theory]
        [InlineData(typeof(string), "IEnumerable", true)]
        [InlineData(typeof(string), "IList", false)]
        [InlineData(typeof(Foo), "Foo", true)]
        [InlineData(typeof(Foo), "Bar", true)]
        public void Given_a_type_then_is_assignable_returns_correct_result(Type type, string typeName, bool expectedResult)
        {
            //// Act
            var result = type.IsAssignableToTypeName(typeName, TypeNameStyle.Name);

            //// Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(typeof(string), "IEnumerable", false)]
        [InlineData(typeof(string), "IList", false)]
        [InlineData(typeof(Foo), "Foo", false)]
        [InlineData(typeof(Foo), "Bar", true)]
        public void Given_a_type_then_inherits_from_returns_correct_result(Type type, string typeName, bool expectedResult)
        {
            //// Act
            var result = type.InheritsFromTypeName(typeName, TypeNameStyle.Name);

            //// Assert
            Assert.Equal(expectedResult, result);
        }

        public class Foo : Bar
        {
        }

        public class Bar
        {
        }

        [Fact]
        public void When_type_implements_equatable_then_item_type_can_be_retrieved()
        {
            //// Act
            var itemType = typeof(MyType).GetEnumerableItemType();

            //// Assert
            Assert.Equal(typeof(int), itemType);
        }

        public class MyType : IEquatable<MyType>, IEnumerable<int>
        {
            public bool Equals(MyType other)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<int> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void When_display_name_is_retrieved_then_string_is_correct()
        {
            //// Act
            var displayName = typeof(Dictionary<string, int>).GetDisplayName();

            //// Assert
            Assert.Equal("DictionaryOfStringAndInt32", displayName);
        }
    }
}