using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Namotion.Reflection
{
    internal class PropertyWriter<TObject, TValue> : IPropertyWriter
    {
        private readonly PropertyInfo _propertyInfo;
        private Action<TObject, TValue> _setter;

        public PropertyWriter(PropertyInfo propertyInfo)
        {
#if !NET40 && !NETSTANDARD1_0
            var method = propertyInfo.SetMethod;
            _setter = method != null ? (Action<TObject, TValue>)Delegate.CreateDelegate(typeof(Action<TObject, TValue>), null, method) : new Action<TObject, TValue>((o, v) => { });
#else
            _propertyInfo = propertyInfo;
#endif
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void SetValue(TObject obj, TValue value)
        {
#if !NET40 && !NETSTANDARD1_0
            _setter(obj, value);
#else
            _propertyInfo.SetValue((TObject)obj, (TValue)value);
#endif
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        void IPropertyWriter.SetValue(object obj, object value)
        {
            SetValue((TObject)obj, (TValue)value);
        }
    }
}
