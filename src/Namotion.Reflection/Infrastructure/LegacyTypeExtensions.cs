﻿//-----------------------------------------------------------------------
// <copyright file="ReflectionExtensions.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
#if NET40

    public static class LegacyTypeExtensions
    {
        public static MethodInfo? GetRuntimeMethod(this Type type, string name, Type[] types)
        {
            return type.GetMethod(name, types);
        }

        public static PropertyInfo? GetRuntimeProperty(this Type type, string name)
        {
            return type.GetProperty(name);
        }

        public static FieldInfo? GetDeclaredField(this Type type, string name)
        {
            return type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        }

        public static PropertyInfo[] GetRuntimeProperties(this Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static Type GetTypeInfo(this Type type)
        {
            return type;
        }

        public static Attribute[] GetCustomAttributes(this FieldInfo fieldInfo, bool inherit = true)
        {
            return fieldInfo.GetCustomAttributes(inherit).OfType<Attribute>().ToArray();
        }

        public static Attribute[] GetCustomAttributes(this Type type, bool inherit = true)
        {
            return type.GetCustomAttributes(inherit).OfType<Attribute>().ToArray();
        }

        public static Attribute[] GetCustomAttributes(this PropertyInfo propertyInfo, bool inherit = true)
        {
            return propertyInfo.GetCustomAttributes(inherit).OfType<Attribute>().ToArray();
        }

        public static T[] GetCustomAttributes<T>(this Type type, bool inherit = true)
            where T : Attribute
        {
            return type.GetCustomAttributes(inherit).OfType<T>().ToArray();
        }

        public static T[] GetCustomAttributes<T>(this PropertyInfo propertyInfo, bool inherit = true)
            where T : Attribute
        {
            return propertyInfo.GetCustomAttributes(inherit).OfType<T>().ToArray();
        }

        public static T? GetCustomAttribute<T>(this Type type)
            where T : Attribute
        {
            return type.GetCustomAttributes().OfType<T>().FirstOrDefault();
        }

        public static T? GetCustomAttribute<T>(this PropertyInfo propertyInfo)
            where T : Attribute
        {
            return propertyInfo.GetCustomAttributes().OfType<T>().FirstOrDefault();
        }

        public static object? GetValue(this PropertyInfo propertyInfo, object? obj)
        {
            return propertyInfo.GetValue(obj, null);
        }

        public static void SetValue(this PropertyInfo propertyInfo, object? obj, object? value)
        {
            propertyInfo.SetValue(obj, value, null);
        }
    }

#endif
}