using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Namotion.Reflection
{
    internal class PropertyReader<TObject, TValue> : IPropertyReader
    {
        private readonly PropertyInfo _propertyInfo;
        private Func<TObject, TValue> _getter;

        public PropertyReader(PropertyInfo propertyInfo)
        {
#if !NET40 && !NETSTANDARD1_0
            var method = propertyInfo.GetMethod;
            _getter = method != null ? (Func<TObject, TValue>)Delegate.CreateDelegate(typeof(Func<TObject, TValue>), null, method) : new Func<TObject, TValue>(o => default);
#else
            _propertyInfo = propertyInfo;
#endif
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public TValue GetValue(TObject obj)
        {
#if !NET40 && !NETSTANDARD1_0
            return _getter(obj);
#else
            return (TValue)_propertyInfo.GetValue(obj);
#endif
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        object IPropertyReader.GetValue(object obj)
        {
            return GetValue((TObject)obj);
        }
    }
}
