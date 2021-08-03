using System;
using System.Threading;

#if NETSTANDARD1_0

namespace FrameworkTraits
{
    internal static class Volatile
    {
        public static T Read<T>(ref T location) where T : class
        {
            // 
            // The VM will replace this with a more efficient implementation.
            //
            var value = location;
            //Thread.MemoryBarrier();
            return value;
        }

        public static void Write<T>(ref T location, T value) where T : class
        {
            // 
            // The VM will replace this with a more efficient implementation.
            //
            //Thread.MemoryBarrier();
            location = value;
        }
    }

    internal static class PlatformHelper
    {
        public static int ProcessorCount
        {
            get { return Environment.ProcessorCount; }
        }
    }

    internal static class Monitor
    {
        public static void Enter(Object obj, ref bool lockTaken)
        {
            if (lockTaken)
                throw new ArgumentException("Argument must be initialized to false", "lockTaken");

            System.Threading.Monitor.Enter(obj);
            lockTaken = true;
        }
    }
}

#endif