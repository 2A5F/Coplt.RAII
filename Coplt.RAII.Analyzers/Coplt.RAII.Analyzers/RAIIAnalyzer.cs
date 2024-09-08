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

        context.RegisterOperationAction(AnalyzerVariableDeclaration, OperationKind.VariableDeclaration);
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

    private void AnalyzerID(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.Node is not IdentifierNameSyntax identifier) return;
        if (identifier.Parent is not EqualsValueClauseSyntax) return;
        var typeInfo = ctx.SemanticModel.GetTypeInfo(ctx.Node);
        var raii_attr = typeInfo.Type?.GetAttributes()
            .FirstOrDefault(IsRaii);
        if (raii_attr is null) return;
        ctx.ReportDiagnostic(Diagnostic.Create(Rule_DisableCopy, identifier.GetLocation(),
            typeInfo.Type));
    }

    private void AnalyzerVariableDeclaration(OperationAnalysisContext obj)
    {
        if (obj.Operation is not IVariableDeclarationOperation variable) return;
        // Console.WriteLine(variable);
    }
}
