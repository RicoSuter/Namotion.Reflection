using System;
using System.Linq;
using Xunit;

#nullable enable

namespace Namotion.Reflection.Tests
{
    public class TypeWithContextNullabilityTests
    {
        class TestAction
        {
            public void Action(Tuple<string, int, int?, TestAction, TestAction?> parameter)
            {
            }
        }

        [Fact]
        public void WhenMethodParametersNullabilityIsReflected_ThenItWorks()
        {
            // Arrange
            var method = typeof(TestAction).GetMethod(nameof(TestAction.Action));
            var parameter = method.GetParameters().First();

            // Act
            var typeWithContext = parameter.GetTypeWithContext();

            // Assert
            Assert.Equal(Nullability.NotNull, typeWithContext.Nullability);

            Assert.Equal(Nullability.NotNull, typeWithContext.GenericArguments[0].Nullability);
            Assert.Equal(Nullability.NeverNull, typeWithContext.GenericArguments[1].Nullability);

            Assert.Equal(Nullability.NeverNull, typeWithContext.GenericArguments[2].OriginalNullability);
            Assert.Equal(Nullability.Null, typeWithContext.GenericArguments[2].Nullability);

            Assert.Equal(Nullability.NotNull, typeWithContext.GenericArguments[3].Nullability);
            Assert.Equal(Nullability.Null, typeWithContext.GenericArguments[4].Nullability);
        }

        class TestFunction
        {
            public Tuple<string, string?> Function()
            {
                return new Tuple<string, string?>("", "");
            }
        }

        [Fact]
        public void WhenMethodReturnTypesNullabilityIsReflected_ThenItWorks()
        {
            // Arrange
            var method = typeof(TestFunction).GetMethod(nameof(TestFunction.Function));
            var parameter = method.ReturnParameter;

            // Act
            var typeWithContext = parameter.GetTypeWithContext();

            // Assert
            Assert.Equal(Nullability.NotNull, typeWithContext.Nullability);
            Assert.Equal(Nullability.NotNull, typeWithContext.GenericArguments[0].Nullability);
            Assert.Equal(Nullability.Null, typeWithContext.GenericArguments[1].Nullability);
        }

        class TestProperty
        {
            public Tuple<string?> Property { get; set; } = new Tuple<string?>("");
        }

        [Fact]
        public void WhenPropertiesNullabilityIsReflected_ThenItWorks()
        {
            // Arrange
            var property = typeof(TestProperty).GetProperty(nameof(TestProperty.Property));

            // Act
            var typeWithContext = property.GetTypeWithContext();

            // Assert
            Assert.Equal(Nullability.NotNull, typeWithContext.Nullability);
            Assert.Equal(Nullability.Null, typeWithContext.GenericArguments[0].Nullability);
        }
    }
}
