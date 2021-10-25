using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Namotion.Reflection.Tests
{
#nullable enable
    public class GenericsTests
    {
        public class UnconstrainedNotNullGenericClass<T>
        {
            public T Prop { get; set; } = default!;

            public T Field = default!;

            public T DoStuff(T original) => original;

            public string NotNullable { get; set; }

            public string? Nullable { get; set; }
        }
        public class UnconstrainedNullableGenericClass<T>
        {
            public T? Prop { get; set; } = default!;

            public T? Field = default!;

            public T? DoStuff(T? original) => original;

            public string NotNullable { get; set; }

            public string? Nullable { get; set; }
        }
        public class NotNullGenericClass<T> where T : notnull
        {
            public T Prop { get; set; } = default!;

            public T Field = default!;

            public T DoStuff(T original) => original;

            public string NotNullable { get; set; }

            public string? Nullable { get; set; }
        }
        public class NullableGenericClass<T> where T : class?
        {
            public T Prop { get; set; } = default!;

            public T Field = default!;

            public T DoStuff(T original) => original;

            public string NotNullable { get; set; }

            public string? Nullable { get; set; }
        }

        public class StructGenericClass<T> where T : struct
        {
            public T Prop { get; set; }

            public T Field = default!;

            public T DoStuff(T original) => original;

            public string NotNullable { get; set; }

            public string? Nullable { get; set; }
        }

        [Fact]
        public void OpenGenericsType()
        {
            void DoTest(ContextualType t, Nullability expectedNullability)
            {
                Assert.Equal(expectedNullability, t.Properties.First(p => p.Name == "Prop")!.Nullability);
                Assert.Equal(Nullability.NotNullable, t.Properties.First(p => p.Name == "NotNullable")!.Nullability);
                Assert.Equal(Nullability.Nullable, t.Properties.First(p => p.Name == "Nullable")!.Nullability);

                var method = t.Methods.First(m => m.Name == "DoStuff")!;
                Assert.Equal(expectedNullability, method.ReturnParameter.Nullability);
                Assert.Equal(expectedNullability, method.Parameters[0].Nullability);
            }

            DoTest(typeof(UnconstrainedNullableGenericClass<>).ToContextualType(), Nullability.Nullable);
            DoTest(typeof(UnconstrainedNotNullGenericClass<>).ToContextualType(), Nullability.NotNullable);
            DoTest(typeof(NotNullGenericClass<>).ToContextualType(), Nullability.NotNullable);
            DoTest(typeof(NullableGenericClass<>).ToContextualType(), Nullability.Nullable);
            DoTest(typeof(StructGenericClass<>).ToContextualType(), Nullability.NotNullable);
        }

        public class ClosedGenericsClass
        {
            public UnconstrainedNotNullGenericClass<string> NotNull1a { get; set; }
            public UnconstrainedNullableGenericClass<string> NotNull1b { get; set; }
            public NotNullGenericClass<string> NotNull2 { get; set; }
            public NullableGenericClass<string> NotNull3 { get; set; }

            public StructGenericClass<int> Struct { get; set; }


            public UnconstrainedNotNullGenericClass<string?> Nullable1a { get; set; } = default!;
            public UnconstrainedNullableGenericClass<string?> Nullable1b { get; set; }
            public NullableGenericClass<string?> Nullable2 { get; set; }

            public string NotNullable { get; set; }

            public string? Nullable { get; set; }
        }

        [Fact]
        public void ClosedGenericsType()
        {
            void DoTest(string propertyName, Nullability expectedNullability)
            {
                var type = typeof(ClosedGenericsClass).ToContextualType();
                var property = type.Properties.First(p => p.Name == propertyName);

                Assert.Equal(expectedNullability, property.PropertyType.GenericArguments[0].Nullability);
                Assert.Equal(expectedNullability, property.PropertyType.GetProperty("Prop")!.Nullability);
                Assert.Equal(expectedNullability, property.PropertyType.GetField("Field")!.Nullability);

                Assert.Equal(Nullability.NotNullable, type.Properties.First(p => p.Name == "NotNullable")!.Nullability);
                Assert.Equal(Nullability.Nullable, type.Properties.First(p => p.Name == "Nullable")!.Nullability);
            }

            DoTest(nameof(ClosedGenericsClass.NotNull1a), Nullability.NotNullable);
            DoTest(nameof(ClosedGenericsClass.NotNull1b), Nullability.NotNullable);
            DoTest(nameof(ClosedGenericsClass.NotNull2), Nullability.NotNullable);
            DoTest(nameof(ClosedGenericsClass.NotNull3), Nullability.NotNullable);
            DoTest(nameof(ClosedGenericsClass.Struct), Nullability.NotNullable);

            DoTest(nameof(ClosedGenericsClass.Nullable1a), Nullability.Nullable);
            DoTest(nameof(ClosedGenericsClass.Nullable1b), Nullability.Nullable);
            DoTest(nameof(ClosedGenericsClass.Nullable2), Nullability.Nullable);
        }
    }
}