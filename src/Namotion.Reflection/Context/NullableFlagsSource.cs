using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Namotion.Reflection
{
    internal readonly struct NullableFlagsSource
    {
        private readonly record struct CacheKey(Type Type, Assembly? Assembly);
        private static Dictionary<CacheKey, NullableFlagsSource> _nullableCache = new();

        public readonly byte[]? NullableFlags;

        public static NullableFlagsSource Create(Type type, Assembly? assembly = null)
        {
            var nullableCache = _nullableCache;

            var key = new CacheKey(type, assembly);
            if (!nullableCache.TryGetValue(key, out var source))
            {
                // this is racy logic, but benefits from less synchronization primitives and faster reads
                source = new NullableFlagsSource(type, assembly);
                Interlocked.CompareExchange(ref _nullableCache, new Dictionary<CacheKey, NullableFlagsSource>(nullableCache)
                {
                    [key] = source
                }, nullableCache);
            }

            return source;
        }

        public static NullableFlagsSource Create(MemberInfo member)
        {
            return new NullableFlagsSource(member);
        }

        private NullableFlagsSource(Type type, Assembly? assembly)
        {
            var flags = GetNullableFlags(type);

            if (flags is null && assembly is not null)
            {
                flags = GetNullableFlags(assembly);
            }

            NullableFlags = flags;
        }

        private NullableFlagsSource(MemberInfo memberInfo)
        {
            NullableFlags = GetNullableFlags(memberInfo);
        }

        private static byte[]? GetNullableFlags<T>(T provider) where T : ICustomAttributeProvider
        {
            var attributes = provider.GetCustomAttributes(false);
            foreach (var attribute in attributes)
            {
                var type = attribute.GetType();
                if (type.Name == "NullableContextAttribute" && type.Namespace == "System.Runtime.CompilerServices")
                {
                    return new byte[] { (byte) type.GetRuntimeField("Flag").GetValue(attribute) };
                }
            }

            return null;
        }
    }
}