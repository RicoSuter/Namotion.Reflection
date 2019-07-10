using System.Linq;
using Xunit;

#nullable enable

namespace Namotion.Reflection.Tests
{
    public class ContextualTypeTests
    {
        public class MultipleParametersWithSameName
        {
            public void Foo(string abc) { }

            public void Bar(int abc) { }
        }

        [Fact]
        public void WhenParameterHasSameNameButDifferentType_ThenContextualTypesAreDifferent()
        {
            // Arrange
            var param1 = typeof(MultipleParametersWithSameName)
                .GetMethod(nameof(MultipleParametersWithSameName.Foo))
                .GetParameters().First();

            var param2 = typeof(MultipleParametersWithSameName)
                .GetMethod(nameof(MultipleParametersWithSameName.Bar))
                .GetParameters().First();

            // Act
            var type1 = param1.ToContextualParameter().TypeName;
            var type2 = param2.ToContextualParameter().TypeName;

            // Assert
            Assert.Equal("String", type1);
            Assert.Equal("Int32", type2);
        }
    }
}
