using Namotion.Reflection.Tests.FullAssembly;
using System;
using System.Linq;
using Xunit;

#nullable enable

namespace Namotion.Reflection.Tests
{
    public class NullabilityTests
    {
        class TestAction
        {
            public void Action(Tuple<string, int, int?, TestAction, TestAction?> parameter)
            {
            }
        }

        [Fact]
        public void MixedTupleParameter()
        {
            // Arrange
            var method = typeof(TestAction).GetMethod(nameof(TestAction.Action));
            var parameter = method.GetParameters().First();

            // Act
            var typeWithContext = parameter.ToContextualParameter();

            // Assert
            Assert.Equal(Nullability.NotNullable, typeWithContext.Nullability);

            Assert.Equal(Nullability.NotNullable, typeWithContext.GenericArguments[0].Nullability);
            Assert.Equal(Nullability.NotNullable, typeWithContext.GenericArguments[1].Nullability);

            Assert.Equal(Nullability.NotNullable, typeWithContext.GenericArguments[2].OriginalNullability);
            Assert.Equal(Nullability.Nullable, typeWithContext.GenericArguments[2].Nullability);

            Assert.Equal(Nullability.NotNullable, typeWithContext.GenericArguments[3].Nullability);
            Assert.Equal(Nullability.Nullable, typeWithContext.GenericArguments[4].Nullability);
        }

        class MultiDimensionalArrayTest
        {
            public void Arrays(string[][]?[] arrays)
            {
            }
        }

        [Fact]
        public void MultiDimensionalArrays()
        {
            // Arrange
            var method = typeof(MultiDimensionalArrayTest).GetMethod(nameof(MultiDimensionalArrayTest.Arrays));
            var parameter = method.GetParameters().First();

            // Act
            var typeWithContext = parameter.ToContextualParameter();

            // Assert
            Assert.Equal(Nullability.NotNullable, typeWithContext.Nullability);
            Assert.Equal(Nullability.Nullable, typeWithContext.ElementType.Nullability);
            Assert.Equal(Nullability.NotNullable, typeWithContext.ElementType.ElementType.Nullability);
            Assert.Equal(Nullability.NotNullable, typeWithContext.ElementType.ElementType.ElementType.Nullability);
        }


        class NullableArrayItemTest
        {
            public void Arrays(string?[] arrays)
            {
            }
        }

        [Fact]
        public void NullableArrayItem()
        {
            // Arrange
            var method = typeof(NullableArrayItemTest).GetMethod(nameof(NullableArrayItemTest.Arrays));
            var parameter = method.GetParameters().First();

            // Act
            var typeWithContext = parameter.ToContextualParameter();

            // Assert
            Assert.Equal(Nullability.NotNullable, typeWithContext.Nullability);
            Assert.Equal(Nullability.Nullable, typeWithContext.ElementType.Nullability);
        }

        class NotNullableArrayItemTest
        {
            public void Arrays(string[] arrays)
            {
            }
        }

        [Fact]
        public void NotNullableArrayItem()
        {
            // Arrange
            var method = typeof(NotNullableArrayItemTest).GetMethod(nameof(NotNullableArrayItemTest.Arrays));
            var parameter = method.GetParameters().First();

            // Act
            var typeWithContext = parameter.ToContextualParameter();

            // Assert
            Assert.Equal(Nullability.NotNullable, typeWithContext.Nullability);
            Assert.Equal(Nullability.NotNullable, typeWithContext.ElementType.Nullability);
        }

        class TestFunction
        {
            public Tuple<string, string?> ReferenceTupleFunction()
            {
                return new Tuple<string, string?>("", "");
            }

            public (string, string?) ValueTupleFunction()
            {
                return ("", "");
            }

            public (string, (string?, Tuple<(string, string?), string>, Tuple<int, int?>?)?, (int?, int, string?)) ComplexNestedTypeFunction()
            {
                return default;
            }
        }

        [Fact]
        public void MixedTupleReturnParameter()
        {
            // Arrange
            var method = typeof(TestFunction).GetMethod(nameof(TestFunction.ReferenceTupleFunction));
            var parameter = method.ReturnParameter;

            // Act
            var typeWithContext = parameter.ToContextualParameter();

            // Assert
            Assert.Equal(Nullability.NotNullable, typeWithContext.Nullability);
            Assert.Equal(Nullability.NotNullable, typeWithContext.GenericArguments[0].Nullability);
            Assert.Equal(Nullability.Nullable, typeWithContext.GenericArguments[1].Nullability);
        }

