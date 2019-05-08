using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    public class TypeWithContext : TypeWithoutContext
    {
        /// <summary>
        /// Creates a <see cref="TypeWithContext"/> with an empty context for the given type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The <see cref="TypeWithContext"/>.</returns>
        public static TypeWithContext ForType(Type type)
        {
            return ForType(type, new Attribute[0]);
        }

        /// <summary>
        /// Creates a <see cref="TypeWithContext"/> the given type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="contextAttributes">The contextual attributes.</param>
        /// <returns>The <see cref="TypeWithContext"/>.</returns>
        public static TypeWithContext ForType(Type type, Attribute[] contextAttributes)
        {
            var index = 0;
            return new TypeWithContext(type, contextAttributes, null, null, ref index);
        }

        internal TypeWithContext(Type type, Attribute[] contextAttributes, TypeWithContext parent, byte[] nullableFlags, ref int nullableFlagsIndex)
            : base(type, false)
        {
            Parent = parent;
            ContextAttributes = contextAttributes;

            InitializeNullability(nullableFlags, ref nullableFlagsIndex);
            InitializeGenericArguments(type, nullableFlags, ref nullableFlagsIndex);
            UpdateDerivedProperties();

            Nullability = IsNullableType ? Nullability.Nullable : OriginalNullability;
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
            GenericArguments = IsNullableType ? new TypeWithContext[0] : OriginalGenericArguments;

            base.OriginalGenericArguments = OriginalGenericArguments;
        }

        /// <summary>
        /// Gets the parent type with context.
        /// </summary>
        public TypeWithContext Parent { get; }

        /// <summary>
        /// Gets the original generic type arguments of the type in the given context.
        /// </summary>
        public new TypeWithContext[] OriginalGenericArguments { get; private set; }

        /// <summary>
        /// Gets the generic type arguments of the type in the given context (empty when unwrapped from Nullable{T}).
        /// </summary>
        public new TypeWithContext[] GenericArguments { get; private set; }

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
        /// Gets an attribute of the given type which is defined on the context (property, field, parameter or contextual generic argument type).
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attribute or null.</returns>
        public T GetContextAttribute<T>()
        {
            return ContextAttributes.OfType<T>().SingleOrDefault();
        }

        /// <summary>
        /// Gets the attributes of the given type which are defined on the context (property, field, parameter or contextual generic argument type).
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attributes.</returns>
        public IEnumerable<T> GetContextAttributes<T>()
        {
            return ContextAttributes.OfType<T>();
        }

        /// <summary>
        /// Gets an attribute of the given type which is defined on the context or on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attribute or null.</returns>
        public T GetAttribute<T>()
        {
            return ContextAttributes.OfType<T>().Concat(TypeAttributes.OfType<T>()).FirstOrDefault();
        }

        /// <summary>
        /// Gets the attributes of the given type which are defined on the context or on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attributes.</returns>
        public IEnumerable<T> GetAttributes<T>()
        {
            return ContextAttributes.OfType<T>().Concat(TypeAttributes.OfType<T>());
        }

        public override string ToString()
        {
            var result = Type.Name.Split('`').First() + ": " + Nullability + "\n  " +
                string.Join("\n", GenericArguments.Select(a => a.ToString())).Replace("\n", "\n  ");

            return result.Trim();
        }
    }
}
