using Namotion.Reflection.Tests.FullAssembly;
using System;
using System.Linq;
using Xunit;

#if !NET45
#nullable enable
#endif

namespace Namotion.Reflection.Tests
{
    public class ContextualTypeTests
    {
#if !NET45

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
            var typeWithContext = parameter.ToContextualParameter();

            // Assert
            Assert.Equal(Nullability.NotNullable, typeWithContext.Nullability);

            Assert.Equal(Nullability.NotNullable, typeWithContext.GenericArguments[0].Nullability);
            Assert.Equal(Nullability.NeverNull, typeWithContext.GenericArguments[1].Nullability);

            Assert.Equal(Nullability.NeverNull, typeWithContext.GenericArguments[2].OriginalNullability);
            Assert.Equal(Nullability.Nullable, typeWithContext.GenericArguments[2].Nullability);

            Assert.Equal(Nullability.NotNullable, typeWithContext.GenericArguments[3].Nullability);
            Assert.Equal(Nullability.Nullable, typeWithContext.GenericArguments[4].Nullability);
        }

        [Fact]
        public void WhenMethodParametersNullabilityIsReflectedOnAssemblyWithProjectNRT_ThenItWorks()
        {
            // Arrange
            var method = typeof(FullAssemblyTestAction).GetMethod(nameof(FullAssemblyTestAction.Action));
            var parameter = method.GetParameters().First();

            // Act
            var typeWithContext = parameter.ToContextualParameter();

            // Assert
            Assert.Equal(Nullability.NotNullable, typeWithContext.Nullability);
        }

#endif

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
            var typeWithContext = parameter.ToContextualParameter();

            // Assert
            Assert.Equal(Nullability.NotNullable, typeWithContext.Nullability);
            Assert.Equal(Nullability.NotNullable, typeWithContext.GenericArguments[0].Nullability);
            Assert.Equal(Nullability.Nullable, typeWithContext.GenericArguments[1].Nullability);
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
            var typeWithContext = property.ToContextualProperty();

            // Assert
            Assert.Equal(Nullability.NotNullable, typeWithContext.Nullability);
            Assert.Equal(Nullability.Nullable, typeWithContext.GenericArguments[0].Nullability);
        }

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
