using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    /// <summary>
    /// A cached type with context information (e.g. parameter, field, property with nullability).
    /// </summary>
    public class ContextualType : CachedType
    {
        private byte[] _nullableFlags;
        private Nullability? nullability;

        internal static ContextualType ForType(Type type, IEnumerable<Attribute> contextAttributes)
        {
            var index = 0;
            return new ContextualType(type, contextAttributes, null, null, ref index, null);
        }

        internal ContextualType(Type type, IEnumerable<Attribute> contextAttributes, ContextualType parent, byte[] nullableFlags, ref int nullableFlagsIndex, IEnumerable<dynamic> customAttributeProviders)
            : base(type)
        {
            Parent = parent;
            ContextAttributes = contextAttributes is Attribute[]?
                (Attribute[])contextAttributes :
                contextAttributes?.ToArray() ??
                new Attribute[0];

            _nullableFlags = nullableFlags;
            InitializeNullableFlagsAndOriginalNullability(ref nullableFlagsIndex, customAttributeProviders);

            if (_nullableFlags != null)
            {
                UpdateOriginalGenericArguments(ref nullableFlagsIndex);
            }
        }

        /// <summary>
        /// Gets the parent type with context.
        /// </summary>
        public ContextualType Parent { get; }

        /// <summary>
        /// Gets the type's associated attributes of the given context (inherited).
        /// </summary>
        public Attribute[] ContextAttributes { get; private set; }

        /// <summary>
        /// Gets the original nullability information of this type in the given context (i.e. without unwrapping Nullable{T}).
        /// </summary>
        public Nullability OriginalNullability { get; private set; }

        /// <summary>
        /// Gets all contextual and type attributes (in this order).
        /// </summary>
        public override IEnumerable<Attribute> Attributes => ContextAttributes.Concat(base.Attributes);

        /// <summary>
        /// Gets the generic type arguments of the type in the given context (empty when unwrapped from Nullable{T}).
        /// </summary>
        public new ContextualType[] GenericArguments
        {
            get
            {
                UpdateOriginalGenericArguments();

                if (genericArguments is ContextualType[])
                {
                    return (ContextualType[])genericArguments;
                }
                else
                {
                    genericArguments = ((IEnumerable)genericArguments).Cast<ContextualType>().ToArray();
                    return (ContextualType[])genericArguments;
                }
            }
        }

        /// <summary>
        /// Gets the original generic type arguments of the type in the given context.
        /// </summary>
        public new ContextualType[] OriginalGenericArguments
        {
            get
            {
                UpdateOriginalGenericArguments();

                if (originalGenericArguments is ContextualType[])
                {
                    return (ContextualType[])originalGenericArguments;
                }
                else
                {
                    originalGenericArguments = ((IEnumerable)originalGenericArguments).Cast<ContextualType>().ToArray();
                    return (ContextualType[])originalGenericArguments;
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

        /// <inheritdocs />
        public override string ToString()
        {
            var result = Type.Name.Split('`').First() + ": " + Nullability + "\n  " +
                string.Join("\n", GenericArguments.Select(a => a.ToString())).Replace("\n", "\n  ");

            return result.Trim();
        }

        /// <summary>Gets the cached type for the given type and nullable flags index.</summary>
        /// <param name="type">The type.</param>
        /// <param name="nullableFlagsIndex">The flags.</param>
        /// <returns>The cached type.</returns>
        protected override CachedType GetCachedType(Type type, ref int nullableFlagsIndex)
        {
            return new ContextualType(type, ContextAttributes, this, _nullableFlags, ref nullableFlagsIndex, null);
        }

        private void InitializeNullableFlagsAndOriginalNullability(ref int nullableFlagsIndex, IEnumerable<dynamic> customAttributeProviders)
        {
            try
            {
                if (_nullableFlags == null)
                {
                    var nullableAttribute = ContextAttributes.FirstOrDefault(a => a.GetType().FullName == "System.Runtime.CompilerServices.NullableAttribute");
                    if (nullableAttribute != null)
                    {
                        _nullableFlags = GetFlagsFromNullableAttribute(nullableAttribute);
                    }
                    else if (customAttributeProviders != null)
                    {
                        _nullableFlags = GetFlagsFromCustomAttributeProviders(customAttributeProviders);
                    }
                    else
                    {
                        _nullableFlags = new byte[] { 0 };
                    }
                }
            }
            catch
            {
                _nullableFlags = new byte[] { 0 };
#if DEBUG
                throw;
#endif
            }

            if (OriginalType.GetTypeInfo().IsValueType)
            {
                OriginalNullability = Nullability.NotNullable;
            }
            else
            {
                var nullableFlag = _nullableFlags.Length > nullableFlagsIndex ? _nullableFlags[nullableFlagsIndex] : _nullableFlags.Last();
                nullableFlagsIndex++;

                OriginalNullability = nullableFlag == 0 ? Nullability.Unknown :
                    nullableFlag == 1 ? Nullability.NotNullable :
                    nullableFlag == 2 ? Nullability.Nullable :
                    Nullability.Unknown;
            }

        }

        private byte[] GetFlagsFromNullableAttribute(Attribute nullableAttribute)
        {
#if NET40
            return (byte[])nullableAttribute?.GetType().GetField("NullableFlags")?.GetValue(nullableAttribute) ?? new byte[0];
#else
            return (byte[])nullableAttribute?.GetType().GetRuntimeField("NullableFlags")?.GetValue(nullableAttribute) ?? new byte[] { 0 };
#endif
        }

        private byte[] GetFlagsFromCustomAttributeProviders(IEnumerable<dynamic> customAttributeProviders)
        {
            foreach (var provider in customAttributeProviders)
            {
                var attributes = (IEnumerable<object>)provider.GetCustomAttributes(false);
                var nullableContextAttribute = attributes.FirstOrDefault(a => a.GetType().FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
                if (nullableContextAttribute != null)
                {
#if NET40
                    return new byte[] { (byte)nullableContextAttribute.GetType().GetField("Flag").GetValue(nullableContextAttribute) };
#else
                    return new byte[] { (byte)nullableContextAttribute.GetType().GetRuntimeField("Flag").GetValue(nullableContextAttribute) };
#endif
                }
            }

            return new byte[] { 0 };

        }
    }
}
