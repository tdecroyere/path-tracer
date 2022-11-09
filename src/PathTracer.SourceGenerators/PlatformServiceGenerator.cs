using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace PathTracer.SourceGenerators;

record PlatformServiceToGenerate
{
    public required string? Namespace { get; init; }
    public required string InterfaceName { get; init; }

    public IList<IMethodSymbol> MethodList { get; } = new List<IMethodSymbol>();

    public string ImplementationClassName
    {
        get
        {
            return InterfaceName.Substring(1);
        }
    }

    public string InteropClassName
    {
        get
        {
            return $"{ImplementationClassName}Interop";
        }
    }
}

[Generator]
public class PlatformServiceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        GenerateAttribute(context);

        var interfaceDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => FilterInterfaceNodes(syntaxNode),
                transform: static (context, _) => FilterPlatformServices(context))
            .Where(static item => item is not null);

        var compilationAndIntefaces = context.CompilationProvider.Combine(interfaceDeclarations.Collect());

        context.RegisterImplementationSourceOutput(compilationAndIntefaces,
            static (context, source) => Generate(source.Left, source.Right, context));
    }

    private static void GenerateAttribute(IncrementalGeneratorInitializationContext context)
    {
        var attributeCode = """
                            namespace PathTracer;

                            [AttributeUsage(AttributeTargets.Interface)]
                            public class PlatformServiceAttribute : Attribute
                            {
                            }
                            """;

        context.RegisterPostInitializationOutput(context => context.AddSource(
            "PlatformServiceAttribute.g.cs",
            attributeCode));
    }

    private static bool FilterInterfaceNodes(SyntaxNode syntaxNode)
    {
        return syntaxNode is InterfaceDeclarationSyntax interfaceNode && interfaceNode.AttributeLists.Count > 0;
    }

    private static InterfaceDeclarationSyntax? FilterPlatformServices(GeneratorSyntaxContext context)
    {
        var interfaceDeclarationSyntax = (InterfaceDeclarationSyntax)context.Node;

        foreach (var attributeListSyntax in interfaceDeclarationSyntax.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                {
                    continue;
                }

                if (attributeSymbol.ContainingType.ToDisplayString() == "PathTracer.PlatformServiceAttribute")
                {
                    return interfaceDeclarationSyntax;
                }
            }
        }

        return null;
    }

    private static void Generate(Compilation compilation, ImmutableArray<InterfaceDeclarationSyntax?> interfaces, SourceProductionContext context)
    {
        if (interfaces.IsDefaultOrEmpty)
        {
            return;
        }

        var distinctInterfaces = interfaces.Where(item => item is not null)
                                           .Select(item => item!)
                                           .Distinct();

        var interfacesToGenerate = GetTypesToGenerate(compilation, distinctInterfaces, context.CancellationToken);

        if (interfacesToGenerate.Count > 0)
        {
            foreach (var platformService in interfacesToGenerate)
            {
                var sourceCode = new StringBuilder();
                GenerateImplementationClass(sourceCode, platformService);
                context.AddSource($"{platformService.ImplementationClassName}.g.cs", SourceText.From(sourceCode.ToString(), Encoding.UTF8));

                // TODO: Generators cannot be chained for now.
                // Check issue for follow up: https://github.com/dotnet/roslyn/issues/57239
                //sourceCode.Clear();
                //GenerateInteropClass(sourceCode, platformService);
                //context.AddSource($"{platformService.InteropClassName}.g.cs", SourceText.From(sourceCode.ToString(), Encoding.UTF8));
            }
        }
    }

    private static void GenerateImplementationClass(StringBuilder sourceCode, PlatformServiceToGenerate platformService)
    {
        if (platformService.Namespace is not null)
        {
            sourceCode.AppendLine($"namespace {platformService.Namespace};");
            sourceCode.AppendLine();
        }

        sourceCode.AppendLine($"internal partial class {platformService.ImplementationClassName} : {platformService.InterfaceName}");
        sourceCode.AppendLine("{");

        foreach (var method in platformService.MethodList)
        {
            sourceCode.AppendLine($"public {method.ReturnType} {method.Name}({string.Join(',', method.Parameters.Select(item => item.Type.Name + " " + item.Name))})");
            sourceCode.AppendLine("{");
            
            sourceCode.AppendLine($"return {platformService.InteropClassName}.{method.Name}({string.Join(',', method.Parameters.Select(item => item.Name))});");
            
            sourceCode.AppendLine("}");
        }

        sourceCode.AppendLine("}");
    }

    private static void GenerateInteropClass(StringBuilder sourceCode, PlatformServiceToGenerate platformService)
    {
        sourceCode.AppendLine("using System.Runtime.InteropServices;");
        sourceCode.AppendLine();

        if (platformService.Namespace is not null)
        {
            sourceCode.AppendLine($"namespace {platformService.Namespace};");
            sourceCode.AppendLine();
        }

        sourceCode.AppendLine($"internal static partial class {platformService.InteropClassName}");
        sourceCode.AppendLine("{");

        foreach (var method in platformService.MethodList)
        {
            sourceCode.AppendLine("[LibraryImport(\"PathTracer.Platform.Native\", StringMarshalling = StringMarshalling.Utf8)]");
            sourceCode.AppendLine($"internal static partial {method.ReturnType} {method.Name}({string.Join(',', method.Parameters.Select(item => item.Type.Name + " " + item.Name))});");
            sourceCode.AppendLine();
        }

        sourceCode.AppendLine("}");
    }

    static List<PlatformServiceToGenerate> GetTypesToGenerate(Compilation compilation, IEnumerable<InterfaceDeclarationSyntax> interfaces, CancellationToken cancellationToken)
    {
        var result = new List<PlatformServiceToGenerate>();
        
        var platformServiceAttribute = compilation.GetTypeByMetadataName("PathTracer.PlatformServiceAttribute");

        if (platformServiceAttribute == null)
        {
            return result;
        }

        foreach (var interfaceDeclarationSyntax in interfaces)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var semanticModel = compilation.GetSemanticModel(interfaceDeclarationSyntax.SyntaxTree);

            if (semanticModel.GetDeclaredSymbol(interfaceDeclarationSyntax, cancellationToken) is not INamedTypeSymbol interfaceSymbol)
            {
                continue;
            }
            
            var interfaceName = interfaceSymbol.Name;
            var namespaceName = interfaceSymbol.ContainingNamespace;

            if (interfaceName is null)
            {
                continue;
            }
            
            var platformService = new PlatformServiceToGenerate
            {
                InterfaceName = interfaceName,
                Namespace = namespaceName?.ToString()
            };

            var members = interfaceSymbol.GetMembers();

            foreach (var member in members)
            {
                if (member is IMethodSymbol method)
                {
                    platformService.MethodList.Add(method);
                }
            }

            result.Add(platformService);
        }

        return result;
    }
}