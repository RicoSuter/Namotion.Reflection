using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Namotion.Reflection
{
    internal static class PropertyReader
    {
        private static Type GenericTypeDefinition = typeof(PropertyReader<object, object>).GetGenericTypeDefinition();

        public static IPropertyReader Create(Type objectType, Type valueType, PropertyInfo propertyInfo)
        {
            var type = GenericTypeDefinition.MakeGenericType(objectType, valueType);
            return (IPropertyReader)Activator.CreateInstance(type, propertyInfo);
        }
    }

    internal class PropertyReader<TObject, TValue> : IPropertyReader
    {
        private readonly PropertyInfo _propertyInfo;
#if !NET40 && !NETSTANDARD1_1
        private Func<TObject?, TValue?>? _getter;
#endif

        public PropertyReader(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;

#if !NET40 && !NETSTANDARD1_1
            var method = propertyInfo.GetMethod;
            _getter = method != null ? (Func<TObject?, TValue?>)Delegate.CreateDelegate(typeof(Func<TObject?, TValue?>), null, method) : null;
#endif
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public TValue? GetValue(TObject? obj)
        {
#if !NET40 && !NETSTANDARD1_1
            return _getter != null ? _getter(obj) : (TValue)_propertyInfo.GetValue(obj);
#else
            return (TValue?)_propertyInfo.GetValue(obj);
#endif
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        object? IPropertyReader.GetValue(object? obj)
        {
            return GetValue((TObject?)obj);
        }
    }
}
