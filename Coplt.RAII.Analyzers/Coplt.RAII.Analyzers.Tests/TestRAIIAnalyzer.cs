using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Coplt.RAII.Analyzers.Tests;

using Verifier = CSharpAnalyzerTest<RAIIAnalyzer, DefaultVerifier>;

public class TestRAIIAnalyzer
{
    private const string BasicCode = @"
using Coplt.RAII;

[RAII]
public struct Foo
{
    public Foo Copy() => default;
}
";

    [Fact]
    public async Task Test1()
    {
        const string code = BasicCode + @"
public static class Program
{
    public static void Main()
    {
        var a = new Foo();
        var b = a.Copy();
    }
}
";
        await new Verifier
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestState =
            {
                Sources = { code },
                AdditionalReferences = { typeof(RAIIAttribute).Assembly.Location },
            }
        }.RunAsync();
    }

    [Fact]
    public async Task Test2()
    {
        const string code = BasicCode + @"
public static class Program
{
    public static void Main()
    {
        var a = new Foo();
        var b = a;
    }
}
";
        await new Verifier
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestState =
            {
                Sources = { code },
                AdditionalReferences = { typeof(RAIIAttribute).Assembly.Location },
            },
            ExpectedDiagnostics =
            {
                new DiagnosticResult(RAIIAnalyzer.Rule_DisableCopy)
                    .WithSpan(15, 17, 15, 18)
                    .WithArguments("Foo"),
            }
        }.RunAsync();
    }

    [Fact]
    public async Task Test3()
    {
        const string code = BasicCode + @"
public static class Program
{
    public static void Main()
    {
        var a = new Foo();
        Some(a);
    }
    public static void Some(Foo foo) { }
}
";
        await new Verifier
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestState =
            {
                Sources = { code },
                AdditionalReferences = { typeof(RAIIAttribute).Assembly.Location },
            },
            ExpectedDiagnostics =
            {
                new DiagnosticResult(RAIIAnalyzer.Rule_DisableCopy)
                    .WithSpan(15, 14, 15, 15)
                    .WithArguments("Foo"),
            }
        }.RunAsync();
    }

    [Fact]
    public async Task Test4()
    {
        const string code = BasicCode + @"
public static class Program
{
    public static void Main()
    {
        var a = new Foo();
        Some(a);
    }
    public static void Some(in Foo foo) { }
}
";
        await new Verifier
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestState =
            {
                Sources = { code },
                AdditionalReferences = { typeof(RAIIAttribute).Assembly.Location },
            }
        }.RunAsync();
    }
}
