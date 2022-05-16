using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Namotion.Reflection
{
    internal static class PropertyWriter
    {
        private static Type GenericTypeDefinition = typeof(PropertyWriter<object, object>).GetGenericTypeDefinition();

        public static IPropertyWriter Create(Type objectType, Type valueType, PropertyInfo propertyInfo)
        {
            var type = GenericTypeDefinition.MakeGenericType(objectType, valueType);
            return (IPropertyWriter)Activator.CreateInstance(type, propertyInfo);
        }
    }

    internal sealed class PropertyWriter<TObject, TValue> : IPropertyWriter
    {
        private readonly PropertyInfo _propertyInfo;
        private Action<TObject?, TValue?>? _setter;

        public PropertyWriter(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;

            var method = propertyInfo.SetMethod;
            _setter = method != null ? (Action<TObject?, TValue?>)Delegate.CreateDelegate(typeof(Action<TObject?, TValue?>), null, method) : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(TObject? obj, TValue? value)
        {
            if (_setter != null)
            {
                _setter(obj, value);
            }
            else
            {
                _propertyInfo.SetValue(obj, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IPropertyWriter.SetValue(object? obj, object? value)
        {
            SetValue((TObject?)obj, (TValue?)value);
        }
    }
}
