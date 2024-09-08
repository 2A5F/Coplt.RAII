using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Coplt.RAII.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RAIIAnalyzer : DiagnosticAnalyzer
{
    #region Diagnostics

    public static readonly DiagnosticDescriptor Rule_DisableCopy = new(
        "RAII0001",
        new LocalizableResourceString(nameof(Resources.RAII0001Title),
            Resources.ResourceManager, typeof(Resources)),
        new LocalizableResourceString(nameof(Resources.RAII0001MessageFormat),
            Resources.ResourceManager, typeof(Resources)),
        "Error",
        DiagnosticSeverity.Error, isEnabledByDefault: true, description:
        new LocalizableResourceString(nameof(Resources.RAII0001Description),
            Resources.ResourceManager, typeof(Resources))
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule_DisableCopy);

    #endregion

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzerID, SyntaxKind.IdentifierName);
    }

    private static bool IsRaii(AttributeData attr)
    {
        if (attr.AttributeClass is not { } c) return false;
        if (c.MetadataName != "RAIIAttribute") return false;
        return c is
        {
            ContainingNamespace :
            {
                MetadataName: "RAII",
                ContainingNamespace:
                {
                    MetadataName: "Coplt",
                    ContainingNamespace.IsGlobalNamespace: true,
                }
            }
        };
    }

    private static bool ShouldReportCopy(SyntaxNodeAnalysisContext ctx, SyntaxNode syntax, out TypeInfo typeInfo)
    {
        typeInfo = ctx.SemanticModel.GetTypeInfo(syntax);
        var raii_attr = typeInfo.Type?.GetAttributes()
            .FirstOrDefault(IsRaii);
        return raii_attr != null;
    }
    
    private static void ReportCopy(SyntaxNodeAnalysisContext ctx, SyntaxNode syntax, TypeInfo typeInfo)
    {
        ctx.ReportDiagnostic(Diagnostic.Create(Rule_DisableCopy, syntax.GetLocation(),
            typeInfo.Type));
    }

    private static void AnalyzerID(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.Node is not IdentifierNameSyntax identifier) return;
        if (!ShouldReportCopy(ctx, identifier, out var typeInfo)) return;
        if (identifier.Parent is EqualsValueClauseSyntax { Value: var v0 } && v0 == identifier)
            ReportCopy(ctx, identifier, typeInfo);
        if (identifier.Parent is ArgumentSyntax { Expression: var v1 } arg && v1 == identifier)
        {
            if (arg.Parent is not { Parent: InvocationExpressionSyntax invocation }) return;
            var index = invocation.ArgumentList.Arguments.IndexOf(arg);
            if (index < 0) return;
            var symbolInfo = ctx.SemanticModel.GetSymbolInfo(invocation);
            if (symbolInfo is not { Symbol: IMethodSymbol symbol }) return;
            var parameter = symbol.Parameters[index];
            if (parameter.RefKind is not RefKind.None) return;
            ReportCopy(ctx, identifier, typeInfo);
        }
    }
}
