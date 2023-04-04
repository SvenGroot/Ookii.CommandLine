using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;

namespace Ookii.CommandLine.Generator;

internal static class SymbolExtensions
{
    public static bool DerivesFrom(this INamedTypeSymbol symbol, string baseClassName)
    {
        INamedTypeSymbol? current = symbol;
        while (current != null)
        {
            if (current.ToDisplayString() == baseClassName)
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    public static bool IsNullableValueType(this INamedTypeSymbol symbol)
        => !symbol.IsReferenceType && symbol.IsGenericType && symbol.ConstructedFrom.ToDisplayString() == "System.Nullable<T>";

    public static string CreateInstantiation(this AttributeData attribute)
    {
        var ctorArgs = attribute.ConstructorArguments.Select(c => c.ToCSharpString());
        var namedArgs = attribute.NamedArguments.Select(n => $"{n.Key} = {n.Value.ToCSharpString()}");
        return $"new {attribute.AttributeClass?.ToDisplayString()}({string.Join(", ", ctorArgs)}) {{ {string.Join(", ", namedArgs)} }}";
    }
}
