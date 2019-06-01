using System;
using System.Reflection;

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
}
