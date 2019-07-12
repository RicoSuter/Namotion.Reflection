using System;

namespace Namotion.Reflection.Tests.FullAssembly
{
    public class FullAssemblyTestAction
    {
        public Tuple<string, string?, int, int?, Action, Action?> Method(string p1, string? p2, int p3, int? p4, Action p5, Action? p6)
        {
            return null;
        }

        public Tuple<string> Method2(string p1)
        {
            return null;
        }

        public Tuple<string?>? Method3(string? p1)
        {
            return null;
        }

        public string Property1 { get; set; }

        public string? Property2 { get; set; }
    }
}
