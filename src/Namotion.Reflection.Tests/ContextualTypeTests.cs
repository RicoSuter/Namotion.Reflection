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
                !.GetParameters().First();

            var param2 = typeof(MultipleParametersWithSameName)
                .GetMethod(nameof(MultipleParametersWithSameName.Bar))
                !.GetParameters().First();

            // Act
            var type1 = param1.ToContextualParameter().ParameterType.Name;
            var type2 = param2.ToContextualParameter().ParameterType.Name;

            // Assert
            Assert.Equal("String", type1);
            Assert.Equal("Int32", type2);
        }

        public class ParentClass
        {
            public string Property { get; set; }
        }

        public class GenericChildClass<T> : ParentClass
        {
        }

        [Fact]
        public void WhenReadingProperties_ThenNoExceptionIsThrown()
        {
            // Arrange
            var ct = typeof(GenericChildClass<string>).ToContextualType();

            // Act
            var properties = ct.Properties;

            // Assert
            Assert.NotNull(properties);
        }

        readonly struct Point2D<T>
            where T : unmanaged
        {
            // THIS BREAKS NSWAG
            public static readonly Point2D<T> Zero = new();

            public T X { get; init; }

            public T Y { get; init; }

            public Point2D(T x, T y)
            {
                X = x;
                Y = y;
            }
        }

        [Fact]
        public void WhenConvertingTypeWithStaticFields_ThenItDoesNotFail()
        {
            // Arrange
            var ct = typeof(Point2D<double>).ToContextualType();

            // Act
            var fs = ct.Fields;

            // Assert
            Assert.NotNull(fs);
        }
    }
}
