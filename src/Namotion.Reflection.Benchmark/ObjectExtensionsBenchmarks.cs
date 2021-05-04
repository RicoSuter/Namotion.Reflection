﻿using NBench;
using Pro.NBench.xUnit.XunitExtensions;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit.Abstractions;

namespace Namotion.Reflection.Benchmark
{
    public class ObjectExtensionsBenchmarks
    {
        private Foo _object = default!;

        public class Foo
        {
            public string Name { get; set; } = default!;

            public List<string> Strings { get; set; } = default!;

            public Dictionary<string, Bar> Bars { get; set; } = default!;

            public List<Foo> Foos { get; set; } = default!;
        }

        public class Bar
        {
            public int Abc { get; set; }

            public string Def { get; set; } = default!;
        }

        private Counter _counter = default!;

        public ObjectExtensionsBenchmarks(ITestOutputHelper output)
        {
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new XunitTraceListener(output));
        }

        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            _counter = context.GetCounter("Iterations");

            _object = new Foo
            {
                Name = "asdf",
                Strings = new List<string> { "a", "b", "c" },
                Foos = new List<Foo> { new Foo(), new Foo
                    {
                        Name = "asdf",
                        Foos = new List<Foo> { new Foo(), new Foo() }
                    }
                },
                Bars = new Dictionary<string, Bar> { { "a", new Bar() }, { "b", new Bar() } }
            };
        }

        [NBenchFact]
        [PerfBenchmark(
            NumberOfIterations = 3,
            RunTimeMilliseconds = 1000,
            RunMode = RunMode.Throughput,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion("Iterations", MustBe.GreaterThan, 100)]
        public void ValidateNullability()
        {
            _object.ValidateNullability();
            _counter.Increment();
        }
    }
}
