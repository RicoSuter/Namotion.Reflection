using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    public class TypeWithContext : TypeWithoutContext
    {
        private byte[] _nullableFlags;
        private Nullability? nullability;

        internal static TypeWithContext ForType(Type type, Attribute[] contextAttributes)
        {
            var index = 0;
            return new TypeWithContext(type, contextAttributes, null, null, ref index);
        }

        internal TypeWithContext(Type type, Attribute[] contextAttributes, TypeWithContext parent, byte[] nullableFlags, ref int nullableFlagsIndex)
            : base(type)
        {
            Parent = parent;
            ContextAttributes = contextAttributes;
            _nullableFlags = nullableFlags;

            InitializeNullableFlagsAndOriginalNullability(ref nullableFlagsIndex);
            UpdateOriginalGenericArguments(ref nullableFlagsIndex);
        }

        /// <summary>
        /// Gets the parent type with context.
        /// </summary>
        public TypeWithContext Parent { get; }

        /// <summary>
        /// Gets the type's associated attributes of the given context.
        /// </summary>
        public Attribute[] ContextAttributes { get; private set; }

        /// <summary>
        /// Gets the original nullability information of this type in the given context (i.e. without unwrapping Nullable{T}).
        /// </summary>
        public Nullability OriginalNullability { get; private set; }

        /// <summary>
        /// Gets the generic type arguments of the type in the given context (empty when unwrapped from Nullable{T}).
        /// </summary>
        public new TypeWithContext[] GenericArguments
        {
            get
            {
                UpdateOriginalGenericArguments();

                if (genericArguments is TypeWithContext[])
                {
                    return (TypeWithContext[])genericArguments;
                }
                else
                {
                    genericArguments = ((IEnumerable)genericArguments).Cast<TypeWithContext>().ToArray();
                    return (TypeWithContext[])genericArguments;
                }
            }
        }

        /// <summary>
        /// Gets the original generic type arguments of the type in the given context.
        /// </summary
        public new TypeWithContext[] OriginalGenericArguments
        {
            get
            {
                UpdateOriginalGenericArguments();

                if (originalGenericArguments is TypeWithContext[])
                {
                    return (TypeWithContext[])originalGenericArguments;
                }
                else
                {
                    originalGenericArguments = ((IEnumerable)originalGenericArguments).Cast<TypeWithContext>().ToArray();
                    return (TypeWithContext[])originalGenericArguments;
                }
            }
        }

        /// <summary>
        /// Gets the nullability information of this type in the given context by unwrapping Nullable{T} into account.
        /// </summary>
        public Nullability Nullability
        {
            get
            {
                if (nullability.HasValue)
                {
                    return nullability.Value;
                }

                UpdateOriginalGenericArguments();
                lock (this)
                {
                    if (!nullability.HasValue)
                    {
                        nullability = IsNullableType ? Nullability.Nullable : OriginalNullability;
                    }

                    return nullability.Value;
                }
            }
        }

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

        protected override TypeWithoutContext GetTypeInformation(Type type, ref int nullableFlagsIndex)
        {
            return new TypeWithContext(type, ContextAttributes, this, _nullableFlags, ref nullableFlagsIndex);
        }

        private void InitializeNullableFlagsAndOriginalNullability(ref int nullableFlagsIndex)
        {
            try
            {
                if (_nullableFlags == null)
                {
                    var nullableAttribute = ContextAttributes.FirstOrDefault(a => a.GetType().FullName == "System.Runtime.CompilerServices.NullableAttribute");
#if NET40
                    _nullableFlags = (byte[])nullableAttribute?.GetType().GetField("NullableFlags")?.GetValue(nullableAttribute) ?? new byte[0];
#else
                    _nullableFlags = (byte[])nullableAttribute?.GetType().GetRuntimeField("NullableFlags")?.GetValue(nullableAttribute) ?? new byte[0];
#endif
                }
            }
            catch
            {
                _nullableFlags = new byte[0];
            }

            var nullableFlag = _nullableFlags.Length > nullableFlagsIndex ? _nullableFlags[nullableFlagsIndex] : -1;
            nullableFlagsIndex++;

            OriginalNullability = nullableFlag == 0 ? Nullability.NeverNull :
                nullableFlag == 1 ? Nullability.NotNullable :
                nullableFlag == 2 ? Nullability.Nullable :
                Nullability.Unknown;
        }
    }
}
