using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    public class TypeWithContext
    {
        public static TypeWithContext ForType(Type type, Attribute[] contextAttributes)
        {
            var index = 0;
            return new TypeWithContext(type, contextAttributes, null, null, ref index);
        }

        internal TypeWithContext(Type type, Attribute[] contextAttributes, TypeWithContext parent, byte[] nullableFlags, ref int nullableFlagsIndex)
        {
            OriginalType = type;
            ContextAttributes = contextAttributes;
            TypeAttributes = type.GetTypeInfo().GetCustomAttributes(true).OfType<Attribute>().ToArray();
            Parent = parent;

            InitializeNullability(nullableFlags, ref nullableFlagsIndex);
            InitializeGenericArguments(type, nullableFlags, ref nullableFlagsIndex);
            CalculateDerivedProperties();
        }

        private void InitializeNullability(byte[] nullableFlags, ref int nullableFlagsIndex)
        {
            try
            {
                if (nullableFlags == null)
                {
                    var nullableAttribute = ContextAttributes.FirstOrDefault(a => a.GetType().FullName == "System.Runtime.CompilerServices.NullableAttribute");
#if NET40
                    nullableFlags = (byte[])nullableAttribute?.GetType().GetField("NullableFlags")?.GetValue(nullableAttribute) ?? new byte[0];
#else
                    nullableFlags = (byte[])nullableAttribute?.GetType().GetRuntimeField("NullableFlags")?.GetValue(nullableAttribute) ?? new byte[0];
#endif
                }
            }
            catch
            {
                nullableFlags = new byte[0];
            }

            var nullableFlag = nullableFlags.Length > nullableFlagsIndex ? nullableFlags[nullableFlagsIndex] : -1;
            nullableFlagsIndex++;

            OriginalNullability = nullableFlag == 0 ? Nullability.NeverNull :
                nullableFlag == 1 ? Nullability.NotNullable :
                nullableFlag == 2 ? Nullability.Nullable :
                Nullability.Unknown;
        }

        private void InitializeGenericArguments(Type type, byte[] nullableFlags, ref int nullableFlagsIndex)
        {
            var genericArguments = new List<TypeWithContext>();
#if NET40
            foreach (var genericArgument in type.GetGenericArguments())
#else
            foreach (var genericArgument in type.GenericTypeArguments)
#endif
            {
                genericArguments.Add(new TypeWithContext(genericArgument, ContextAttributes, this, nullableFlags, ref nullableFlagsIndex));
            }
            OriginalGenericArguments = genericArguments.ToArray();
        }

        private void CalculateDerivedProperties()
        {
            IsNullableType = OriginalType.Name == "Nullable`1";
            Nullability = IsNullableType ? Nullability.Nullable : OriginalNullability;
            GenericArguments = IsNullableType ? new TypeWithContext[0] : OriginalGenericArguments;
            Type = IsNullableType ? OriginalGenericArguments.First().OriginalType : OriginalType;
        }

        /// <summary>
        /// Gets the original type (i.e. without unwrapping Nullable{T}).
        /// </summary>
        public Type OriginalType { get; }

        /// <summary>
        /// Gets the actual unwrapped type (e.g. gets T of Nullable{T} types).
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this type is wrapped with Nullable{T}.
        /// </summary>
        public bool IsNullableType { get; private set; }

        /// <summary>
        /// Gets the original nullability information of this type in the given context (i.e. without unwrapping Nullable{T}).
        /// </summary>
        public Nullability OriginalNullability { get; private set; }

        /// <summary>
        /// Gets the nullability information of this type in the given context by unwrapping Nullable{T} into account.
        /// </summary>
        public Nullability Nullability { get; private set; }

        /// <summary>
        /// Gets the type's associated attributes of the given context.
        /// </summary>
        public Attribute[] ContextAttributes { get; }

        /// <summary>
        /// Gets the type's associated attributes of the type.
        /// </summary>
        public Attribute[] TypeAttributes { get; }

        /// <summary>
        /// Gets the parent type with context.
        /// </summary>
        public TypeWithContext Parent { get; }

        /// <summary>
        /// Gets the original generic type arguments of the type in the given context.
        /// </summary>
        public TypeWithContext[] OriginalGenericArguments { get; private set; }

        /// <summary>
        /// Gets the generic type arguments of the type in the given context (empty when unwrapped from Nullable{T}).
        /// </summary>
        public TypeWithContext[] GenericArguments { get; private set; }

        public T GetTypeAttribute<T>()
        {
            return TypeAttributes.OfType<T>().SingleOrDefault();
        }

        public IEnumerable<T> GetTypeAttributes<T>()
        {
            return TypeAttributes.OfType<T>();
        }

        public T GetContextAttribute<T>()
        {
            return ContextAttributes.OfType<T>().SingleOrDefault();
        }

        public IEnumerable<T> GetContextAttributes<T>()
        {
            return ContextAttributes.OfType<T>();
        }

        public override string ToString()
        {
            var result = Type.Name + ": " + Nullability + "\n  " +
                string.Join("\n", GenericArguments.Select(a => a.ToString())).Replace("\n", "\n  ");

            return result.Trim();
        }
    }
}
