using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PathTracer.SourceGenerators;

[Generator]
public class PlatformServiceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var test = """
                    namespace PathTracer;

                    [AttributeUsage(AttributeTargets.Interface)]
                    public class PlatformServiceAttribute : Attribute
                    {
                    }
                    """;

        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "PlatformServiceAttribute.g.cs",
            test));
    }
}