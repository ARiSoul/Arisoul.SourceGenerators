using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Arisoul.SourceGenerators.Extensions;

internal static class SyntaxNodeExtensions
{
    internal static string GetNamespace(this SyntaxNode? node)
       => node switch
       {
           NamespaceDeclarationSyntax namespaceNode => namespaceNode.Name.ToString(),
           FileScopedNamespaceDeclarationSyntax fileScopedNamespaceNode => fileScopedNamespaceNode.Name.ToString(),
           { } => GetNamespace(node.Parent),
           _ => throw new InvalidOperationException("Could not find namespace")
       };
}
