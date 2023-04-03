using Arisoul.SourceGenerators.Diagnostics.DataTransferObjects;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace Arisoul.SourceGenerators.DataTransferObjects;

/// <summary>
/// Generates Data Transfer Objects (DTOs) from classes that have properties decorated with the <see cref="DtoPropertyAttribute"/>.
/// </summary>
[Generator]
public class DtoGenerator : IIncrementalGenerator
{
    #region Constants

    internal const string DtoNamespace = "Arisoul.SourceGenerators.DataTransferObjects";
    internal const string FullyQualifiedDtoPropertyMarkerName = $"{DtoNamespace}.{nameof(DtoPropertyAttribute)}";
    internal const string FullyQualifiedDtoClassGenerationMarkerName = $"{DtoNamespace}.{nameof(DtoClassGenerationAttribute)}";
    internal const string FullyQualifiedDtoExtensionsClassGenerationMarkerName = $"{DtoNamespace}.{nameof(DtoExtensionsClassGenerationAttribute)}";
    internal const string DTO = "dto";
    internal const string POCO = "poco";

    #endregion

    #region Public Methods

    /// <inheritdoc/>
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

    #endregion

    #region Private Methods

    private static bool IsSyntaxTargetForGeneration(SyntaxNode syntaxNode)
      => syntaxNode is ClassDeclarationSyntax;

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        // do foreach instead of LINQ due performance

        var classSyntax = (ClassDeclarationSyntax)context.Node;



        // loop through all the class members
        foreach (var member in classSyntax!.Members)
        {
            // must have attributes
            if (member.AttributeLists.Count == 0)
                continue;

            // must be a property
            var propSemanticModel = context.SemanticModel.Compilation.GetSemanticModel(member.SyntaxTree);
            if (propSemanticModel == null || propSemanticModel.GetDeclaredSymbol(member) is not IPropertySymbol propertySymbol)
                continue;

            // loop through attributes and find the required attribute
            foreach (var attribute in propertySymbol.GetAttributes())
            {
                // is this the attribute?
                if (attribute.AttributeClass == null || !FullyQualifiedDtoPropertyMarkerName.Contains(attribute.AttributeClass.ToString()))
                    continue;

                // attribute found in at least one property, return the class
                return classSyntax;
            }
        }

