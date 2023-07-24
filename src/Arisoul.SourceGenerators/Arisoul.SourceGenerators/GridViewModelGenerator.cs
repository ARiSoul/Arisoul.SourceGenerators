// Create an IIncrementalGenerator that finds all classes with GridViweModel<T> declarations in body
// grab the generic type T and generate a new class with name of the type of T followed by GVM
// The new generated class will have all the properties of T with type string

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Arisoul.SourceGenerators.DataTransferObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Arisoul.SourceGenerators.GridViewModels;

[Generator]
public class GridViewModelGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // do a simple filter for classes
        IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)!;

        // combine the selected classes with the compilation
        IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndClasses
            = context.CompilationProvider.Combine(classDeclarations.Collect());

        // generate the source using the compilation and classes
        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode syntaxNode)
      => syntaxNode is ClassDeclarationSyntax;

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        // do foreach instead of LINQ due performance

        var classSyntax = (ClassDeclarationSyntax)context.Node;

        // loop through all the class members
        foreach (var member in classSyntax!.Members)
        {
            // must be of type GridViewModel<T>
            if (member is not PropertyDeclarationSyntax propertyDeclarationSyntax
                || propertyDeclarationSyntax.Type is not GenericNameSyntax genericNameSyntax
                || genericNameSyntax.Identifier.Text != "GridViewModel")
                continue;

            // it has at least one gridviewmodel property, return the class
            return classSyntax;
        }

        // none of the properties in class has the required attribute
        return null;
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty) return;

        IEnumerable<ClassDeclarationSyntax> distinctClasses = classes.Distinct();

        // loop through all the classes
        foreach (var classDeclarationSyntax in distinctClasses)
        {
            var parentClassName = classDeclarationSyntax.Identifier.Text;
            var parentClassNamespace = classDeclarationSyntax.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString();

            // get all the GridViewModel properties
            var gridViewModelProperties = classDeclarationSyntax.Members
                .OfType<PropertyDeclarationSyntax>()
                .Where(p => p.Type is GenericNameSyntax genericNameSyntax
                && genericNameSyntax.Identifier.Text == "GridViewModel");

            // loop through all the GridViewModel properties
            foreach (var gvm in gridViewModelProperties)
            {
                var sb = new StringBuilder();

                sb.Append($@"{ClassWriter.WriteClassHeader(((CSharpCompilation)compilation).LanguageVersion >= LanguageVersion.CSharp8)}

{ClassWriter.WriteUsing("System")}");

 sb.Append($@"

namespace {parentClassNamespace}
{{
    public static partial class {parentClassName}
    {{");

                var genericNameSyntax = (GenericNameSyntax)gvm.Type;
                
                // get the type of the generic argument
                var typeArgument = genericNameSyntax.TypeArgumentList.Arguments[0];

                // get the name of gvm
                var gvmName = gvm.Identifier.Text;

                // get the syntax tree of the typeArgument
                var typeArgumentSyntaxTree = compilation.SyntaxTrees
                    .FirstOrDefault(s => s.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
                                       .Any(c => c.Identifier.Text == typeArgument.ToString()));

                // get the class declaration syntax of the typeArgument
                var typeArgumentClassDeclarationSyntax = typeArgumentSyntaxTree?.GetRoot().DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault(c => c.Identifier.Text == typeArgument.ToString());

                if (typeArgumentClassDeclarationSyntax != null)
                {
                    sb.Append($@"
        private readonly Class{typeArgument}Columns _cols{typeArgument};

        public void Init{typeArgument}Columns()
        {{
            _cols{typeArgument} = new Class{typeArgument}Columns({gvmName});
        }}

        public class Class{typeArgument}Columns
        {{

            private readonly GridViewModel<{typeArgument}> _gvm;

            public Class{typeArgument}Columns(GridViewModel<{typeArgument}> gvm)
            {{
                _gvm = gvm;
            }}
");
                    foreach (var argumentMember in typeArgumentClassDeclarationSyntax.Members)
                    {
                        // must be a property
                        var propSemanticModel = compilation.GetSemanticModel(argumentMember.SyntaxTree);
                        if (propSemanticModel == null || propSemanticModel.GetDeclaredSymbol(argumentMember) is not IPropertySymbol propertySymbol)
                            continue;

                        sb.Append($@"
            public string {propertySymbol.Name} => _gvm.GetColCaption(nameof(_gvm.Class.{propertySymbol.Name}));");
                        
                    }
                    sb.Append($@"
        }}");
                }

                sb.Append($@"
    }}
}}");

                var gvmFileName = $"{parentClassName}.{gvmName}.g.cs";
                context.AddSource(gvmFileName, SourceText.From(sb.ToString(), Encoding.UTF8));

            }

        }
    }
}
