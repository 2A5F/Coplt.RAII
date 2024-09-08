# Coplt.RAII

[![.NET](https://github.com/2A5F/Coplt.RAII/actions/workflows/dotnet.yml/badge.svg)](https://github.com/2A5F/Coplt.RAII/actions/workflows/dotnet.yml)
[![Nuget](https://img.shields.io/nuget/v/Coplt.RAII)](https://www.nuget.org/packages/Coplt.RAII/)

Disable copying of structures

Currently, dispose check is not supported,  
it is recommended to use jetbrains [[MustDisposeResource](https://www.jetbrains.com/help/rider/Reference__Code_Annotation_Attributes.html#MustDisposeResourceAttribute)]

### Example

```csharp
using Coplt.RAII;

[RAII]
public struct Foo
{
    // Any method, usually using Copy and Move
    public Foo Copy() => default;
    public Foo Move() => default;
}

var a = new Foo();

var b = a.Copy(); // ok
var b = a.Move(); // ok

var b = a; // err
// Error RAII0001: Copying RAII struct Foo is not allowed, try calling a method that returns Foo to get a copy
```

### Todo
- [x] EqualsValueClauseSyntax
- [x] ArgumentSyntax
- [ ] other expr
