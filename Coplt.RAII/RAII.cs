namespace Coplt.RAII;

/// <summary>
/// Structures marked with raii will disable copying
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
public sealed class RAIIAttribute : Attribute;
