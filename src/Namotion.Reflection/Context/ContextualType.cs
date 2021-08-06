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
        private readonly int _nullableFlagsIndex;
        private byte[]? _nullableFlags;
        private Nullability? nullability;

        private ContextualMethodInfo[]? _methods;
        private ContextualPropertyInfo[]? _properties;
        private ContextualFieldInfo[]? _fields;
        private bool? _isValueType;

        internal static ContextualType ForType(Type type, IEnumerable<Attribute> contextAttributes)
        {
            var index = 0;
            return new ContextualType(type, contextAttributes, null, ref index, null, null);
        }

        internal ContextualType(Type type, IEnumerable<Attribute> contextAttributes, ContextualType? parent,
            ref int nullableFlagsIndex, byte[]? nullableFlags, IEnumerable<dynamic>? customAttributeProviders)
            : base(type)
        {
            Parent = parent;
            ContextAttributes = contextAttributes is Attribute[] attributesArray ?
                attributesArray : contextAttributes?.ToArray() ??
                new Attribute[0];

            _nullableFlags = nullableFlags;
            _nullableFlagsIndex = nullableFlagsIndex;

            InitializeNullableFlagsAndOriginalNullability(ref nullableFlagsIndex, customAttributeProviders);

            if (_nullableFlags != null)
            {
                UpdateOriginalGenericArguments(ref nullableFlagsIndex);
            }
        }

        /// <summary>
        /// Gets the parent type with context.
        /// </summary>
        public ContextualType? Parent { get; }

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
                if (_genericArguments is null)
                {
                    throw new InvalidOperationException("_genericArguments is not initialized");
                }
                if (_genericArguments is ContextualType[])
                {
                    return (ContextualType[])_genericArguments;
                }
                else
                {
                    _genericArguments = ((IEnumerable)_genericArguments).Cast<ContextualType>().ToArray();
                    return (ContextualType[])_genericArguments;
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

                if (_originalGenericArguments is null)
                {
                    throw new InvalidOperationException("_originalGenericArguments is not initialized");
                }
                if (_originalGenericArguments is ContextualType[])
                {
                    return (ContextualType[])_originalGenericArguments;
                }
                else
                {
                    _originalGenericArguments = ((IEnumerable)_originalGenericArguments).Cast<ContextualType>().ToArray();
                    return (ContextualType[])_originalGenericArguments;
                }
            }
        }

        /// <summary>
        /// Gets the type's element type (i.e. array type).
        /// </summary>
        public new ContextualType? ElementType
        {
            get
            {
                UpdateOriginalGenericArguments();
                return _elementType as ContextualType;
            }
        }

        private ContextualType? _enumerableItemType;

        /// <summary>
        /// Gets the type's element type (i.e. array type).
        /// </summary>
        public ContextualType? EnumerableItemType
        {
            get
            {
                var elementType = ElementType;
                if (elementType != null)
                {
                    return elementType;
                }

                var getEnumeratorMethod = Methods.SingleOrDefault(m => m.Name == "GetEnumerator");
                if (getEnumeratorMethod != null)
                {
                    if (GenericArguments?.Length == 1)
                    {
                        return GenericArguments[0];
                    }

                    if (_enumerableItemType != null)
                    {
                        return _enumerableItemType;
                    }

                    var returnParam = getEnumeratorMethod.ReturnParameter;
                    if (returnParam?.GenericArguments.Length == 1)
                    {
                        _enumerableItemType = returnParam.GenericArguments[0];
                        return _enumerableItemType;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the type's base type
        /// </summary>
        public ContextualType? BaseType =>
            Type.GetTypeInfo().BaseType?.ToContextualType(Type.GetTypeInfo().GetCustomAttributes());

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
        /// Gets a value indicating whether the System.Type is a value type.
        /// </summary>
        public bool IsValueType => _isValueType ?? ((bool)(_isValueType = TypeInfo.IsValueType));

        /// <summary>
        /// Gets an attribute of the given type which is defined on the context (property, field, parameter or contextual generic argument type).
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attribute or null.</returns>
        public T? GetContextAttribute<T>()
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
        public T? GetAttribute<T>()
        {
            return ContextAttributes.OfType<T>().Concat(InheritedAttributes.OfType<T>()).FirstOrDefault();
        }

        /// <summary>
        /// Gets the attributes of the given type which are defined on the context or on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attributes.</returns>
        public IEnumerable<T> GetAttributes<T>()
        {
            return ContextAttributes.OfType<T>().Concat(InheritedAttributes.OfType<T>());
        }

        /// <summary>
        /// Gets the contextual properties of this type.
        /// </summary>
        public ContextualPropertyInfo[] Properties
        {
            get
            {
                if (_properties == null)
                {
                    lock (this)
                    {
                        if (_properties == null)
                        {
                            _properties = Type
                                .GetRuntimeProperties()
                                .Select(property =>
                                {
                                    if (TypeInfo.IsGenericType && !TypeInfo.ContainsGenericParameters)
                                    {
                                        var genericType = TypeInfo.GetGenericTypeDefinition();
                                        var genericProperty = genericType.GetRuntimeProperty(property.Name);
                                        if (genericProperty != null && genericProperty.PropertyType.IsGenericParameter)
                                        {
                                            var actualType = GenericArguments[genericProperty.PropertyType.GenericParameterPosition];
                                            var actualIndex = actualType._nullableFlagsIndex;
                                            return new ContextualPropertyInfo(property, ref actualIndex, actualType._nullableFlags);
                                        }
                                    }

                                    var index = 0;
                                    return new ContextualPropertyInfo(property, ref index, null);
                                })
                                .ToArray();
                        }
                    }
                }

                return _properties;
            }
        }

        /// <summary>
        /// Gets the contextual methods of this type (runtime).
        /// </summary>
        public ContextualMethodInfo[] Methods
        {
            get
            {
                if (_methods == null)
                {
                    lock (this)
                    {
                        if (_methods == null)
                        {
#if !NET40
                            _methods = Type.GetRuntimeMethods().Select(method =>
#else
                            _methods = Type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Select(method =>
#endif
                            {
                                // TODO: Correctly implement generics here

                                //if (TypeInfo.IsGenericType && !TypeInfo.ContainsGenericParameters)
                                //{
                                //    var genericType = method.DeclaringType.GetGenericTypeDefinition();
                                //    var genericProperty = genericType.GetRuntimeProperty(method.Name);
                                //    if (genericProperty != null)
                                //    {
                                //        var actualType = GenericArguments[genericProperty.PropertyType.GenericParameterPosition];
                                //        var actualIndex = actualType._nullableFlagsIndex;
                                //        return new ContextualMethodInfo(
                                //            method,
                                //            new ContextualParameterInfo(method.ReturnParameter, ref actualIndex, null),
                                //            method
                                //                .GetParameters()
                                //                .Select(p => new ContextualParameterInfo(p, ref actualIndex, actualType?._nullableFlags)));
                                //    }
                                //}

                                var index = 0;
                                return new ContextualMethodInfo(
                                    method,
                                    new ContextualParameterInfo(method.ReturnParameter, ref index, null),
                                    method
                                        .GetParameters()
                                        .Select(p =>
                                        {
                                            var index = 0;
                                            return new ContextualParameterInfo(p, ref index, null);
                                        }));
                            }).ToArray();
                        }
                    }
                }

                return _methods;
            }
        }

        /// <summary>
        /// Gets the contextual properties of this type.
        /// </summary>
        public ContextualFieldInfo[] Fields
        {
            get
            {
                if (_fields == null)
                {
                    lock (this)
                    {
                        if (_fields == null)
                        {
                            _fields = Type.GetRuntimeFields().Select(field =>
                            {
                                if (TypeInfo.IsGenericType && !TypeInfo.ContainsGenericParameters)
                                {
                                    var genericType = field.DeclaringType.GetGenericTypeDefinition();
                                    var genericField = genericType.GetRuntimeField(field.Name);
                                    if (genericField != null)
                                    {
                                        var actualType = GenericArguments[genericField.FieldType.GenericParameterPosition];
                                        var actualIndex = actualType._nullableFlagsIndex;
                                        return new ContextualFieldInfo(field, ref actualIndex, actualType._nullableFlags);
                                    }
                                }

                                var index = 0;
                                return new ContextualFieldInfo(field, ref index, null);
                            }).ToArray();
                        }
                    }
                }

                return _fields;
            }
        }

        /// <summary>
        /// Gets a contextual property of the given contextual type (preserving the context).
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The contextual property or null.</returns>
        public ContextualPropertyInfo? GetProperty(string propertyName)
        {
            return Properties.FirstOrDefault(p => p.Name == propertyName);
        }

        /// <summary>
        /// Gets a contextual field of the given contextual type (preserving the context).
        /// </summary>
        /// <param name="fieldName">The field name.</param>
        /// <returns>The contextual field or null.</returns>
        public ContextualFieldInfo? GetField(string fieldName)
        {
            return Fields.FirstOrDefault(p => p.Name == fieldName);
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
            return new ContextualType(type, ContextAttributes, this, ref nullableFlagsIndex, _nullableFlags, null);
        }

        private void InitializeNullableFlagsAndOriginalNullability(ref int nullableFlagsIndex, IEnumerable<dynamic>? customAttributeProviders)
        {
            var typeInfo = OriginalType.GetTypeInfo();

            try
            {
                if (_nullableFlags == null)
                {
                    var nullableAttribute = ContextAttributes.FirstOrDefault(a => a.GetType().FullName == "System.Runtime.CompilerServices.NullableAttribute");
                    if (nullableAttribute is not null)
                    {
                        _nullableFlags = GetFlagsFromNullableAttribute(nullableAttribute);
                    }
                    else if (typeInfo.IsGenericParameter)
                    {
                        nullableAttribute = typeInfo.GetCustomAttributes().FirstOrDefault(a => a.GetType().FullName == "System.Runtime.CompilerServices.NullableAttribute");
                        if (nullableAttribute is not null)
                        {
                            _nullableFlags = GetFlagsFromNullableAttribute(nullableAttribute);
                        }
                        else
                        {
                            // Default nullability (NullableContextAttribute) from the context
                            _nullableFlags = GetFlagsFromCustomAttributeProviders(typeInfo.DeclaringType.IsNested ? new dynamic[] { typeInfo.DeclaringType, typeInfo.DeclaringType.DeclaringType } : new dynamic[] { typeInfo.DeclaringType });
                        }
                    }
                    else if (customAttributeProviders is not null)
                    {
                        // Default nullability (NullableContextAttribute) from the context
                        _nullableFlags = GetFlagsFromCustomAttributeProviders(customAttributeProviders);
                    }
                    else
                    {
                        _nullableFlags = new byte[] { 0 }; // Unknown
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

            if (typeInfo.IsValueType)
            {
                if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() != typeof(Nullable<>))
                {
                    nullableFlagsIndex++;
                }

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
            return (byte[]?)nullableAttribute?.GetType().GetField("NullableFlags")?.GetValue(nullableAttribute) ?? new byte[0];
#else
            return (byte[]?)nullableAttribute?.GetType().GetRuntimeField("NullableFlags")?.GetValue(nullableAttribute) ?? new byte[] { 0 };
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
