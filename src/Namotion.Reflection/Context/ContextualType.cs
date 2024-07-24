using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    /// <summary>
    /// A cached type with context information (e.g. parameter, field, property with nullability).
    /// </summary>
    public class ContextualType : CachedType
    {
        private static readonly byte[] _emptyNullableFlags = { 0 };

        private readonly int _nullableFlagsIndex;

        private byte[]? _nullableFlags;
        private Nullability? nullability;

        private ContextualMethodInfo[]? _methods;
        private ContextualPropertyInfo[]? _properties;
        private ContextualFieldInfo[]? _fields;
        private bool? _isValueType;

        internal static ContextualType ForType(Type type, ICustomAttributeProvider? contextualAttributeProvider)
        {
            var index = 0;
            return new ContextualType(type, contextualAttributeProvider ?? new GenericTypeContext(Array.Empty<Attribute>()), null, ref index, null, null);
        }

        internal ContextualType(Type type, ICustomAttributeProvider contextualAttributeProvider, ContextualType? parent,
            ref int nullableFlagsIndex, byte[]? nullableFlags, NullableFlagsSource[]? customAttributeProviders)
            : base(type)
        {
            Parent = parent;

            Context = contextualAttributeProvider;

            _nullableFlags = nullableFlags;
            _nullableFlagsIndex = nullableFlagsIndex;

            InitializeNullableFlagsAndOriginalNullability(ref nullableFlagsIndex, customAttributeProviders);

            if (_nullableFlags != null)
            {
                Initialize(ref nullableFlagsIndex);
            }
        }

        /// <summary>
        /// Gets the context (property, field, parameter).
        /// </summary>
        public ICustomAttributeProvider Context { get; }

        /// <summary>
        /// Gets the parent type with context.
        /// </summary>
        public ContextualType? Parent { get; }

        /// <summary>
        /// Gets the original nullability information of this type in the given context (i.e. without unwrapping Nullable{T}).
        /// </summary>
        public Nullability OriginalNullability { get; private set; }

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
                try
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
                        if (returnParam?.ParameterType.GenericArguments.Length == 1)
                        {
                            _enumerableItemType = returnParam.ParameterType.GenericArguments[0];
                            return _enumerableItemType;
                        }
                    }

                    return null;
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException($"Failed to retrieve enumerable item type of {Type.FullName}.", exception);
                }
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
                                if (field.DeclaringType is { IsGenericType: true })
                                {
                                    var genericType = field.DeclaringType.GetGenericTypeDefinition();
                                    var genericField = genericType.GetRuntimeField(field.Name);
                                    if (genericField != null && genericField.FieldType.IsGenericParameter)
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
                                        var genericProperty = genericType
                                            .GetRuntimeProperties()
                                            .SingleOrDefault(p => p.Name == property.Name && p.DeclaringType == genericType);

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
                            _methods = Type.GetRuntimeMethods().Select(method =>
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

        /// <inheritdoc />
        public override string ToString()
        {
            var result = Type.Name.FirstToken('`') + ": " + Nullability + "\n  " +
                string.Join("\n", GenericArguments.Select(a => a.ToString())).Replace("\n", "\n  ");

            return result.Trim();
        }

        /// <summary>Gets the cached type for the given type and nullable flags index.</summary>
        /// <param name="type">The type.</param>
        /// <param name="nullableFlagsIndex">The flags.</param>
        /// <returns>The cached type.</returns>
        protected override CachedType GetCachedType(Type type, ref int nullableFlagsIndex)
        {
            return new ContextualType(type, Context, this, ref nullableFlagsIndex, _nullableFlags, null);
        }

        private void InitializeNullableFlagsAndOriginalNullability(ref int nullableFlagsIndex, NullableFlagsSource[]? customAttributeProviders)
        {
            var typeInfo = OriginalType.GetTypeInfo();
            try
            {
                if (_nullableFlags == null)
                {
                    var nullableAttribute = Context?
                        .GetCustomAttributes(true)
                        .FirstOrDefault(a => a.GetType().FullName == "System.Runtime.CompilerServices.NullableAttribute");

                    if (nullableAttribute is not null)
                    {
                        _nullableFlags = GetFlagsFromNullableAttribute(nullableAttribute);
                    }
                    else if (typeInfo.IsGenericParameter)
                    {
                        if (typeInfo.GenericParameterAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint) || // specifically a struct - existing code works
                            typeInfo.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint)) // specifically a class - existing code works
                        {
                            nullableAttribute = typeInfo.GetCustomAttributes().FirstOrDefault(a => a.GetType().FullName == "System.Runtime.CompilerServices.NullableAttribute");
                            if (nullableAttribute is not null)
                            {
                                _nullableFlags = GetFlagsFromNullableAttribute(nullableAttribute);
                            }
                            else
                            {
                                // Default nullability (NullableContextAttribute) from the context
                                var attributeProviders = typeInfo.DeclaringType.IsNested
                                    ? new[] { NullableFlagsSource.Create(typeInfo.DeclaringType), NullableFlagsSource.Create(typeInfo.DeclaringType.DeclaringType) }
                                    : new[] { NullableFlagsSource.Create(typeInfo.DeclaringType) };

                                _nullableFlags = GetFlagsFromCustomAttributeProviders(attributeProviders);
                            }
                        }
                        else
                        {
                            // unconstrained generic - take nullability from the context
                            if (customAttributeProviders is not null)
                            {
                                _nullableFlags = GetFlagsFromCustomAttributeProviders(customAttributeProviders);
                            }
                            else
                            {
                                _nullableFlags = _emptyNullableFlags; // Unknown
                            }
                        }

                    }
                    else if (customAttributeProviders is not null)
                    {
                        // Default nullability (NullableContextAttribute) from the context
                        _nullableFlags = GetFlagsFromCustomAttributeProviders(customAttributeProviders);
                    }
                    else
                    {
                        _nullableFlags = _emptyNullableFlags; // Unknown
                    }
                }
            }
            catch
            {
                _nullableFlags = _emptyNullableFlags;
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
                var nullableFlag = _nullableFlags!.Length > nullableFlagsIndex ? _nullableFlags[nullableFlagsIndex] : _nullableFlags.Last();
                nullableFlagsIndex++;

                OriginalNullability = nullableFlag == 0 ? Nullability.Unknown :
                    nullableFlag == 1 ? Nullability.NotNullable :
                    nullableFlag == 2 ? Nullability.Nullable :
                    Nullability.Unknown;
            }
        }

        private byte[] GetFlagsFromNullableAttribute(object nullableAttribute)
        {
            return (byte[]?)nullableAttribute?.GetType().GetRuntimeField("NullableFlags")?.GetValue(nullableAttribute) ?? _emptyNullableFlags;
        }

        private static byte[]? GetFlagsFromCustomAttributeProviders(NullableFlagsSource[] customAttributeProviders)
        {
            foreach (var provider in customAttributeProviders)
            {
                var flags = provider.NullableFlags;
                if (flags is not null)
                {
                    return flags;
                }
            }

            return _emptyNullableFlags;
        }
    }
}
