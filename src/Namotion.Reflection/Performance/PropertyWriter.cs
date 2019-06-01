using System;
using System.Reflection;

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
}
