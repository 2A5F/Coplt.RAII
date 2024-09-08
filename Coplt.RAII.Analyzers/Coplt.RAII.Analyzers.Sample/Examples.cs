using System;

namespace Coplt.RAII.Analyzers.Sample;

[RAII]
public struct Foo
{
    public Foo Copy() => default;
}

public static class Program
{
    public static void Main()
    {
        // var a = new Foo();
        // var b = a; // error
        //var c = a.Copy();
        // Console.WriteLine(b);
    }
}
