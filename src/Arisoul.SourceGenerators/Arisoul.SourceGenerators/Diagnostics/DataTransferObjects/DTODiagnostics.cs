using DC = Arisoul.SourceGenerators.Diagnostics.DiagnosticsConstants.DTO;

namespace Arisoul.SourceGenerators.Diagnostics.DataTransferObjects;

public class DTODiagnostics
{
    public static Diagnostic ReadonlyPropertyDiagnostic(IPropertySymbol propertySymbol, MemberDeclarationSyntax syntax)
    {
        var descriptor = new DiagnosticDescriptor(
            id: DC.ReadOnlyProperty.ID,
            title: DC.ReadOnlyProperty.Title,
            messageFormat: string.Format(DC.ReadOnlyProperty.Description, propertySymbol.Name),
            category: DC.ReadOnlyProperty.Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        return Diagnostic.Create(descriptor, syntax.GetLocation());
    }

    public static Diagnostic AbstractClassDiagnostic(string className, MemberDeclarationSyntax syntax)
    {
        var descriptor = new DiagnosticDescriptor(
            id: DC.AbstractClass.ID,
            title: DC.AbstractClass.Title,
            messageFormat: string.Format(DC.AbstractClass.Description, className),
            category: DC.AbstractClass.Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        return Diagnostic.Create(descriptor, syntax.GetLocation());
    }
}
