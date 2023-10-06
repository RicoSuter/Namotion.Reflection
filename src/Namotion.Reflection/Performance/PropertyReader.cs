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

    internal sealed class PropertyReader<TObject, TValue> : IPropertyReader
    {
        private readonly PropertyInfo _propertyInfo;
        private Func<TObject?, TValue?>? _getter;

        public PropertyReader(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;

            var method = propertyInfo.GetMethod;
            _getter = method != null ? (Func<TObject?, TValue?>)Delegate.CreateDelegate(typeof(Func<TObject?, TValue?>), null, method) : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue? GetValue(TObject? obj)
        {
            return _getter != null ? _getter(obj) : (TValue)_propertyInfo.GetValue(obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object? IPropertyReader.GetValue(object? obj)
        {
            return GetValue((TObject?)obj);
        }
    }
}
