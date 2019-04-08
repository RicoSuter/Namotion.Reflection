using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    public class TypeWithoutContext
    {
        internal TypeWithoutContext(Type type)
            : this(type, true)
        {
        }

        protected TypeWithoutContext(Type type, bool loadGenericArguments)
        {
            OriginalType = type;
            TypeAttributes = type.GetTypeInfo().GetCustomAttributes(true).OfType<Attribute>().ToArray();

            if (loadGenericArguments)
            {
                InitializeGenericArguments(type);
                UpdateDerivedProperties();
            }
        }

        private void InitializeGenericArguments(Type type)
        {
            var genericArguments = new List<TypeWithoutContext>();
#if NET40
            foreach (var genericArgument in type.GetGenericArguments())
#else
            foreach (var genericArgument in type.GenericTypeArguments)
#endif
            {
                genericArguments.Add(genericArgument.GetTypeWithoutContext());
            }

            OriginalGenericArguments = genericArguments.ToArray();
        }

        /// <summary>
        /// Updates the derived properties.
        /// </summary>
        protected void UpdateDerivedProperties()
        {
            IsNullableType = OriginalType.Name == "Nullable`1";
            GenericArguments = IsNullableType ? new TypeWithoutContext[0] : OriginalGenericArguments;
            Type = IsNullableType ? OriginalGenericArguments.First().OriginalType : OriginalType;
        }

        /// <summary>
        /// Gets the original type (i.e. without unwrapping Nullable{T}).
        /// </summary>
        public Type OriginalType { get; }

        /// <summary>
        /// Gets the actual unwrapped type (e.g. gets T of a Nullable{T} type).
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this type is wrapped with Nullable{T}.
        /// </summary>
        public bool IsNullableType { get; private set; }

        /// <summary>
        /// Gets the type's associated attributes of the type.
        /// </summary>
        public Attribute[] TypeAttributes { get; }

        /// <summary>
        /// Gets the type's generic arguments (Nullable{T} is unwrapped).
        /// </summary>
        public TypeWithoutContext[] GenericArguments { get; protected set; }

        /// <summary>
        /// Gets the type's original generic arguments (Nullable{T} is not unwrapped).
        /// </summary>
        public TypeWithoutContext[] OriginalGenericArguments { get; protected set; }

        /// <summary>
        /// Gets an attribute of the given type which is defined on the type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attribute or null.</returns>
        public T GetTypeAttribute<T>()
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
    }
}
