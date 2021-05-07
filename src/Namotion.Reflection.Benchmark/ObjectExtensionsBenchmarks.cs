using NBench;
using Pro.NBench.xUnit.XunitExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
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
            //// Act
            _object.ValidateNullability();

            //// Complete
            _counter.Increment();
        }

        [NBenchFact]
        [PerfBenchmark(
          NumberOfIterations = 3,
          RunTimeMilliseconds = 1000,
          RunMode = RunMode.Throughput,
          TestMode = TestMode.Test)]
        [CounterThroughputAssertion("Iterations", MustBe.GreaterThan, 100)]
        public void ReadXmlDocs()
        {
            //// Arrange
            XmlDocs.ClearCache();

            //// Act
            var singleSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.Single)).GetXmlDocsSummary();
            var multiSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.Multi)).GetXmlDocsSummary();
            var multiGenericParameterSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.MultiGenericParameter)).GetXmlDocsSummary();
            var nestedGenericParameterSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.NestedGenericParameter)).GetXmlDocsSummary();
            var singleAsyncSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.SingleAsync)).GetXmlDocsSummary();
            var multiAsyncSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.MultiAsync)).GetXmlDocsSummary();
            var multiGenericParameterAsyncSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.MultiGenericParameterAsync)).GetXmlDocsSummary();
            var nestedGenericParameterAsyncSummary = typeof(InheritedGenericClass2).GetMethod(nameof(InheritedGenericClass2.NestedGenericParameterAsync)).GetXmlDocsSummary();

            //// Complete
            _counter.Increment();
        }

        public class BaseGenericClass<T>
        {
            /// <summary>
            /// Single
            /// </summary>
            public T Single(T input)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Multi
            /// </summary>
            public ICollection<T> Multi(ICollection<T> input)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// MultiGenericParameter
            /// </summary>
            public IDictionary<string, string> MultiGenericParameter(IDictionary<string, string> input)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// NestedGenericParameter
            /// </summary>
            public IDictionary<string, IDictionary<string, IDictionary<string, string>>> NestedGenericParameter(IDictionary<string, IDictionary<string, IDictionary<string, string>>> input)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// SingleAsync
            /// </summary>
            public Task<T> SingleAsync(T input)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// MultiAsync
            /// </summary>
            public Task<ICollection<T>> MultiAsync(ICollection<T> input)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// MultiGenericParameterAsync
            /// </summary>
            public Task<IDictionary<string, string>> MultiGenericParameterAsync(IDictionary<string, string> input)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// NestedGenericParameterAsync
            /// </summary>
            public Task<IDictionary<string, IDictionary<string, IDictionary<string, string>>>> NestedGenericParameterAsync(IDictionary<string, IDictionary<string, IDictionary<string, string>>> input)
            {
                throw new NotImplementedException();
            }

        }

        public class InheritedGenericClass2 : BaseGenericClass<string>
        {
        }
    }
}