        [Fact]
        public void MixedValueTupleReturnParameter()
        {
            // Arrange
            var method = typeof(TestFunction).GetMethod(nameof(TestFunction.ValueTupleFunction));
            var parameter = method.ReturnParameter;

            // Act
            var typeWithContext = parameter.ToContextualParameter();

            // Assert
            Assert.Equal(Nullability.NotNullable, typeWithContext.Nullability);
            Assert.Equal(Nullability.NotNullable, typeWithContext.GenericArguments[0].Nullability);
            Assert.Equal(Nullability.Nullable, typeWithContext.GenericArguments[1].Nullability);
        }

        [Fact]
        public void ComplexNestedTypeReturnParameter()
        {
            // Arrange
            var method = typeof(TestFunction).GetMethod(nameof(TestFunction.ComplexNestedTypeFunction));
            var parameter = method.ReturnParameter;

            // Act
            var typeWithContext = parameter.ToContextualParameter();

            // Assert
            Assert.Equal(Nullability.NotNullable, typeWithContext.Nullability);
            Assert.Equal(Nullability.NotNullable, typeWithContext.GenericArguments[0].Nullability);
            Assert.Equal(Nullability.Nullable,    typeWithContext.GenericArguments[1].Nullability);
            Assert.Equal(Nullability.NotNullable, typeWithContext.GenericArguments[2].Nullability);

            Assert.Equal(Nullability.Nullable,    typeWithContext.GenericArguments[1].GenericArguments[0].Nullability);
            Assert.Equal(Nullability.NotNullable, typeWithContext.GenericArguments[1].GenericArguments[1].Nullability);
            Assert.Equal(Nullability.Nullable,    typeWithContext.GenericArguments[1].GenericArguments[2].Nullability);

            Assert.Equal(Nullability.Nullable,    typeWithContext.GenericArguments[2].GenericArguments[0].Nullability);
            Assert.Equal(Nullability.NotNullable, typeWithContext.GenericArguments[2].GenericArguments[1].Nullability);
            Assert.Equal(Nullability.Nullable,    typeWithContext.GenericArguments[2].GenericArguments[2].Nullability);

            Assert.Equal(Nullability.NotNullable, typeWithContext.GenericArguments[1].GenericArguments[1].GenericArguments[0].Nullability);
            Assert.Equal(Nullability.NotNullable, typeWithContext.GenericArguments[1].GenericArguments[1].GenericArguments[1].Nullability);

            Assert.Equal(Nullability.NotNullable, typeWithContext.GenericArguments[1].GenericArguments[2].GenericArguments[0].Nullability);
            Assert.Equal(Nullability.Nullable,    typeWithContext.GenericArguments[1].GenericArguments[2].GenericArguments[1].Nullability);

            Assert.Equal(Nullability.NotNullable, typeWithContext.GenericArguments[1].GenericArguments[1].GenericArguments[0].GenericArguments[0].Nullability);
            Assert.Equal(Nullability.Nullable,    typeWithContext.GenericArguments[1].GenericArguments[1].GenericArguments[0].GenericArguments[1].Nullability);
        }

        class TestProperty
        {
            public Tuple<string?> Property { get; set; } = new Tuple<string?>("");
        }

        [Fact]
        public void MixedTupleProperty()
        {
            // Arrange
            var property = typeof(TestProperty).GetProperty(nameof(TestProperty.Property));

            // Act
            var typeWithContext = property.ToContextualProperty();

            // Assert
            Assert.Equal(Nullability.NotNullable, typeWithContext.Nullability);
            Assert.Equal(Nullability.Nullable, typeWithContext.GenericArguments[0].Nullability);
        }

        [Fact]
        public void AssemblyMixedTupleParameter()
        {
            // Arrange
            var method = typeof(FullAssemblyTestAction).GetMethod(nameof(FullAssemblyTestAction.Method));
            var parameters = method.GetParameters();

            // Act & Assert
            Assert.Equal(Nullability.NotNullable, parameters[0].ToContextualParameter().Nullability);
            Assert.Equal(Nullability.Nullable, parameters[1].ToContextualParameter().Nullability);
            Assert.Equal(Nullability.NotNullable, parameters[2].ToContextualParameter().Nullability);
            Assert.Equal(Nullability.Nullable, parameters[3].ToContextualParameter().Nullability);
            Assert.Equal(Nullability.NotNullable, parameters[4].ToContextualParameter().Nullability);
            Assert.Equal(Nullability.Nullable, parameters[5].ToContextualParameter().Nullability);
        }

        [Fact]
        public void AssemblyMixedReturnParameter()
        {
            // Arrange
            var method = typeof(FullAssemblyTestAction).GetMethod(nameof(FullAssemblyTestAction.Method));
            var returnType = method.ReturnParameter.ToContextualParameter();

            // Act & Assert
            Assert.Equal(Nullability.NotNullable, returnType.Nullability);

            Assert.Equal(Nullability.NotNullable, returnType.GenericArguments[0].Nullability);
            Assert.Equal(Nullability.Nullable, returnType.GenericArguments[1].Nullability);
            Assert.Equal(Nullability.NotNullable, returnType.GenericArguments[2].Nullability);
            Assert.Equal(Nullability.Nullable, returnType.GenericArguments[3].Nullability);
            Assert.Equal(Nullability.NotNullable, returnType.GenericArguments[4].Nullability);
            Assert.Equal(Nullability.Nullable, returnType.GenericArguments[5].Nullability);
        }

