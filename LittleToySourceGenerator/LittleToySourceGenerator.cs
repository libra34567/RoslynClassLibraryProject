using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace LittleToySourceGenerator;

[Generator]
public class Generator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        System.Console.WriteLine(System.DateTime.Now.ToString());

        var sourceBuilder = new StringBuilder(
        @"
        using System;
        namespace ToyMe
        {
            public static class ToyMeGenerated
            {
                public static string GetTestText() 
                {
                    return ""This is from source generator ");

        sourceBuilder.Append(System.DateTime.Now.ToString());

        sourceBuilder.Append(
                @""";
                }
    }
}
");

        context.AddSource("exampleSourceGenerator", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
    }

    public void Initialize(GeneratorInitializationContext context) { }
}
