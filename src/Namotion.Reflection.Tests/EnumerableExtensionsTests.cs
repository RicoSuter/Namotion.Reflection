using Xunit;

namespace Namotion.Reflection.Tests
{
    public class EnumerableExtensionsTests
    {
        [Fact]
        public void When_retrieving_first_assignable_object_then_it_is_correct()
        {
            //// Act
            var obj = new object[] { 3, "abc" }.FirstAssignableToTypeNameOrDefault("String", TypeNameStyle.Name);

            //// Assert
            Assert.Equal("abc", obj);
        }

        public class Dog : Animal
        {
        }

        public class Horse : Animal
        {
        }

        public class Animal
        {
        }

        [Fact]
        public void When_two_classes_inherit_common_base_class_then_it_is_the_common_base_type()
        {
            //// Act
            var commonBaseType = new[] { typeof(Dog), typeof(Horse) }.GetCommonBaseType();

            //// Assert
            Assert.Equal(typeof(Animal), commonBaseType);
        }

        [Fact]
        public void When_one_class_is_base_class_then_it_is_the_common_base_class()
        {
            //// Act
            var commonBaseType = new[] { typeof(Animal), typeof(Horse) }.GetCommonBaseType();

            //// Assert
            Assert.Equal(typeof(Animal), commonBaseType);
        }

        [Fact]
        public void When_no_common_base_class_exists_then_object_is_common_base_class()
        {
            //// Act
            var commonBaseType = new[] { typeof(Animal), typeof(Horse), typeof(EnumerableExtensionsTests) }.GetCommonBaseType();

            //// Assert
            Assert.Equal(typeof(object), commonBaseType);
        }
    }
}
