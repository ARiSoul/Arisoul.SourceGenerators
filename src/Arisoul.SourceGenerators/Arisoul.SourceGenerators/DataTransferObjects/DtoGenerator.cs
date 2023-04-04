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
    internal const string FullyQualifiedTargetDtoPropertyMarkerName = $"{DtoNamespace}.TargetDtoPropertyAttribute";
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
                if (attribute.AttributeClass == null 
                    || (!FullyQualifiedDtoPropertyMarkerName.Contains(attribute.AttributeClass.Name)
                    && !FullyQualifiedTargetDtoPropertyMarkerName.Contains(attribute.AttributeClass.Name)))
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

        List<DtoGeneratorClassInfo> classInfoList = GetTypesToGenerate(compilation, distinctClasses, context);

        foreach (var classInfo in classInfoList)
        {
            var dtoClassFileName = $"{classInfo.DtoClassGenerationInfo.Name}.g.cs";
            var dtoClassCode = GetDtoClassCode(classInfo);

            context.AddSource(dtoClassFileName, dtoClassCode);

            if (classInfo.ExtensionsClassGenerationInfo.GenerationBehavior != GenerationBehavior.NoGeneration)
            {
                var extensionsClassFileName = $"{classInfo.ExtensionsClassGenerationInfo.Name}.g.cs";
                var extensionsClassCode = GetDtoExtensionsClassCode(classInfo);

                context.AddSource(extensionsClassFileName, extensionsClassCode);
            }
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
            var sourceClassName = classDeclaration.Identifier.ToString();

            // class cannot be abstract
            bool classIsAbstract = CheckIfClassIfAbstract(context, classDeclaration, sourceClassName);
            if (classIsAbstract)
                continue;

            var propsToGenerate = new List<DtoGeneratorPropertyInfo>();

            PopulatePropsToGenerate(compilation, context, classDeclaration, propsToGenerate);

            if (propsToGenerate.Count == 0)
                continue;

            var sourceNamespace = classDeclaration.GetNamespace();
            DtoGeneratorClassInfo dtoGeneratorClassInfo = InitDtoGeneratorClassInfo(sourceClassName, propsToGenerate, sourceNamespace);

            // if the class has a DtoClassGeneration attribute defined, get corresponding information
            if (!SetGeneratorClassInfoClassesGenerationInfo(compilation, classDeclaration, dtoGeneratorClassInfo))
                continue;

            dtoGeneratorClassInfoList.Add(dtoGeneratorClassInfo);
        }

        return dtoGeneratorClassInfoList;
    }

    private static bool SetGeneratorClassInfoClassesGenerationInfo(Compilation compilation, ClassDeclarationSyntax classDeclaration, DtoGeneratorClassInfo dtoGeneratorClassInfo)
    {
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
                return false;

            foreach (var attribute in classSymbol.GetAttributes())
                SetClassGenerationInfoPropertiesFromClassAttribute(dtoGeneratorClassInfo, classExpectedAttributes, attribute);
        }

        return true;
    }

    private static bool CheckIfClassIfAbstract(SourceProductionContext context, ClassDeclarationSyntax classDeclaration, string sourceClassName)
    {
        bool classIsAbstract = false;

        foreach (var modifier in classDeclaration.Modifiers)
            if (modifier.Text.Equals("abstract"))
            {
                context.ReportDiagnostic(DTODiagnostics.AbstractClassDiagnostic(sourceClassName, classDeclaration));
                classIsAbstract = true;
                break;
            }

        return classIsAbstract;
    }

    private static void PopulatePropsToGenerate(Compilation compilation, SourceProductionContext context, ClassDeclarationSyntax classDeclaration, List<DtoGeneratorPropertyInfo> propsToGenerate)
    {
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

            SetPropertyNameAndPopulatePropsToGenerate(propsToGenerate, propertySymbol);
        }
    }

    private static void SetClassGenerationInfoPropertiesFromClassAttribute(DtoGeneratorClassInfo dtoGeneratorClassInfo, string[] classExpectedAttributes, AttributeData attribute)
    {
        // filter by expected attributes
        foreach (var expectedAttribute in classExpectedAttributes)
        {
            if (attribute.AttributeClass == null || !expectedAttribute.Contains(attribute.AttributeClass.ToString()))
                continue;

            // now check for named arguments (these attributes will not have constructor arguments, to facilitate generation)
            if (!attribute.NamedArguments.IsEmpty)
            {
                Dictionary<string, object> arguments = GetClassAttributeNamedArgumentsWithNoError(attribute);

                // if there's no error, arguments should exist
                if (arguments.Count > 0)
                    SetClassGenerationInfoProperties(dtoGeneratorClassInfo, attribute, arguments);

                break;
            }
        }
    }

    private static Dictionary<string, object> GetClassAttributeNamedArgumentsWithNoError(AttributeData attribute)
    {
        Dictionary<string, object> arguments = new();

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

        return arguments;
    }

    private static void SetClassGenerationInfoProperties(DtoGeneratorClassInfo dtoGeneratorClassInfo, AttributeData attribute, Dictionary<string, object> arguments)
    {
        foreach (var arg in arguments)
            // dto class properties
            if (FullyQualifiedDtoClassGenerationMarkerName.Contains(attribute.AttributeClass.Name))
                SetDtoClassInfoProperties(dtoGeneratorClassInfo, arg);
            // extensions class properties
            else if (FullyQualifiedDtoExtensionsClassGenerationMarkerName.Contains(attribute.AttributeClass.Name))
                SetExtensionsClassInfoProperties(dtoGeneratorClassInfo, arg);
    }

    private static void SetDtoClassInfoProperties(DtoGeneratorClassInfo dtoGeneratorClassInfo, KeyValuePair<string, object> arg)
        => SetClassGenerationInfoProperties(dtoGeneratorClassInfo.DtoClassGenerationInfo, arg);

    private static void SetExtensionsClassInfoProperties(DtoGeneratorClassInfo dtoGeneratorClassInfo, KeyValuePair<string, object> arg)
    {
        if (arg.Key.Equals(nameof(DtoExtensionsClassGenerationAttribute.GenerationBehavior)))
            dtoGeneratorClassInfo.ExtensionsClassGenerationInfo.GenerationBehavior = (GenerationBehavior)arg.Value;
        else
            SetClassGenerationInfoProperties(dtoGeneratorClassInfo.ExtensionsClassGenerationInfo, arg);
    }

    private static void SetClassGenerationInfoProperties(BaseClassGenerationInfo classGenerationInfo, KeyValuePair<string, object> arg)
    {
        switch (arg.Key)
        {
            case nameof(BaseClassGenerationInfo.Name):
                classGenerationInfo.Name = arg.Value.ToString();
                break;

            case nameof(BaseClassGenerationInfo.Namespace):
                classGenerationInfo.Namespace = arg.Value.ToString();
                break;

            default:
                break;
        }
    }

    private static DtoGeneratorClassInfo InitDtoGeneratorClassInfo(string sourceClassName, List<DtoGeneratorPropertyInfo> propsToGenerate, string sourceNamespace)
    {
        DtoGeneratorClassInfo dtoGeneratorClassInfo;

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

        return dtoGeneratorClassInfo;
    }

    private static void SetPropertyNameAndPopulatePropsToGenerate(List<DtoGeneratorPropertyInfo> propsToGenerate, IPropertySymbol propertySymbol)
    {
        // loop through attributes and find the required attribute
        foreach (var attribute in propertySymbol.GetAttributes())
        {
            // is this the attribute?
            if (attribute.AttributeClass == null 
                || (!FullyQualifiedDtoPropertyMarkerName.Contains(attribute.AttributeClass.Name)
                && !FullyQualifiedTargetDtoPropertyMarkerName.Contains(attribute.AttributeClass.Name)))
                continue;

            // this is the attribute, go on
            string? dtoPropertyName = GetPropertyNameFromDtoAttribute(attribute);
            if (string.IsNullOrWhiteSpace(dtoPropertyName))
                dtoPropertyName = propertySymbol.Name;

            var sourceType = propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            var targetType = sourceType;

            // in case is the TargetDtoProperty, the target type must be the one received in generic
            // TODO: TargetDtoPropertyAttribute contains DtoPropertyAttribute. Rethink this...
            // TODO: detect if a property is not a primitive (namespace does not starts with System), to allow instantiation. Consider Collections and Enums.
            // TODO: Allowing this, the Extensions class complicates a lot, having to cast to the correct type. Think about this too
            if (FullyQualifiedTargetDtoPropertyMarkerName.Contains(attribute.AttributeClass.Name))
            {
                var target = attribute.AttributeClass.TypeArguments[0];
                sourceType = target.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            }

            var propInfo = new DtoGeneratorPropertyInfo(propertySymbol.Name, dtoPropertyName!, sourceType, targetType);

            propsToGenerate.Add(propInfo);
            break;
        }
    }

    private static string GetPropertyNameFromDtoAttribute(AttributeData attribute)
    {
        string dtoPropertyName;

        // Check the constructor arguments
        dtoPropertyName = GetPropertyNameFromConstructorArguments(attribute);

        if (string.IsNullOrEmpty(dtoPropertyName)) // now check for named arguments if it was not possible to get before
            dtoPropertyName = GetPropertyNameFromNamedArguments(attribute);

        return dtoPropertyName;
    }

    private static string GetPropertyNameFromNamedArguments(AttributeData attribute)
    {
        if (!attribute.NamedArguments.IsEmpty)
            foreach (KeyValuePair<string, TypedConstant> arg in attribute.NamedArguments)
            {
                TypedConstant typedConstant = arg.Value;
                if (typedConstant.Kind == TypedConstantKind.Error)
                    // have an error, so don't try and do any generation
                    break;
                else
                    // Use the constructor argument or property name to infer which value is set
                    switch (arg.Key)
                    {
                        case "Name":
                            return (string)typedConstant.Value!;
                    }
            }

        return string.Empty;
    }

    private static string GetPropertyNameFromConstructorArguments(AttributeData attribute)
    {
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
                    return (string)args[0].Value!;
            }
        }

        return string.Empty;
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
        {PropertyWriter.WritePublicPropertySimple(prop.TargetType, prop.TargetName)}");

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
        if (!string.Equals(classInfo.DtoClassGenerationInfo.Namespace, classInfo.ExtensionsClassGenerationInfo.Namespace, StringComparison.Ordinal))
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
    {{");

        if (!classInfo.ExtensionsClassGenerationInfo.GenerationBehavior.In(GenerationBehavior.OnlyDto, GenerationBehavior.OnlyFromMethods))
        {
            sb.Append($@"
        public static {dtoClassName} To{dtoCapitalized}(this {classInfo.SourceClassName} {POCO})
        {{
            {dtoClassName} {DTO} = new {dtoClassName}();
");

            foreach (var prop in classInfo.Properties)
                sb.Append(@$"
            {PropertyWriter.WritePropertyAttribution($"{DTO}.{prop.TargetName}", $"{POCO}.{prop.SourceName}")}");

            sb.Append($@"

            return {DTO};
        }}
");
        }

        if (!classInfo.ExtensionsClassGenerationInfo.GenerationBehavior.In(GenerationBehavior.OnlyDto, GenerationBehavior.OnlyToMethods))
        {
            sb.Append($@"
        public static void From{dtoCapitalized}(this {classInfo.SourceClassName} {POCO}, {dtoClassName} {DTO})
        {{");

            foreach (var prop in classInfo.Properties)
                sb.Append(@$"
            {POCO}.{prop.SourceName} = {DTO}.{prop.TargetName};");

            sb.Append($@"
        }}
");
        }

        if (!classInfo.ExtensionsClassGenerationInfo.GenerationBehavior.In(GenerationBehavior.OnlyPoco, GenerationBehavior.OnlyFromMethods))
        {
            sb.Append($@"
        public static {classInfo.SourceClassName} To{pocoCapitalized}(this {dtoClassName} {DTO})
        {{
            {classInfo.SourceClassName} {POCO} = new {classInfo.SourceClassName}();
");

            foreach (var prop in classInfo.Properties)
                sb.Append(@$"
            {POCO}.{prop.SourceName} = {DTO}.{prop.TargetName};");

            sb.Append(@$"

            return {POCO};
        }}
");
        }

        if (!classInfo.ExtensionsClassGenerationInfo.GenerationBehavior.In(GenerationBehavior.OnlyPoco, GenerationBehavior.OnlyToMethods))
        {
            sb.Append($@"
        public static void From{pocoCapitalized}(this {dtoClassName} {DTO}, {classInfo.SourceClassName} {POCO})
        {{");

            foreach (var prop in classInfo.Properties)
                sb.Append(@$"
            {DTO}.{prop.TargetName} = {POCO}.{prop.SourceName};");

            sb.Append(@$"
        }}
");
        }
        sb.Append($@"    }}
}}");

        return SourceText.From(sb.ToString(), Encoding.UTF8);
    }

    #endregion
}
