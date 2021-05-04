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

    internal class PropertyWriter<TObject, TValue> : IPropertyWriter
    {
        private readonly PropertyInfo _propertyInfo;
#if !NET40 && !NETSTANDARD1_0
        private Action<TObject?, TValue?>? _setter;
#endif

        public PropertyWriter(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;

#if !NET40 && !NETSTANDARD1_0
            var method = propertyInfo.SetMethod;
            _setter = method != null ? (Action<TObject?, TValue?>)Delegate.CreateDelegate(typeof(Action<TObject?, TValue?>), null, method) : null;
#endif
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void SetValue(TObject? obj, TValue? value)
        {
#if !NET40 && !NETSTANDARD1_0
            if (_setter != null)
            {
                _setter(obj, value);
            }
            else
            {
                _propertyInfo.SetValue(obj, value);
            }
#else
            _propertyInfo.SetValue(obj, value);
#endif
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        void IPropertyWriter.SetValue(object? obj, object? value)
        {
            SetValue((TObject?)obj, (TValue?)value);
        }
    }
}
