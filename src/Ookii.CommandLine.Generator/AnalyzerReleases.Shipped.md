; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 4.0

### New Rules

Rule ID | Category          | Severity | Notes
--------|-------------------|----------|---------------------------------------------------------------------------------
OCL0001 | Ookii.CommandLine | Error    | TypeNotReferenceType
OCL0002 | Ookii.CommandLine | Error    | ClassNotPartial
OCL0003 | Ookii.CommandLine | Error    | ClassIsGeneric
OCL0004 | Ookii.CommandLine | Error    | ClassIsNested
OCL0005 | Ookii.CommandLine | Error    | InvalidArrayRank
OCL0006 | Ookii.CommandLine | Error    | PropertyIsReadOnly
OCL0007 | Ookii.CommandLine | Error    | NoConverter
OCL0008 | Ookii.CommandLine | Error    | InvalidMethodSignature
OCL0009 | Ookii.CommandLine | Error    | NonRequiredInitOnlyProperty
OCL0010 | Ookii.CommandLine | Error    | GeneratedCustomParsingCommand
OCL0011 | Ookii.CommandLine | Error    | PositionalArgumentAfterMultiValue
OCL0012 | Ookii.CommandLine | Error    | PositionalRequiredArgumentAfterOptional
OCL0013 | Ookii.CommandLine | Error    | InvalidAssemblyName
OCL0014 | Ookii.CommandLine | Error    | UnknownAssemblyName
OCL0015 | Ookii.CommandLine | Error    | ArgumentConverterStringNotSupported or ParentCommandStringNotSupported
OCL0016 | Ookii.CommandLine | Warning  | IgnoredTypeConverterAttribute
OCL0017 | Ookii.CommandLine | Warning  | NonPublicStaticMethod
OCL0018 | Ookii.CommandLine | Warning  | NonPublicInstanceProperty
OCL0019 | Ookii.CommandLine | Warning  | CommandAttributeWithoutInterface or CommandInterfaceWithoutAttribute
OCL0020 | Ookii.CommandLine | Warning  | DefaultValueWithRequired or DefaultValueWithMultiValue or DefaultValueWithMethod
OCL0021 | Ookii.CommandLine | Warning  | IsRequiredWithRequiredProperty
OCL0022 | Ookii.CommandLine | Warning  | DuplicatePosition
OCL0023 | Ookii.CommandLine | Warning  | ShortAliasWithoutShortName
OCL0024 | Ookii.CommandLine | Warning  | AliasWithoutLongName
OCL0025 | Ookii.CommandLine | Warning  | IsHiddenWithPositionalOrRequired
OCL0026 | Ookii.CommandLine | Warning  | InvalidGeneratedConverterNamespace
OCL0027 | Ookii.CommandLine | Warning  | IgnoredAttributeForNonDictionary
OCL0028 | Ookii.CommandLine | Warning  | IgnoredAttributeForDictionaryWithConverter
OCL0029 | Ookii.CommandLine | Warning  | IgnoredAttributeForNonMultiValue
OCL0030 | Ookii.CommandLine | Warning  | ArgumentStartsWithNumber
OCL0031 | Ookii.CommandLine | Error    | NoLongOrShortName
OCL0032 | Ookii.CommandLine | Warning  | IsShortIgnored
OCL0033 | Ookii.CommandLine | Warning  | ArgumentWithoutDescription
OCL0034 | Ookii.CommandLine | Warning  | CommandWithoutDescription
OCL0035 | Ookii.CommandLine | Warning  | IgnoredAttributeForNonCommand
OCL0036 | Ookii.CommandLine | Warning  | IgnoredFriendlyNameAttribute
OCL0037 | Ookii.CommandLine | Error    | UnsupportedLanguageVersion
OCL0038 | Ookii.CommandLine | Error    | MixedImplicitExplicitPositions
OCL0039 | Ookii.CommandLine | Warning  | UnsupportedInitializerSyntax

## Release 4.1

### New Rules

Rule ID | Category          | Severity | Notes
--------|-------------------|----------|--------------------------------
OCL0040 | Ookii.CommandLine | Warning  | ParserShouldBeGenerated
OCL0041 | Ookii.CommandLine | Warning  | ValidateEnumInvalidType
OCL0042 | Ookii.CommandLine | Warning  | ValidateEnumWithCustomConverter
