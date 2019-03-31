using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    public class TypeWithContext
    {
        public static TypeWithContext ForType(Type type, Attribute[] attributes)
        {
            var index = 0;
            return new TypeWithContext(type, attributes, null, null, ref index);
        }

        private TypeWithContext(Type type, Attribute[] attributes, TypeWithContext parent, byte[] nullableFlags, ref int nullableFlagsIndex)
        {
            OriginalType = type;
            Attributes = attributes;
            Parent = parent;

            if (nullableFlags == null)
            {
                var nullableAttribute = attributes.FirstOrDefault(a => a.GetType().FullName == "System.Runtime.CompilerServices.NullableAttribute");
                nullableFlags = (byte[])nullableAttribute?.GetType().GetField("NullableFlags")?.GetValue(nullableAttribute) ?? new byte[0];
            }

            var nullableFlag = nullableFlags.Length > nullableFlagsIndex ? nullableFlags[nullableFlagsIndex] : -1;
            nullableFlagsIndex++;

            OriginalNullability = nullableFlag == 0 ? Nullability.NeverNull :
                nullableFlag == 1 ? Nullability.NotNull :
                nullableFlag == 2 ? Nullability.Null :
                Nullability.Unknown;

            var genericArguments = new List<TypeWithContext>();
            foreach (var genericArgument in type.GenericTypeArguments)
            {
                genericArguments.Add(new TypeWithContext(genericArgument, attributes, this, nullableFlags, ref nullableFlagsIndex));
            }
            GenericArguments = genericArguments.ToArray();
        }

        /// <summary>
        /// Gets the original type (i.e. without unwrapping Nullable{T}).
        /// </summary>
        public Type OriginalType { get; }

        /// <summary>
        /// Gets the actual unwrapped type (e.g. gets T of Nullable{T} types).
        /// </summary>
        internal Type Type => IsNullableType ? OriginalType.GenericTypeArguments.First() : OriginalType;

        /// <summary>
        /// Gets a value indicating whether this type is wrapped with Nullable{T}.
        /// </summary>
        public bool IsNullableType => OriginalType.Name == "Nullable`1";

        /// <summary>
        /// Gets the original nullability information of this type in the given context (i.e. without unwrapping Nullable{T}).
        /// </summary>
        public Nullability OriginalNullability { get; }

        /// <summary>
        /// Gets the nullability information of this type in the given context by unwrapping Nullable{T} into account.
        /// </summary>
        public Nullability Nullability => IsNullableType ? Nullability.Null : OriginalNullability;

        /// <summary>
        /// Gets the type's associated attributes of the given context.
        /// </summary>
        public Attribute[] Attributes { get; }

        /// <summary>
        /// Gets the parent type with context.
        /// </summary>
        public TypeWithContext Parent { get; }

        /// <summary>
        /// Gets the generic type arguments of the type in the given context.
        /// </summary>
        public TypeWithContext[] GenericArguments { get; }

        /// <summary>
        /// Gets the type context's parameter info.
        /// </summary>
        public ParameterInfo ParemterInfo { get; internal set; }

        /// <summary>
        /// Gets the type context's property info.
        /// </summary>
        public PropertyInfo PropertyInfo { get; internal set; }

        /// <summary>
        /// Gets the type context's field info.
        /// </summary>
        public FieldInfo FieldInfo { get; internal set; }
    }
}
