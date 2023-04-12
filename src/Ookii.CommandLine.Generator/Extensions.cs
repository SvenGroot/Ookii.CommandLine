using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Diagnostics;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Ookii.CommandLine.Generator;

internal static class Extensions
{
    public static bool DerivesFrom(this ITypeSymbol symbol, string baseClassName)
    {
        var current = symbol;
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

    public static bool DerivesFrom(this ITypeSymbol type, ITypeSymbol baseClass)
    {
        var current = type;
        while (current != null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseClass))
            {
                return true;
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
        => (type is not INamedTypeSymbol namedType || namedType.IsNullableValueType()) || (type.IsReferenceType && type.NullableAnnotation != NullableAnnotation.NotAnnotated);

    public static INamedTypeSymbol GetUnderlyingType(this INamedTypeSymbol type)
        => type.IsNullableValueType() ? (INamedTypeSymbol)type.TypeArguments[0] : type;

    public static ITypeSymbol GetUnderlyingType(this ITypeSymbol type)
        => type is INamedTypeSymbol namedType && namedType.IsNullableValueType() ? (INamedTypeSymbol)namedType.TypeArguments[0] : type;

    public static bool IsEnum(this ITypeSymbol type) => type.BaseType?.SpecialType == SpecialType.System_Enum;

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
        if (interfaceType == null)
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
        => SymbolEqualityComparer.Default.Equals(targetType, sourceType) || sourceType.DerivesFrom(targetType)
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
}
