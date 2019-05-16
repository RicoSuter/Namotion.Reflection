using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Namotion.Reflection
{
    public class TypeWithoutContext
    {
        private bool isNullableType;
        private Type type;

        protected object genericArguments;
        protected object originalGenericArguments;

        private Attribute[] typeAttributes;

        internal TypeWithoutContext(Type type)
        {
            OriginalType = type;
        }

        /// <summary>
        /// Gets the original type (i.e. without unwrapping Nullable{T}).
        /// </summary>
        public Type OriginalType { get; }

        /// <summary>
        /// Gets the type's associated attributes of the type.
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
                        typeAttributes = type.GetTypeInfo().GetCustomAttributes(true).OfType<Attribute>().ToArray();
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
                return type;
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
                return isNullableType;
            }
        }

        /// <summary>
        /// Gets the type's generic arguments (Nullable{T} is unwrapped).
        /// </summary>
        public TypeWithoutContext[] GenericArguments
        {
            get
            {
                UpdateOriginalGenericArguments();
                return (TypeWithoutContext[])genericArguments;
            }
        }

        /// <summary>
        /// Gets the type's original generic arguments (Nullable{T} is not unwrapped).
        /// </summary>
        public TypeWithoutContext[] OriginalGenericArguments
        {
            get
            {
                UpdateOriginalGenericArguments();
                return (TypeWithoutContext[])originalGenericArguments;
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

        public override string ToString()
        {
            var result = Type.Name.Split('`').First() + "\n  " +
                string.Join("\n", GenericArguments.Select(a => a.ToString())).Replace("\n", "\n  ");

            return result.Trim();
        }

        protected virtual TypeWithoutContext GetTypeInformation(Type type, ref int nullableFlagsIndex)
        {
            return type.GetTypeWithoutContext();
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected void UpdateOriginalGenericArguments()
        {
            var nullableFlagsIndex = 0;
            UpdateOriginalGenericArguments(ref nullableFlagsIndex);
        }

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
                        var arguments = new List<TypeWithoutContext>();
#if NET40
                        foreach (var type in OriginalType.GetGenericArguments())
#else
                        foreach (var type in OriginalType.GenericTypeArguments)
#endif
                        {
                            arguments.Add(GetTypeInformation(type, ref nullableFlagsIndex));
                        }

                        originalGenericArguments = arguments.ToArray();
                        isNullableType = OriginalType.Name == "Nullable`1";
                        genericArguments = isNullableType ? new TypeWithoutContext[0] : originalGenericArguments;
                        type = isNullableType ? ((IEnumerable)originalGenericArguments).Cast<TypeWithoutContext>().First().OriginalType : OriginalType;
                    }
                }
            }
        }
    }
}
