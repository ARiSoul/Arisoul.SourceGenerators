using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Arisoul.SourceGenerators.Tests;

public static class TestHelper
{
    public static Task Verify<T>(string source)
        where T : class, IIncrementalGenerator, new()
    {
        // Parse the provided string into a C# syntax tree
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = AppDomain.CurrentDomain.GetAssemblies()
                                .Where(assembly => !assembly.IsDynamic)
                                .Select(assembly => MetadataReference
                                                    .CreateFromFile(assembly.Location))
                                .Cast<MetadataReference>();

        // Create a Roslyn compilation for the syntax tree.
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Create an instance of our EnumGenerator incremental source generator
        var generator = new T();

        // The GeneratorDriver is used to run our generator against a compilation
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the source generator!
        driver = driver.RunGenerators(compilation);

        // Use verify to snapshot test the source generator output!
        return Verifier
            .Verify(driver)
            .UseDirectory("Snapshots");
    }
}