        [Fact]
        public void AssemblyNotNullableMethodParameters()
        {
            // Arrange
            var method = typeof(FullAssemblyTestAction).GetMethod(nameof(FullAssemblyTestAction.Method2));
            var parameters = method.GetParameters();

            // Act & Assert
            Assert.Equal(Nullability.NotNullable, parameters[0].ToContextualParameter().Nullability);
        }

        [Fact]
        public void AssemblyNotNullableReturnParameter()
        {
            // Arrange
            var method = typeof(FullAssemblyTestAction).GetMethod(nameof(FullAssemblyTestAction.Method2));
            var returnType = method.ReturnParameter.ToContextualParameter();

            // Act & Assert
            Assert.Equal(Nullability.NotNullable, returnType.Nullability);
            Assert.Equal(Nullability.NotNullable, returnType.GenericArguments[0].Nullability);
        }

        [Fact]
        public void AssemblyNullableMethodParameters()
        {
            // Arrange
            var method = typeof(FullAssemblyTestAction).GetMethod(nameof(FullAssemblyTestAction.Method3));
            var parameters = method.GetParameters();

            // Act & Assert
            Assert.Equal(Nullability.Nullable, parameters[0].ToContextualParameter().Nullability);
        }

        [Fact]
        public void AssemblyNullableReturnParameter()
        {
            // Arrange
            var method = typeof(FullAssemblyTestAction).GetMethod(nameof(FullAssemblyTestAction.Method3));
            var returnType = method.ReturnParameter.ToContextualParameter();

            // Act & Assert
            Assert.Equal(Nullability.Nullable, returnType.Nullability);
            Assert.Equal(Nullability.Nullable, returnType.GenericArguments[0].Nullability);
        }

        [Fact]
        public void AssemblyNotNullableStringProperty()
        {
            // Arrange
            var property = typeof(FullAssemblyTestAction).GetProperty(nameof(FullAssemblyTestAction.Property1)).ToContextualMember();

            // Act & Assert
            Assert.Equal(Nullability.NotNullable, property.Nullability);
        }

        [Fact]
        public void AssemblyNullableStringProperty()
        {
            // Arrange
            var property = typeof(FullAssemblyTestAction).GetProperty(nameof(FullAssemblyTestAction.Property2)).ToContextualMember();

            // Act & Assert
            Assert.Equal(Nullability.Nullable, property.Nullability);
        }

        public class MyBase<T1, T2, T3, T4, T5> { }
        public class Derived : MyBase<string?, object, object?, int, int?> { }

        [Fact]
        public void BaseClassWithNullableGenericParameter()
        {
            // Arrange
            var drived = typeof(Derived).ToContextualType();

            // Act & Assert
            Assert.Equal(Nullability.Nullable, drived.BaseType.GenericArguments[0].Nullability);
            Assert.Equal(Nullability.NotNullable, drived.BaseType.GenericArguments[1].Nullability);
            Assert.Equal(Nullability.Nullable, drived.BaseType.GenericArguments[2].Nullability);
            Assert.Equal(Nullability.NotNullable, drived.BaseType.GenericArguments[3].Nullability);
            Assert.Equal(Nullability.Nullable, drived.BaseType.GenericArguments[4].Nullability);
        }

        [Fact]
        public void ConstructorParameters() {

            var constructors = typeof(FullAssemblyTestAction).GetConstructors();

            Assert.Single(constructors);
            var constructor = constructors[0];

            // verify: public FullAssemblyTestAction(string p1, string? p2, int p3, int? p4, Action p5, Action? p6, Tuple<string, string?, int, int?, Action, Action?>? t) { }
            var paramInfos = constructor.GetParameters();
            Assert.Equal(7, paramInfos.Length);

            var paramContexts = paramInfos.Select(p => p.ToContextualParameter()).ToArray();

            Assert.Equal(Nullability.NotNullable, paramContexts[0].Nullability);
            Assert.Equal(Nullability.Nullable, paramContexts[1].Nullability);
            Assert.Equal(Nullability.NotNullable, paramContexts[2].Nullability);
            Assert.Equal(Nullability.Nullable, paramContexts[3].Nullability);
            Assert.Equal(Nullability.NotNullable, paramContexts[4].Nullability);
            Assert.Equal(Nullability.Nullable, paramContexts[5].Nullability);
            Assert.Equal(Nullability.Nullable, paramContexts[6].Nullability);
        }
    }
}
