﻿using Arisoul.SourceGenerators.Diagnostics.DataTransferObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
    internal const string FullyQualifiedDtoChildPropertyMarkerName = $"{DtoNamespace}.DtoChildPropertyAttribute";
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
                    && !FullyQualifiedDtoChildPropertyMarkerName.Contains(attribute.AttributeClass.Name)))
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
            var dtoClassCode = GenerateDtoClassCode(classInfo, compilation);

            context.AddSource(dtoClassFileName, dtoClassCode);

            if (classInfo.ExtensionsClassGenerationInfo.GenerationBehavior != GenerationBehavior.NoGeneration)
            {
                var extensionsClassFileName = $"{classInfo.ExtensionsClassGenerationInfo.Name}.g.cs";
                var extensionsClassCode = GenerateDtoExtensionsClassCode(classInfo, compilation);

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

            bool isToContinue = PopulatePropsToGenerate(compilation, context, classDeclaration, propsToGenerate);

            if (!isToContinue || propsToGenerate.Count == 0)
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

    private static bool PopulatePropsToGenerate(Compilation compilation, SourceProductionContext context, ClassDeclarationSyntax classDeclaration, List<DtoGeneratorPropertyInfo> propsToGenerate)
    {
        // loop through all class members
        foreach (var member in classDeclaration.Members)
        {
            // must have attributes
            if (member.AttributeLists.Count == 0)
                continue;

            // must be a property
            var semanticModel = compilation.GetSemanticModel(member.SyntaxTree);
            if (semanticModel == null || semanticModel.GetDeclaredSymbol(member) is not IPropertySymbol propertySymbol)
                continue;

            if (!SetPropertyNameAndPopulatePropsToGenerate(propsToGenerate, propertySymbol, context, classDeclaration, compilation, member))
                return false;
        }

        return true;
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

    private static bool SetPropertyNameAndPopulatePropsToGenerate(List<DtoGeneratorPropertyInfo> propsToGenerate, IPropertySymbol propertySymbol, SourceProductionContext context, ClassDeclarationSyntax classSyntax, Compilation compilation, MemberDeclarationSyntax propertySyntax)
    {
        var semanticModel = compilation.GetSemanticModel(classSyntax.SyntaxTree);
        var classSymbol = semanticModel.GetDeclaredSymbol(classSyntax);

        // loop through attributes and find the required attribute
        foreach (var attribute in propertySymbol.GetAttributes())
        {
            // is this an expected attribute?
            if (attribute.AttributeClass == null
                || (!FullyQualifiedDtoPropertyMarkerName.Contains(attribute.AttributeClass.Name)
                && !FullyQualifiedDtoChildPropertyMarkerName.Contains(attribute.AttributeClass.Name)))
                continue;

            // the property cannot be readonly
            if (propertySymbol.IsReadOnly)
            {
                context.ReportDiagnostic(DTODiagnostics.ReadonlyPropertyDiagnostic(propertySymbol, propertySyntax));
                continue;
            }

            // if this is a child property and ExtensionsClass.GenerationBehavior != None, diagnose unsupported generation of extensions class with property childs
            if (FullyQualifiedDtoChildPropertyMarkerName.Contains(attribute.AttributeClass.Name))
            {
                var attributeSymbol = classSymbol!.GetAttributes().FirstOrDefault(a => FullyQualifiedDtoExtensionsClassGenerationMarkerName.Contains(a.AttributeClass!.Name));

                bool notSupported = attributeSymbol == null;

                if (!notSupported)
                {
                    // Find the specific argument by name
                    var targetArgumentName = nameof(DtoExtensionsClassGenerationAttribute.GenerationBehavior);
                    var argumentValue = attributeSymbol!.NamedArguments
                        .Where(namedArg => namedArg.Key == targetArgumentName)
                        .Select(namedArg => namedArg.Value.Value)
                        .FirstOrDefault();

                    notSupported = argumentValue == null;

                    if (!notSupported)
                        notSupported = ((GenerationBehavior)argumentValue!) != GenerationBehavior.NoGeneration;
                }

                if (notSupported)
                {
                    context.ReportDiagnostic(DTODiagnostics.UnsupportedExtensionsClassGenerationWithChildPropertyDiagnostic(classSyntax.Identifier.ToString(), classSyntax));
                    return false;
                }
            }

            // this is a valid attribute, go on
            string? dtoPropertyName = GetPropertyNameFromDtoAttribute(attribute);
            if (string.IsNullOrWhiteSpace(dtoPropertyName))
                dtoPropertyName = propertySymbol.Name;

            var sourceType = propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var targetType = sourceType;
            bool isChildProperty = false;
            var collectionTypeArguments = new List<CollectionTypeArgument>();

            // in case is the TargetDtoProperty, the target type must be the one received in generic
            if (FullyQualifiedDtoChildPropertyMarkerName.Contains(attribute.AttributeClass.Name))
            {
                ITypeSymbol target = attribute.AttributeClass.TypeArguments[0];
                targetType = target.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                isChildProperty = true;

                // Manage collections, so the argument types belong to the correct namespace and don't conflict in Extensions class
                if (target.IsCollection(semanticModel) && target is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType)
                    // loop through type arguments, because if is a dictionary, for example, will have more that one
                    foreach (var argument in ((INamedTypeSymbol)target).TypeArguments)
                        collectionTypeArguments.Add(new()
                        {
                            CollectionName = target.Name,
                            SourceNamespace = argument.ContainingNamespace.IsGlobalNamespace ? string.Empty : argument.ContainingNamespace.ToString(),
                            Name = argument.Name,
                        });
            }
            else
            {
                ITypeSymbol typeSymbol = semanticModel.GetTypeInfo(((PropertyDeclarationSyntax)propertySymbol.DeclaringSyntaxReferences[0].GetSyntax()).Type).Type!;
                // Manage collections, so the argument types belong to the correct namespace and don't conflict in Extensions class
                if (typeSymbol.IsCollection(semanticModel) && propertySymbol.Type is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType)
                    // loop through type arguments, because if is a dictionary, for example, will have more that one
                    foreach (var argument in namedTypeSymbol.TypeArguments)
                        collectionTypeArguments.Add(new()
                        {
                            CollectionName = propertySymbol.Type.Name,
                            SourceNamespace = argument.ContainingNamespace.ToString(),
                            Name = argument.Name,
                        });
            }

            var propInfo = new DtoGeneratorPropertyInfo(
                propertySymbol.Name,
                dtoPropertyName!,
                sourceType,
                targetType,
                isChildProperty,
                collectionTypeArguments);

            propsToGenerate.Add(propInfo);
            break;
        }

        return true;
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

    private static SourceText GenerateDtoClassCode(DtoGeneratorClassInfo classInfo, Compilation compilation)
    {
        var sb = new StringBuilder();
        sb.Append(@$"{ClassWriter.WriteClassHeader(((CSharpCompilation)compilation).LanguageVersion >= LanguageVersion.CSharp8)}

{ClassWriter.WriteUsing("System")}
{ClassWriter.WriteUsing("System.Collections.Generic")}
{ClassWriter.WriteUsing("System.Collections.ObjectModel")}

namespace {classInfo.DtoClassGenerationInfo.Namespace}
{{
    public partial class {classInfo.DtoClassGenerationInfo.Name}
    {{");

        foreach (var prop in classInfo.Properties)
        {
            string targetType = prop.TargetType;
            if (prop.CollectionTypeArguments.Any())
            {
                // in this case, it is a collection, and to avoid cases where the class name is the same in different namespaces,
                // where extensions generation will raise an error because it cannot convert a collection of a class in a namespace, to another
                // it is required to set the type arguments types with full original namespace

                StringBuilder targetTypeSb = new StringBuilder();
                foreach (var arg in prop.CollectionTypeArguments)
                {
                    if (targetTypeSb.Length == 0)
                        targetTypeSb.Append($"{arg.CollectionName}<");
                    else
                        targetTypeSb.Append(", ");

                    targetTypeSb.Append(arg);
                }

                targetTypeSb.Append(">");
                targetType = targetTypeSb.ToString();
            }

            sb.Append(@$"
        {PropertyWriter.WritePublicPropertySimple(targetType, prop.TargetName)}");
        }

        sb.Append(@"
    }
}");

        return SourceText.From(sb.ToString(), Encoding.UTF8);
    }

    private static SourceText GenerateDtoExtensionsClassCode(DtoGeneratorClassInfo classInfo, Compilation compilation)
    {
        string pocoCapitalized = POCO.Capitalize()!;
        string dtoCapitalized = DTO.Capitalize()!;
        string dtoClassName = classInfo.DtoClassGenerationInfo.FullyQualifiedName;
        string extensionsClassName = classInfo.ExtensionsClassGenerationInfo.Name;

        var sb = new StringBuilder();

        sb.Append($@"{ClassWriter.WriteClassHeader(((CSharpCompilation)compilation).LanguageVersion >= LanguageVersion.CSharp8)}

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
    public static partial class {extensionsClassName}
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
