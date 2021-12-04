using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit {}
}

namespace Namotion.Reflection
{
    internal readonly struct NullableFlagsSource
    {
        private readonly record struct CacheKey(Type Type, Assembly? Assembly);
        private static Dictionary<CacheKey, NullableFlagsSource> _typeToAttributeSource = new();

        public readonly byte[]? NullableFlags;

        public static NullableFlagsSource Create(Type type, Assembly? assembly = null)
        {
            var sourceMapping = _typeToAttributeSource;

            var key = new CacheKey(type, assembly);
            if (!sourceMapping.TryGetValue(key, out var source))
            {
                // this is racy logic, but benefits from less synchronization primitives and faster reads
                source = new NullableFlagsSource(type, assembly);
                Interlocked.CompareExchange(ref _typeToAttributeSource, new Dictionary<CacheKey, NullableFlagsSource>(_typeToAttributeSource)
                {
                    [key] = source
                }, sourceMapping);
            }

            return source;
        }

        public static NullableFlagsSource Create(MemberInfo member)
        {
            return new NullableFlagsSource(member);
        }

        private NullableFlagsSource(Type type, Assembly? assembly)
        {
            byte[]? flags = null;

            // assembly level is the normal case
            if (assembly is not null)
            {
                flags = GetNullableFlags(assembly);
            }

            if (flags is null)
            {
                // search type
                flags = GetNullableFlags(type);
            }

            NullableFlags = flags;
        }

        private NullableFlagsSource(MemberInfo memberInfo)
        {
            NullableFlags = GetNullableFlags(memberInfo);
        }

#if !NETSTANDARD1_0 && !NET40
        private static byte[]? GetNullableFlags(ICustomAttributeProvider provider)
        {
            var attributes = provider.GetCustomAttributes(false);
            foreach (var attribute in attributes)
            {
                var type = attribute.GetType();
                if (type.Name == "NullableContextAttribute" && type.Namespace == "System.Runtime.CompilerServices")
                {
#if NET40
                    return new byte[] { (byte) type.GetField("Flag").GetValue(attribute) };
#else
                    return new byte[] { (byte) type.GetRuntimeField("Flag").GetValue(attribute) };
#endif
                }
            }

            return null;
        }
#else
        private static byte[]? GetNullableFlags(object provider)
        {
            var attributes = (IEnumerable<Attribute>) ((dynamic) provider).GetCustomAttributes(false);
            foreach (var attribute in attributes)
            {
                var type = attribute.GetType();
                if (type.Name == "NullableContextAttribute" && type.Namespace == "System.Runtime.CompilerServices")
                {
#if NET40
                    return new byte[] { (byte) type.GetField("Flag").GetValue(attribute) };
#else
                    return new byte[] { (byte) type.GetRuntimeField("Flag").GetValue(attribute) };
#endif
                }
            }

            return null;
        }
#endif

    }
}