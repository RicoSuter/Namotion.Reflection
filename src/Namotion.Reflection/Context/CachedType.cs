using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Namotion.Reflection
{
    /// <summary>
    /// A cached type object without context.
    /// </summary>
    public class CachedType
    {
        private Type _type;
        private bool _isNullableType;
        private string _typeName;

        private Attribute[] typeAttributes;

        /// <summary>
        /// Internal generic arguments.
        /// </summary>
        protected object genericArguments;

        /// <summary>
        /// Internal original generic arguments.
        /// </summary>
        protected object originalGenericArguments;

        /// <summary>
        /// Unwraps the OriginalType as <see cref="Type"/> from the context type.
        /// </summary>
        /// <param name="type">The contextual type</param>
        public static implicit operator Type(CachedType type)
        {
            return type.OriginalType;
        }

        internal CachedType(Type type)
        {
            OriginalType = type;
        }

        /// <summary>
        /// Gets the original type (i.e. without unwrapping Nullable{T}).
        /// </summary>
        public Type OriginalType { get; }

        /// <summary>
        /// Gets all type attributes.
        /// </summary>
        public virtual IEnumerable<Attribute> Attributes => TypeAttributes;

        /// <summary>
        /// Gets the type name.
        /// </summary>
        public string TypeName => _typeName ?? (_typeName = Type.Name);

        /// <summary>
        /// Gest the original's type info.
        /// </summary>
#if !NET40
        public TypeInfo TypeInfo => _typeInfo ?? (_typeInfo = Type.GetTypeInfo());
        private TypeInfo _typeInfo;
#else
        public Type TypeInfo => Type;
#endif
        /// <summary>
        /// Gets the type's associated attributes of the type (inherited).
        /// </summary>
        public Attribute[] TypeAttributes
        {
            get
            {
                if (typeAttributes != null)
                {
                    return typeAttributes;
                }

                UpdateOriginalGenericArguments();
                lock (this)
                {
                    if (typeAttributes == null)
                    {
                        // TODO: rename to inherited type attributes and add type attributes property
                        typeAttributes = _type.GetTypeInfo().GetCustomAttributes(true).OfType<Attribute>().ToArray();
                    }

                    return typeAttributes;
                }
            }
        }

        /// <summary>
        /// Gets the actual unwrapped type (e.g. gets T of a Nullable{T} type).
        /// </summary>
        public Type Type
        {
            get
            {
                UpdateOriginalGenericArguments();
                return _type;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this type is wrapped with Nullable{T}.
        /// </summary>
        public bool IsNullableType
        {
            get
            {
                UpdateOriginalGenericArguments();
                return _isNullableType;
            }
        }

        /// <summary>
        /// Gets the type's generic arguments (Nullable{T} is unwrapped).
        /// </summary>
        public CachedType[] GenericArguments
        {
            get
            {
                UpdateOriginalGenericArguments();
                return (CachedType[])genericArguments;
            }
        }

        /// <summary>
        /// Gets the type's original generic arguments (Nullable{T} is not unwrapped).
        /// </summary>
        public CachedType[] OriginalGenericArguments
        {
            get
            {
                UpdateOriginalGenericArguments();
                return (CachedType[])originalGenericArguments;
            }
        }

        /// <summary>
        /// Gets an attribute of the given type which is defined on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attribute or null.</returns>
        public T GetTypeAttribute<T>()
            where T : Attribute
        {
            return TypeAttributes.OfType<T>().SingleOrDefault();
        }

        /// <summary>
        /// Gets the attributes of the given type which are defined on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attributes.</returns>
        public IEnumerable<T> GetTypeAttributes<T>()
        {
            return TypeAttributes.OfType<T>();
        }

        /// <inheritdocs />
        public override string ToString()
        {
            var result = Type.Name.Split('`').First() + "\n  " +
                string.Join("\n", GenericArguments.Select(a => a.ToString())).Replace("\n", "\n  ");

            return result.Trim();
        }

        /// <summary>Gets the cached type for the given type and nullable flags index.</summary>
        /// <param name="type">The type.</param>
        /// <param name="nullableFlagsIndex">The flags.</param>
        /// <returns>The cached type.</returns>
        protected virtual CachedType GetCachedType(Type type, ref int nullableFlagsIndex)
        {
            return type.ToCachedType();
        }

        /// <summary>
        /// Updates the original generic arguments.
        /// </summary>
#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected void UpdateOriginalGenericArguments()
        {
            var nullableFlagsIndex = 0;
            UpdateOriginalGenericArguments(ref nullableFlagsIndex);
        }

        /// <summary>
        /// Updates the original generic arguments.
        /// </summary>
#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected void UpdateOriginalGenericArguments(ref int nullableFlagsIndex)
        {
            if (originalGenericArguments == null)
            {
                lock (this)
                {
                    if (originalGenericArguments == null)
                    {
                        var arguments = new List<CachedType>();
#if NET40
                        foreach (var type in OriginalType.GetGenericArguments())
#else
                        foreach (var type in OriginalType.GenericTypeArguments)
#endif
                        {
                            arguments.Add(GetCachedType(type, ref nullableFlagsIndex));
                        }

                        originalGenericArguments = arguments.ToArray();
                        _isNullableType = OriginalType.Name == "Nullable`1";
                        genericArguments = _isNullableType ? new CachedType[0] : originalGenericArguments;
                        _type = _isNullableType ? ((IEnumerable)originalGenericArguments).Cast<CachedType>().First().OriginalType : OriginalType;
                    }
                }
            }
        }
    }
}
