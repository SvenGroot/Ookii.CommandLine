using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Diagnostics;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Ookii.CommandLine.Generator;

internal static class Extensions
{
    // This is the format used to emit type names in the output. It includes the global namespace
    // so that this doesn't break if the class matches the namespace name.
    private static readonly SymbolDisplayFormat QualifiedFormat = new(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

    public static bool DerivesFrom(this ITypeSymbol symbol, ITypeSymbol? baseClass)
    {
        if (baseClass == null)
        {
            return false;
        }

        var current = symbol;
        while (current != null)
        {
            if (current.SymbolEquals(baseClass))
            {
                return true;
            }

            // No point checking base classes if the type we're looking for is sealed.
            if (baseClass.IsSealed)
            {
                break;
            }

            current = current.BaseType;
        }

        return false;
    }

    public static bool IsNullableValueType(this INamedTypeSymbol type)
        => !type.IsReferenceType && type.IsGenericType && type.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T;

    public static bool IsNullableValueType(this ITypeSymbol type)
        => type is INamedTypeSymbol namedType && namedType.IsNullableValueType();

    public static bool AllowsNull(this ITypeSymbol type)
        => (type is INamedTypeSymbol namedType && namedType.IsNullableValueType()) || (type.IsReferenceType && type.NullableAnnotation != NullableAnnotation.NotAnnotated);

    public static INamedTypeSymbol GetUnderlyingType(this INamedTypeSymbol type)
        => type.IsNullableValueType() ? (INamedTypeSymbol)type.TypeArguments[0] : type;

    public static ITypeSymbol GetUnderlyingType(this ITypeSymbol type)
        => type is INamedTypeSymbol namedType && namedType.IsNullableValueType() ? (INamedTypeSymbol)namedType.TypeArguments[0] : type;

    public static INamedTypeSymbol? FindGenericInterface(this ITypeSymbol type, ITypeSymbol? interfaceToFind)
    {
        if (interfaceToFind == null)
        {
            return null;
        }

        if (type.TypeKind == TypeKind.Interface && ((INamedTypeSymbol)type).IsConstructedFrom(interfaceToFind))
        {
            return (INamedTypeSymbol)type;
        }

        foreach (var iface in type.AllInterfaces)
        {
            if (iface.IsConstructedFrom(interfaceToFind))
            {
                return iface;
            }
        }

        return null;
    }

    public static bool IsConstructedFrom(this INamedTypeSymbol type, ITypeSymbol typeDefinition)
    {
        var realType = type;
        if (realType.IsGenericType)
        {
            realType = realType.ConstructedFrom;
        }

        return realType.SymbolEquals(typeDefinition);
    }

    public static bool ImplementsInterface(this ITypeSymbol type, ITypeSymbol? interfaceType)
    {
        if (interfaceType == null || interfaceType.TypeKind != TypeKind.Interface)
        {
            return false;
        }

        if (type.SymbolEquals(interfaceType))
        {
            return true;
        }

        foreach (var iface in type.AllInterfaces)
        {
            if (iface.SymbolEquals(interfaceType))
            {
                return true;
            }
        }

        return false;
    }

    public static bool CanAssignFrom(this ITypeSymbol targetType, ITypeSymbol sourceType)
        => targetType.SymbolEquals(sourceType) || sourceType.DerivesFrom(targetType)
            || sourceType.ImplementsInterface(targetType);

    public static string CreateInstantiation(this AttributeData attribute)
    {
        var ctorArgs = attribute.ConstructorArguments.Select(c => c.ToFullCSharpString());
        var namedArgs = attribute.NamedArguments.Select(n => $"{n.Key} = {n.Value.ToFullCSharpString()}");
        return $"new {attribute.AttributeClass?.ToDisplayString()}({string.Join(", ", ctorArgs)}) {{ {string.Join(", ", namedArgs)} }}";
    }

    public static string ToCSharpString(this bool value)
    {
        return value ? "true" : "false";
    }

    public static string ToFullCSharpString(this TypedConstant constant)
    {
        return constant.Kind switch
        {
            TypedConstantKind.Array => $"new {constant.Type?.ToDisplayString()} {constant.ToCSharpString()}",
            _ => constant.ToCSharpString(),
        };
    }

    public static bool SymbolEquals(this ISymbol left, ISymbol? right) => SymbolEqualityComparer.Default.Equals(left, right);

    public static string ToIdentifier(this string displayName, string suffix)
    {
        var builder = new StringBuilder(displayName.Length + suffix.Length);
        foreach (var ch in displayName)
        {
            builder.Append(char.IsLetterOrDigit(ch) ? ch : '_');
        }

        builder.Append(suffix);
        return builder.ToString();
    }

    public static IMethodSymbol? FindConstructor(this INamedTypeSymbol type, ITypeSymbol? argumentType)
    {
        foreach (var ctor in type.Constructors)
        {
            if (!ctor.IsStatic && ctor.DeclaredAccessibility == Accessibility.Public && ctor.Parameters.Length == 1 &&
                ctor.Parameters[0].Type.SymbolEquals(argumentType))
            {
                return ctor;
            }
        }

        return null;
    }

    // Using a ref parameter with bool return allows me to chain these together.
    public static bool CheckType(this AttributeData data, ITypeSymbol? attributeType, ref AttributeData? attribute)
    {
        if (!(data.AttributeClass?.DerivesFrom(attributeType) ?? false))
        {
            return false;
        }

        attribute ??= data;
        return true;
    }

    // Using a ref parameter with bool return allows me to chain these together.
    public static bool CheckType(this AttributeData data, ITypeSymbol? attributeType, ref List<AttributeData>? attributes)
    {
        if (!(data.AttributeClass?.DerivesFrom(attributeType) ?? false))
        {
            return false;
        }

        attributes ??= new();
        attributes.Add(data);
        return true;
    }

    public static TypedConstant? GetNamedArgument(this AttributeData data, string name)
    {
        foreach (var arg in data.NamedArguments)
        {
            if (arg.Key == name)
            {
                return arg.Value;
            }
        }

        return null;
    }

    public static AttributeData? GetAttribute(this ISymbol symbol, ITypeSymbol type)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if (type.SymbolEquals(attribute.AttributeClass))
            {
                return attribute;
            }
        }

        return null;
    }

    public static Location? GetLocation(this AttributeData attribute)
        => attribute.ApplicationSyntaxReference?.SyntaxTree.GetLocation(attribute.ApplicationSyntaxReference.Span);

    public static string ToQualifiedName(this ITypeSymbol symbol)
    {
        return symbol.ToDisplayString(QualifiedFormat);
    }
}
