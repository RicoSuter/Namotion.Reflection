using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Namotion.Reflection.Tests
{
#nullable enable
    public class GenericsTests
    {
        public class UnconstrainedGenericClass<T>
        {
            public T Prop { get; set; } = default!;

            public T Field = default!;

            public T DoStuff(T original) => original;
        }
        public class NotNullGenericClass<T> where T : notnull
        {
            public T Prop { get; set; } = default!;

            public T Field = default!;

            public T DoStuff(T original) => original;
        }
        public class NullableGenericClass<T> where T : class?
        {
            public T Prop { get; set; } = default!;

            public T Field = default!;

            public T DoStuff(T original) => original;
        }

        public class StructGenericClass<T> where T : struct
        {
            public T Prop { get; set; }

            public T Field = default!;

            public T DoStuff(T original) => original;
        }

        [Fact]
        public void OpenGenericsType()
        {
            void DoTest(ContextualType t, Nullability expectedNullability)
            {
                Assert.Equal(expectedNullability, t.GenericArguments[0].Nullability);
                Assert.Equal(expectedNullability, t.Properties.First(p => p.Name == "Prop")!.Nullability);

                var method = t.Methods.First(m => m.Name == "DoStuff")!;
                Assert.Equal(expectedNullability, method.ReturnParameter.Nullability);
                Assert.Equal(expectedNullability, method.Parameters[0].Nullability);
            }

            DoTest(typeof(UnconstrainedGenericClass<>).ToContextualType(), Nullability.Nullable);
            DoTest(typeof(NotNullGenericClass<>).ToContextualType(), Nullability.NotNullable);
            DoTest(typeof(NullableGenericClass<>).ToContextualType(), Nullability.Nullable);
            DoTest(typeof(StructGenericClass<>).ToContextualType(), Nullability.NotNullable);
        }

        public class ClosedGenericsClass
        {
            public UnconstrainedGenericClass<string> NotNull1 { get; set; }
            public NotNullGenericClass<string> NotNull2 { get; set; }
            public NullableGenericClass<string> NotNull3 { get; set; }

            public StructGenericClass<int> Struct { get; set; }


            public UnconstrainedGenericClass<string?> Nullable1 { get; set; }
            public NullableGenericClass<string?> Nullable2 { get; set; }
        }

        [Fact]
        public void ClosedGenericsType()
        {
            void DoTest(string propertyName, Nullability expectedNullability)
            {
                var property = typeof(ClosedGenericsClass).GetProperty(propertyName)!.ToContextualProperty();

                Assert.Equal(expectedNullability, property.PropertyType.GenericArguments[0].Nullability);
                Assert.Equal(expectedNullability, property.PropertyType.GetProperty("Prop")!.Nullability);
                Assert.Equal(expectedNullability, property.PropertyType.GetField("Field")!.Nullability);
            }

            DoTest(nameof(ClosedGenericsClass.NotNull1), Nullability.NotNullable);
            DoTest(nameof(ClosedGenericsClass.NotNull2), Nullability.NotNullable);
            DoTest(nameof(ClosedGenericsClass.NotNull3), Nullability.NotNullable);
            DoTest(nameof(ClosedGenericsClass.Struct), Nullability.NotNullable);

            DoTest(nameof(ClosedGenericsClass.Nullable1), Nullability.Nullable);
            DoTest(nameof(ClosedGenericsClass.Nullable2), Nullability.Nullable);
        }
    }
}