        // none of the properties in class has the required attribute
        return null;
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty) return;

        IEnumerable<ClassDeclarationSyntax> distinctClasses = classes.Distinct();

        List<DtoGeneratorClassInfo> classesToGenerate = GetTypesToGenerate(compilation, distinctClasses, context);

        foreach (var classToGenerate in classesToGenerate)
        {
            var dtoClassFileName = $"{classToGenerate.DtoClassGenerationInfo.Name}.g.cs";
            var dtoClassCode = GetDtoClassCode(classToGenerate);
            var extensionsClassFileName = $"{classToGenerate.ExtensionsClassGenerationInfo.Name}.g.cs";
            var extensionsClassCode = GetDtoExtensionsClassCode(classToGenerate);

            context.AddSource(dtoClassFileName, dtoClassCode);
            context.AddSource(extensionsClassFileName, extensionsClassCode);
        }
    }

    private static List<DtoGeneratorClassInfo> GetTypesToGenerate(Compilation compilation, IEnumerable<ClassDeclarationSyntax> distinctClasses, SourceProductionContext context)
    {
        var dtoGeneratorClassInfoList = new List<DtoGeneratorClassInfo>();

        INamedTypeSymbol? markerAttribute = compilation.GetTypeByMetadataName(FullyQualifiedDtoPropertyMarkerName);

        if (markerAttribute == null)
            return dtoGeneratorClassInfoList;

        // loop through all classes
        foreach (var classDeclaration in distinctClasses)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            DtoGeneratorClassInfo dtoGeneratorClassInfo = null;

            var sourceClassName = classDeclaration.Identifier.ToString();

            // class cannot be abstract
            bool classIsAbstract = false;
            foreach (var modifier in classDeclaration.Modifiers)
                if (modifier.Text.Equals("abstract"))
                {
                    context.ReportDiagnostic(DTODiagnostics.AbstractClassDiagnostic(sourceClassName, classDeclaration));
                    classIsAbstract = true;
                    break;
                }

            if (classIsAbstract)
                continue;

            var propsToGenerate = new List<DtoGeneratorPropertyInfo>();

            // loop through all class members
            foreach (var member in classDeclaration.Members)
            {
                // must have attributes
                if (member.AttributeLists.Count == 0)
                    continue;

                // must be a property
                var propSemanticModel = compilation.GetSemanticModel(member.SyntaxTree);
                if (propSemanticModel == null || propSemanticModel.GetDeclaredSymbol(member) is not IPropertySymbol propertySymbol)
                    continue;

                // the property cannot be readonly
                if (propertySymbol.IsReadOnly)
                {
                    context.ReportDiagnostic(DTODiagnostics.ReadonlyPropertyDiagnostic(propertySymbol, member));
                    continue;
                }

                // loop through attributes and find the required attribute
                foreach (var attribute in propertySymbol.GetAttributes())
                {
                    // is this the attribute?
                    if (attribute.AttributeClass == null || !FullyQualifiedDtoPropertyMarkerName.Contains(attribute.AttributeClass.ToString()))
                        continue;

                    // this is the attribute, go on
                    string? dtoPropertyName = propertySymbol.Name;

                    // Check the constructor arguments
                    if (!attribute.ConstructorArguments.IsEmpty)
                    {
                        ImmutableArray<TypedConstant> args = attribute.ConstructorArguments;

                        // make sure we don't have any errors
                        foreach (TypedConstant arg in args)
                            if (arg.Kind == TypedConstantKind.Error)
                                // have an error, so don't try and do any generation
                                break;

                        // Use the position of the argument to infer which value is set
                        switch (args.Length)
                        {
                            case 1:
                                dtoPropertyName = (string)args[0].Value!;
                                break;
                        }
                    }

                    // now check for named arguments
                    if (!attribute.NamedArguments.IsEmpty)
                        foreach (KeyValuePair<string, TypedConstant> arg in attribute.NamedArguments)
                        {
                            TypedConstant typedConstant = arg.Value;
                            if (typedConstant.Kind == TypedConstantKind.Error)
                                // have an error, so don't try and do any generation
                                break;
                            else
                            {
                                // Use the constructor argument or property name to infer which value is set
                                switch (arg.Key)
                                {
                                    case "Name":
                                        dtoPropertyName = (string)typedConstant.Value!;
                                        break;
                                }
                            }
                        }

                    var propertyType = propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

                    var propInfo = new DtoGeneratorPropertyInfo(propertySymbol.Name, dtoPropertyName!, propertyType);

                    propsToGenerate.Add(propInfo);
                    break;
                }
            }

            if (propsToGenerate.Count == 0)
                continue;

            var sourceNamespace = classDeclaration.GetNamespace();

            DtoClassGenerationInfo dtoClassGenerationInfo = new()
            {
                Name = $"{sourceClassName}{DTO.Capitalize()}",
                Namespace = sourceNamespace
            };

            ExtensionsClassGenerationInfo extensionsClassGenerationInfo = new()
            {
                Name = $"{sourceClassName}Extensions",
                Namespace = sourceNamespace
            };

            dtoGeneratorClassInfo = new DtoGeneratorClassInfo(
                sourceClassName, 
                sourceNamespace, 
                dtoClassGenerationInfo, 
                extensionsClassGenerationInfo, 
                propsToGenerate);

            // if the class has a DtoClassGeneration attribute defined, get corresponding information

            // check if class has attributes
            if (classDeclaration.AttributeLists.Count > 0)
            {
                string[] classExpectedAttributes = new[]
                {
                    FullyQualifiedDtoClassGenerationMarkerName,
                    FullyQualifiedDtoExtensionsClassGenerationMarkerName
                };

                // get class symbol 
                var classSemanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                if (classSemanticModel == null || classSemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
                    continue;

                foreach (var attribute in classSymbol.GetAttributes())
                {
                    // filter by expected attributes
                    foreach (var expectedAttribute in classExpectedAttributes)
                    {
                        if (attribute.AttributeClass == null || !expectedAttribute.Contains(attribute.AttributeClass.ToString()))
                            continue;

                        // now check for named arguments (these attributes will not have constructor arguments, to facilitate generation)
                        if (!attribute.NamedArguments.IsEmpty)
                        {
                            Dictionary<string, object> arguments = new Dictionary<string, object>();

                            // grant there's no error
                            foreach (KeyValuePair<string, TypedConstant> arg in attribute.NamedArguments)
                            {
                                TypedConstant typedConstant = arg.Value;
                                if (typedConstant.Kind == TypedConstantKind.Error)
                                    // have an error, so don't try and do any generation
                                    break;
                                else
                                    arguments.Add(arg.Key, typedConstant.Value!);
                            }

                            // if there's no error, arguments should exist
                            if (arguments.Count > 0)
                                foreach (var arg in arguments)
                                {
                                    // dto class properties
                                    if (FullyQualifiedDtoClassGenerationMarkerName.Contains(attribute.AttributeClass.Name))
                                    {
                                        if (arg.Key.Equals(nameof(BaseDtoClassGenerationAttribute.Name)))
                                            dtoGeneratorClassInfo.DtoClassGenerationInfo.Name = arg.Value.ToString();
                                        else if (arg.Key.Equals(nameof(BaseDtoClassGenerationAttribute.Namespace)))
                                            dtoGeneratorClassInfo.DtoClassGenerationInfo.Namespace = arg.Value.ToString();
                                    }
                                    // extensions class properties
                                    else if (FullyQualifiedDtoExtensionsClassGenerationMarkerName.Contains(attribute.AttributeClass.Name))
                                    {
                                        if (arg.Key.Equals(nameof(BaseDtoClassGenerationAttribute.Name)))
                                            dtoGeneratorClassInfo.ExtensionsClassGenerationInfo.Name = arg.Value.ToString();
                                        else if (arg.Key.Equals(nameof(BaseDtoClassGenerationAttribute.Namespace)))
                                            dtoGeneratorClassInfo.ExtensionsClassGenerationInfo.Namespace = arg.Value.ToString();
                                    }
                                }

                            break;
                        }
                    }
                }
            }

            dtoGeneratorClassInfoList.Add(dtoGeneratorClassInfo);
        }

        return dtoGeneratorClassInfoList;
    }

    private static SourceText GetDtoClassCode(DtoGeneratorClassInfo classInfo)
    {
        var sb = new StringBuilder();
        sb.Append(@$"{ClassWriter.WriteClassHeader(true)}

{ClassWriter.WriteUsing("System")}

namespace {classInfo.DtoClassGenerationInfo.Namespace}
{{
    public class {classInfo.DtoClassGenerationInfo.Name}
    {{");

        foreach (var prop in classInfo.Properties)
            sb.Append(@$"
        {PropertyWriter.WritePublicPropertySimple(prop.Type, prop.DtoName)}");

        sb.Append(@"
    }
}");

        return SourceText.From(sb.ToString(), Encoding.UTF8);
    }

    private static SourceText GetDtoExtensionsClassCode(DtoGeneratorClassInfo classInfo)
    {
        string pocoCapitalized = POCO.Capitalize()!;
        string dtoCapitalized = DTO.Capitalize()!;
        string dtoClassName = classInfo.DtoClassGenerationInfo.Name;
        string extensionsClassName = classInfo.ExtensionsClassGenerationInfo.Name;

        var sb = new StringBuilder();

        sb.Append($@"{ClassWriter.WriteClassHeader(true)}

{ClassWriter.WriteUsing("System")}");
        if(!string.Equals(classInfo.DtoClassGenerationInfo.Namespace, classInfo.ExtensionsClassGenerationInfo.Namespace, StringComparison.Ordinal))
        {
            sb.Append($@"
{ClassWriter.WriteUsing(classInfo.DtoClassGenerationInfo.Namespace)}");
        }

        if (!string.Equals(classInfo.SourceNamespace, classInfo.ExtensionsClassGenerationInfo.Namespace, StringComparison.Ordinal))
        {
            sb.Append($@"
{ClassWriter.WriteUsing(classInfo.SourceNamespace)}");
        }

        sb.Append($@"

namespace {classInfo.ExtensionsClassGenerationInfo.Namespace}
{{
    public static class {extensionsClassName}
    {{
        public static {dtoClassName} To{dtoCapitalized}(this {classInfo.SourceClassName} {POCO})
        {{
            {dtoClassName} {DTO} = new {dtoClassName}();
");

        foreach (var prop in classInfo.Properties)
            sb.Append(@$"
            {PropertyWriter.WritePropertyAttribution($"{DTO}.{prop.DtoName}", $"{POCO}.{prop.PocoName}")}");

        sb.Append($@"

            return {DTO};
        }}

        public static void From{dtoCapitalized}(this {classInfo.SourceClassName} {POCO}, {dtoClassName} {DTO})
        {{");

        foreach (var prop in classInfo.Properties)
            sb.Append(@$"
            {POCO}.{prop.PocoName} = {DTO}.{prop.DtoName};");

        sb.Append($@"
        }}

        public static {classInfo.SourceClassName} To{pocoCapitalized}(this {dtoClassName} {DTO})
        {{
            {classInfo.SourceClassName} {POCO} = new {classInfo.SourceClassName}();
");

        foreach (var prop in classInfo.Properties)
            sb.Append(@$"
            {POCO}.{prop.PocoName} = {DTO}.{prop.DtoName};");

        sb.Append(@$"

            return {POCO};
        }}

        public static void From{pocoCapitalized}(this {dtoClassName} {DTO}, {classInfo.SourceClassName} {POCO})
        {{");

        foreach (var prop in classInfo.Properties)
            sb.Append(@$"
            {DTO}.{prop.DtoName} = {POCO}.{prop.PocoName};");

        sb.Append(@$"
        }}
    }}
}}");

        return SourceText.From(sb.ToString(), Encoding.UTF8);
    }

    #endregion
}
