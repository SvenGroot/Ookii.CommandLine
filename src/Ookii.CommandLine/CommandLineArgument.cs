// Copyright (c) Sven Groot (Ookii.org)
using Ookii.CommandLine.Conversion;
using Ookii.CommandLine.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ookii.CommandLine;

/// <summary>
/// Provides information about command line arguments that are recognized by a <see cref="CommandLineParser"/>.
/// </summary>
/// <threadsafety static="true" instance="false"/>
/// <seealso cref="CommandLineArgumentAttribute"/>
public abstract class CommandLineArgument
{
    #region Nested types

    private interface IValueHelper
    {
        object? Value { get; }
        bool SetValue(CommandLineArgument argument, object? value);
        void ApplyValue(CommandLineArgument argument, object target);
    }

    private class SingleValueHelper : IValueHelper
    {
        public SingleValueHelper(object? initialValue)
        {
            Value = initialValue;
        }

        public object? Value { get; private set; }

        public void ApplyValue(CommandLineArgument argument, object target)
        {
            argument.SetProperty(target, Value);
        }

        public bool SetValue(CommandLineArgument argument, object? value)
        {
            Value = value;
            return true;
        }
    }

    private class MultiValueHelper<T> : IValueHelper
    {
        // The actual element type may not be nullable. This is handled by the allow null check
        // when parsing the value. Here, we always treat the values as if they're nullable.
        private readonly List<T?> _values = new();

        public object? Value => _values.ToArray();

        public void ApplyValue(CommandLineArgument argument, object target)
        {
            if (argument.CanSetProperty)
            {
                argument.SetProperty(target, Value);
                return;
            }

            var list = (ICollection<T?>?)argument.GetProperty(target) ?? throw new InvalidOperationException();
            list.Clear();
            foreach (var value in _values)
            {
                list.Add(value);
            }
        }

        public bool SetValue(CommandLineArgument argument, object? value)
        {
            _values.Add((T?)value);
            return true;
        }
    }

    private class DictionaryValueHelper<TKey, TValue> : IValueHelper
        where TKey : notnull
    {
        // The actual value type may not be nullable. This is handled by the allow null check.
        private readonly Dictionary<TKey, TValue?> _dictionary = new();
        private readonly bool _allowDuplicateKeys;
        private readonly bool _allowNullValues;

        public DictionaryValueHelper(bool allowDuplicateKeys, bool allowNullValues)
        {
            _allowDuplicateKeys = allowDuplicateKeys;
            _allowNullValues = allowNullValues;
        }

        public object? Value => _dictionary;

        public void ApplyValue(CommandLineArgument argument, object target)
        {
            if (argument.CanSetProperty)
            {
                argument.SetProperty(target, _dictionary);
                return;
            }

            var dictionary = (IDictionary<TKey, TValue?>?)argument.GetProperty(target)
                ?? throw new InvalidOperationException();

            dictionary.Clear();
            foreach (var pair in _dictionary)
            {
                dictionary.Add(pair.Key, pair.Value);
            }
        }

        public bool SetValue(CommandLineArgument argument, object? value)
        {
            // ConvertToArgumentType is guaranteed to return non-null for dictionary arguments.
            var pair = (KeyValuePair<TKey?, TValue?>)value!;

            // With the KeyValuePairConverter, these should already be checked, but it's still
            // checked here to deal with custom converters.
            if (pair.Key == null || (!_allowNullValues && pair.Value == null))
            {
                throw argument._parser.StringProvider.CreateException(CommandLineArgumentErrorCategory.NullArgumentValue, argument);
            }

            try
            {
                if (_allowDuplicateKeys)
                {
                    _dictionary[pair.Key] = pair.Value;
                }
                else
                {
                    _dictionary.Add(pair.Key, pair.Value);
                }
            }
            catch (ArgumentException ex)
            {
                throw argument._parser.StringProvider.CreateException(CommandLineArgumentErrorCategory.InvalidDictionaryValue, ex, argument, value.ToString());
            }

            return true;
        }
    }

    private class MethodValueHelper : IValueHelper
    {
        public object? Value { get; private set; }

        public void ApplyValue(CommandLineArgument argument, object target)
        {
            throw new InvalidOperationException();
        }

        public bool SetValue(CommandLineArgument argument, object? value)
        {
            Value = value;
            return argument.CallMethod(value);
        }
    }

    private class HelpArgument : CommandLineArgument
    {
        public HelpArgument(CommandLineParser parser, string argumentName, char shortName, char shortAlias)
            : base(CreateInfo(parser, argumentName, shortName, shortAlias))
        { 
        }

        protected override bool CanSetProperty => false;

        private static ArgumentInfo CreateInfo(CommandLineParser parser, string argumentName, char shortName, char shortAlias)
        {
            var info = new ArgumentInfo()
            {
                Parser = parser,
                ArgumentName = argumentName,
                Kind = ArgumentKind.Method,
                Long = true,
                Short = true,
                ShortName = parser.StringProvider.AutomaticHelpShortName(),
                ArgumentType = typeof(bool),
                ElementTypeWithNullable = typeof(bool),
                ElementType = typeof(bool),
                Description = parser.StringProvider.AutomaticHelpDescription(),
                MemberName = "AutomaticHelp",
                CancelParsing = true,
                Validators = Enumerable.Empty<ArgumentValidationAttribute>(),
                Converter = Conversion.BooleanConverter.Instance,
            };

            if (parser.Mode == ParsingMode.LongShort)
            {
                if (parser.ShortArgumentNameComparer!.Compare(shortAlias, shortName) != 0)
                {
                    info.ShortAliases = new[] { shortAlias };
                }
            }
            else
            {
                var shortNameString = shortName.ToString();
                var shortAliasString = shortAlias.ToString();
                info.Aliases = string.Compare(shortAliasString, shortNameString, parser.ArgumentNameComparison) == 0
                    ? new[] { shortNameString }
                    : new[] { shortNameString, shortAliasString };
            }

            return info;
        }

        protected override bool CallMethod(object? value) => true;
        protected override object? GetProperty(object target) => throw new InvalidOperationException();
        protected override void SetProperty(object target, object? value) => throw new InvalidOperationException();
    }

    private class VersionArgument : CommandLineArgument
    {
        public VersionArgument(CommandLineParser parser, string argumentName)
            : base(CreateInfo(parser, argumentName))
        {
        }

        protected override bool CanSetProperty => false;

        private static ArgumentInfo CreateInfo(CommandLineParser parser, string argumentName)
        {
            return new ArgumentInfo()
            {
                Parser = parser,
                ArgumentName = argumentName,
                Kind = ArgumentKind.Method,
                Long = true,
                ArgumentType = typeof(bool),
                ElementTypeWithNullable = typeof(bool),
                ElementType = typeof(bool),
                Description = parser.StringProvider.AutomaticVersionDescription(),
                MemberName = nameof(AutomaticVersion),
                Validators = Enumerable.Empty<ArgumentValidationAttribute>(),
                Converter = Conversion.BooleanConverter.Instance
            };
        }

        protected override bool CallMethod(object? value) => AutomaticVersion(Parser);
        protected override object? GetProperty(object target) => throw new InvalidOperationException();
        protected override void SetProperty(object target, object? value) => throw new InvalidOperationException();
    }

    internal struct ArgumentInfo
    {
        public CommandLineParser Parser { get; set; }
        public string MemberName { get; set; }
        public string ArgumentName { get; set; }
        public bool Long { get; set; }
        public bool Short { get; set; }
        public char ShortName { get; set; }
        public IEnumerable<string>? Aliases { get; set; }
        public IEnumerable<char>? ShortAliases { get; set; }
        public Type ArgumentType { get; set; }
        public Type ElementType { get; set; }
        public Type ElementTypeWithNullable { get; set; }
        public ArgumentKind Kind { get; set; }
        public ArgumentConverter Converter { get; set; }
        public int? Position { get; set; }
        public bool IsRequired { get; set; }
        public object? DefaultValue { get; set; }
        public string? Description { get; set; }
        public string? ValueDescription { get; set; }
        public string? MultiValueSeparator { get; set; }
        public bool AllowMultiValueWhiteSpaceSeparator { get; set; }
        public string? KeyValueSeparator { get; set; }
        public bool AllowDuplicateDictionaryKeys { get; set; }
        public bool AllowNull { get; set; }
        public bool CancelParsing { get; set; }
        public bool IsHidden { get; set; }
        public Type? KeyType { get; set; }
        public Type? ValueType { get; set; }
        public IEnumerable<ArgumentValidationAttribute> Validators { get; set; }
    }

    #endregion

    private readonly CommandLineParser _parser;
    private readonly ArgumentConverter _converter;
    private readonly string _argumentName;
    private readonly bool _hasLongName = true;
    private readonly char _shortName;
    private readonly ReadOnlyCollection<string>? _aliases;
    private readonly ReadOnlyCollection<char>? _shortAliases;
    private readonly Type _argumentType;
    private readonly Type _elementType;
    private readonly Type _elementTypeWithNullable;
    private readonly Type? _keyType;
    private readonly Type? _valueType;
    private readonly string? _description;
    private readonly bool _isRequired;
    private readonly string _memberName;
    private readonly object? _defaultValue;
    private readonly ArgumentKind _argumentKind;
    private readonly bool _allowDuplicateDictionaryKeys;
    private readonly string? _multiValueSeparator;
    private readonly bool _allowMultiValueWhiteSpaceSeparator;
    private readonly string? _keyValueSeparator;
    private readonly bool _allowNull;
    private readonly bool _cancelParsing;
    private readonly bool _isHidden;
    private readonly IEnumerable<ArgumentValidationAttribute> _validators;
    private string? _valueDescription;
    private IValueHelper? _valueHelper;
    private ReadOnlyMemory<char> _usedArgumentName;

    internal CommandLineArgument(ArgumentInfo info)
    {
        // If this method throws anything other than a NotSupportedException, it constitutes a bug in the Ookii.CommandLine library.
        _parser = info.Parser;
        _memberName = info.MemberName;
        _argumentName = info.ArgumentName;
        if (_parser.Mode == ParsingMode.LongShort)
        {
            _hasLongName = info.Long;
            if (info.Short)
            {
                if (info.ShortName != '\0')
                {
                    _shortName = info.ShortName;
                }
                else
                {
                    _shortName = _argumentName[0];
                }
            }

            if (!HasLongName)
            {
                if (!HasShortName)
                {
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.NoLongOrShortName, _argumentName));
                }

                _argumentName = _shortName.ToString();
            }
        }

        if (HasLongName && info.Aliases != null)
        {
            _aliases = new(info.Aliases.ToArray());
        }

        if (HasShortName && info.ShortAliases != null)
        {
            _shortAliases = new(info.ShortAliases.ToArray());
        }

        _argumentType = info.ArgumentType;
        _argumentKind = info.Kind;
        _elementTypeWithNullable = info.ElementTypeWithNullable;
        _elementType = info.ElementType;
        _keyType = info.KeyType;
        _valueType = info.ValueType;
        _description = info.Description;
        _isRequired = info.IsRequired;
        _allowNull = info.AllowNull;
        _cancelParsing = info.CancelParsing;
        _validators = info.Validators;
        // Required or positional arguments cannot be hidden.
        _isHidden = info.IsHidden && !info.IsRequired && info.Position == null;
        Position = info.Position;
        _converter = info.Converter;
        _defaultValue = ConvertToArgumentTypeInvariant(info.DefaultValue);
        _valueDescription = info.ValueDescription;
        _allowDuplicateDictionaryKeys = info.AllowDuplicateDictionaryKeys;
        _allowMultiValueWhiteSpaceSeparator = IsMultiValue && !IsSwitch && info.AllowMultiValueWhiteSpaceSeparator;
        _allowNull = info.AllowNull;
        _keyValueSeparator = info.KeyValueSeparator;
        _multiValueSeparator = info.MultiValueSeparator;
    }

    /// <summary>
    /// Gets the <see cref="CommandLineParser"/> that this argument belongs to.
    /// </summary>
    /// <value>
    /// An instance of the <see cref="CommandLineParser"/> class.
    /// </value>
    public CommandLineParser Parser => _parser;

    /// <summary>
    /// Gets the name of the property, method, or constructor parameter that defined this command line argument.
    /// </summary>
    /// <value>
    /// The name of the property, method, or constructor parameter that defined this command line argument.
    /// </value>
    public string MemberName
    {
        get { return _memberName; }
    }

    /// <summary>
    /// Gets the name of this argument.
    /// </summary>
    /// <value>
    /// The name of this argument.
    /// </value>
    /// <remarks>
    /// <para>
    ///   This name is used to supply an argument value by name on the command line, and to describe the argument in the usage help
    ///   generated by <see cref="CommandLineParser.WriteUsage(UsageWriter)"/>.
    /// </para>
    /// <para>
    ///   If the <see cref="CommandLineParser.Mode"/> property is <see cref="ParsingMode.LongShort"/>,
    ///   and the <see cref="HasLongName"/> property is <see langword="false"/>, this returns
    ///   the short name of the argument. Otherwise, it returns the long name.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineArgumentAttribute.ArgumentName"/>
    public string ArgumentName => _argumentName;

    /// <summary>
    /// Gets the short name of this argument.
    /// </summary>
    /// <value>
    /// The short name of the argument, or a null character ('\0') if it doesn't have one.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The short name is only used if the parser is using <see cref="ParsingMode.LongShort"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineArgumentAttribute.ShortName"/>
    public char ShortName => _shortName;

    /// <summary>
    /// Gets the name of this argument, with the appropriate argument name prefix.
    /// </summary>
    /// <value>
    /// The name of the argument, with an argument name prefix.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If the <see cref="CommandLineParser.Mode"/> property is <see cref="ParsingMode.LongShort"/>,
    ///   this will use the long name with the long argument prefix if the argument has a long
    ///   name, and the short name with the primary short argument prefix if not.
    /// </para>
    /// <para>
    ///   For <see cref="ParsingMode.Default"/>, the prefix used is the first prefix specified
    ///   in the <see cref="CommandLineParser.ArgumentNamePrefixes"/> property.
    /// </para>
    /// </remarks>
    public string ArgumentNameWithPrefix
    {
        get
        {
            var prefix = (_parser.Mode == ParsingMode.LongShort && HasLongName)
                ? _parser.LongArgumentNamePrefix
                : _parser.ArgumentNamePrefixes[0];

            return prefix + _argumentName;
        }
    }

    /// <summary>
    /// Gets the long argument name with the long prefix.
    /// </summary>
    /// <value>
    /// The long argument name with its prefix, or <see langword="null"/> if the <see cref="CommandLineParser.Mode"/>
    /// property is not <see cref="ParsingMode.LongShort"/> or the <see cref="HasLongName"/>
    /// property is <see langword="false"/>.
    /// </value>
    public string? LongNameWithPrefix
    {
        get
        {
            return (_parser.Mode == ParsingMode.LongShort && HasLongName)
                ? _parser.LongArgumentNamePrefix + _argumentName
                : null;
        }
    }

    /// <summary>
    /// Gets the short argument name with the primary short prefix.
    /// </summary>
    /// <value>
    /// The short argument name with its prefix, or <see langword="null"/> if the <see cref="CommandLineParser.Mode"/>
    /// property is not <see cref="ParsingMode.LongShort"/> or the <see cref="HasShortName"/>
    /// property is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The prefix used is the first prefix specified in the <see cref="CommandLineParser.ArgumentNamePrefixes"/>
    ///   property.
    /// </para>
    /// </remarks>
    public string? ShortNameWithPrefix
    {
        get
        {
            return (_parser.Mode == ParsingMode.LongShort && HasShortName)
                ? _parser.ArgumentNamePrefixes[0] + _shortName
                : null;
        }
    }

    /// <summary>
    /// Gets a value that indicates whether the argument has a short name.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the argument has a short name; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The short name is only used if the parser is using <see cref="ParsingMode.LongShort"/>.
    ///   Otherwise, this property is always <see langword="false"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineArgumentAttribute.IsShort"/>
    public bool HasShortName => _shortName != '\0';

    /// <summary>
    /// Gets a value that indicates whether the argument has a long name.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if the argument has a long name; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If the <see cref="CommandLineParser.Mode"/> property is not <see cref="ParsingMode.LongShort"/>,
    ///   this property is always <see langword="true"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineArgumentAttribute.IsLong"/>
    public bool HasLongName => _hasLongName;

    /// <summary>
    /// Gets the alternative names for this command line argument.
    /// </summary>
    /// <value>
    /// A list of alternative names for this command line argument, or an empty collection if none were specified.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If the <see cref="CommandLineParser.Mode"/> property is <see cref="ParsingMode.LongShort"/>,
    ///   and the <see cref="HasLongName"/> property is <see langword="false"/>, this property
    ///   will always return an empty collection.
    /// </para>
    /// </remarks>
    /// <seealso cref="AliasAttribute"/>
    public ReadOnlyCollection<string>? Aliases => _aliases;

    /// <summary>
    /// Gets the alternative short names for this command line argument.
    /// </summary>
    /// <value>
    /// A list of alternative short names for this command line argument, or an empty collection if none were specified.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If the <see cref="CommandLineParser.Mode"/> property is not <see cref="ParsingMode.LongShort"/>,
    ///   or the <see cref="HasShortName"/> property is <see langword="false"/>, this property
    ///   will always return an empty collection.
    /// </para>
    /// </remarks>
    /// <seealso cref="ShortAliasAttribute"/>
    public ReadOnlyCollection<char>? ShortAliases => _shortAliases;

    /// <summary>
    /// Gets the type of the argument's value.
    /// </summary>
    /// <value>
    /// The <see cref="Type"/> of the argument.
    /// </value>
    public Type ArgumentType
    {
        get { return _argumentType; }
    }

    /// <summary>
    /// Gets the type of the elements of the argument value.
    /// </summary>
    /// <value>
    /// If the <see cref="IsMultiValue"/> property is <see langword="true"/>, the <see cref="Type"/>
    /// of each individual value; if the argument type is an instance of <see cref="Nullable{T}"/>,
    /// the type <c>T</c>; otherwise, the same value as the <see cref="ArgumentType"/>
    /// property.
    /// </value>
    public Type ElementType => _elementType;

    /// <summary>
    /// Gets the position of this argument.
    /// </summary>
    /// <value>
    /// The position of this argument, or <see langword="null"/> if this is not a positional argument.
    /// </value>
    /// <remarks>
    /// <para>
    ///   A positional argument is created either using a constructor parameter on the command line arguments type,
    ///   or by using the <see cref="CommandLineArgumentAttribute.Position"/> property.
    /// </para>
    /// <para>
    ///   The <see cref="Position"/> property reflects the actual position of the positional argument. For positional
    ///   arguments created from properties this doesn't need to match the original value of the <see cref="CommandLineArgumentAttribute.Position"/> property.
    /// </para>
    /// </remarks>
    public int? Position { get; internal set; }

    /// <summary>
    /// Gets a value that indicates whether the argument is required.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if the argument's value must be specified on the command line; <see langword="false"/> if the argument may be omitted.
    /// </value>
    /// <remarks>
    /// <para>
    ///   An argument defined by a constructor parameter is required if the parameter does not
    ///   have a default value. An argument defined by a property or method is required if its
    ///   <see cref="CommandLineArgumentAttribute.IsRequired"/> property is <see langword="true"/>.
    /// </para>
    /// </remarks>
    public bool IsRequired
    {
        get { return _isRequired; }
    }

    /// <summary>
    /// Gets the default value for an argument.
    /// </summary>
    /// <value>
    /// The default value of the argument.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The default value of an argument defined by a constructor parameter is specified by
    ///   the default value of that parameter. For an argument defined by a property, the default
    ///   value is set by the <see cref="CommandLineArgumentAttribute.DefaultValue"/> property.
    /// </para>
    /// <para>
    ///   This value is only used if <see cref="IsRequired"/> is <see langword="false"/>.
    /// </para>
    /// </remarks>
    public object? DefaultValue
    {
        get { return _defaultValue; }
    }

    /// <summary>
    /// Gets the description of the argument.
    /// </summary>
    /// <value>
    /// The description of the argument.
    /// </value>
    /// <remarks>
    /// <para>
    ///   This property is used only when generating usage information using <see cref="CommandLineParser.WriteUsage"/>.
    /// </para>
    /// <para>
    ///   To set the description of an argument, apply the <see cref="System.ComponentModel.DescriptionAttribute"/>
    ///   attribute to the constructor parameter, property, or method that defines the argument.
    /// </para>
    /// </remarks>
    public string Description
    {
        get { return _description ?? string.Empty; }
    }

    /// <summary>
    /// Gets the short description of the argument's value to use when printing usage information.
    /// </summary>
    /// <value>
    /// The description of the value.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The value description is a short, typically one-word description that indicates the type of value that
    ///   the user should supply. By default, the type of the property is used, applying the <see cref="NameTransform"/>
    ///   specified by the <see cref="ParseOptions.ValueDescriptionTransform"/> property or the
    ///   <see cref="ParseOptionsAttribute.ValueDescriptionTransform"/> property. If this is a
    ///   multi-value argument, the <see cref="ElementType"/> is used. If the type is a nullable
    ///   value type, its underlying type is used.
    /// </para>
    /// <para>
    ///   The value description is used only when generating usage help. For example, the usage for an argument named Sample with
    ///   a value description of String would look like "-Sample &lt;String&gt;".
    /// </para>
    /// <note>
    ///   This is not the long description used to describe the purpose of the argument. That can be retrieved
    ///   using the <see cref="Description"/> property.
    /// </note>
    /// </remarks>
    /// <seealso cref="CommandLineArgumentAttribute.ValueDescription"/>
    public string ValueDescription => _valueDescription ??= DetermineValueDescription();

    /// <summary>
    /// Gets a value indicating whether this argument is a switch argument.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if the argument is a switch argument; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   A switch argument is an argument that doesn't need a value; instead, its value is <see langword="true"/> or
    ///   <see langword="false"/> depending on whether the argument is present on the command line.
    /// </para>
    /// <para>
    ///   A argument is a switch argument when it is not positional, and its <see cref="ElementType"/>
    ///   is a <see cref="bool"/>.
    /// </para>
    /// </remarks>
    public bool IsSwitch => Position == null && ElementType == typeof(bool);

    /// <summary>
    /// Gets a value which indicates what kind of argument this instance represents.
    /// </summary>
    /// <value>
    /// One of the values of the <see cref="ArgumentKind"/> enumeration.
    /// </value>
    /// <remarks>
    /// <para>
    ///   An argument that is <see cref="ArgumentKind.MultiValue"/> can accept multiple values
    ///   by being supplied more than once. An argument is multi-value if its <see cref="ArgumentType"/>
    ///   is an array or the argument was defined by a read-only property whose type implements
    ///   the <see cref="ICollection{T}"/> generic interface.
    /// </para>
    /// <para>
    ///   An argument is <see cref="ArgumentKind.Dictionary"/> dictionary argument is a
    ///   multi-value argument whose values are key/value pairs, which get added to a
    ///   dictionary based on the key. An argument is a dictionary argument when its
    ///   <see cref="ArgumentType"/> is <see cref="Dictionary{TKey,TValue}"/>, or it was defined
    ///   by a read-only property whose type implements the <see cref="IDictionary{TKey,TValue}"/>
    ///   property.
    /// </para>
    /// <para>
    ///   An argument is <see cref="ArgumentKind.Method"/> if it is backed by a method instead
    ///   of a property, which will be invoked when the argument is set. Method arguments
    ///   cannot be multi-value or dictionary arguments.
    /// </para>
    /// <para>
    ///   Otherwise, the value will be <see cref="ArgumentKind.SingleValue"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="MultiValueSeparator"/>
    public ArgumentKind Kind => _argumentKind;

    /// <summary>
    /// Gets a value indicating whether this argument is a multi-value argument.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if the <see cref="Kind"/> property is <see cref="ArgumentKind.MultiValue"/>
    ///   or <see cref="ArgumentKind.Dictionary"/>; otherwise, <see langword="false"/>.
    /// </value>
    /// <seealso cref="MultiValueSeparator"/>
    /// <seealso cref="Kind"/>
    public bool IsMultiValue => _argumentKind is ArgumentKind.MultiValue or ArgumentKind.Dictionary;

    /// <summary>
    /// Gets the separator for the values if this argument is a multi-value argument
    /// </summary>
    /// <value>
    /// The separator for multi-value arguments, or <see langword="null"/> if no separator is used.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If the <see cref="IsMultiValue"/> property is <see langword="false"/>, this property
    ///   is always <see langword="null"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="MultiValueSeparatorAttribute"/>
    public string? MultiValueSeparator
    {
        get { return _multiValueSeparator; }
    }

    /// <summary>
    /// Gets a value that indicates whether or not a multi-value argument can consume multiple
    /// following argument values.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if a multi-value argument can consume multiple following values;
    /// otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   A multi-value argument that allows white-space separators is able to consume multiple
    ///   values from the command line that follow it. All values that follow the name, up until
    ///   the next argument name, are considered values for this argument.
    /// </para>
    /// <para>
    ///   If the <see cref="IsMultiValue"/> property is <see langword="false"/>, this property
    ///   is always <see langword="false"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="MultiValueSeparatorAttribute"/>
    public bool AllowMultiValueWhiteSpaceSeparator => _allowMultiValueWhiteSpaceSeparator;

    /// <summary>
    /// Gets the separator for key/value pairs if this argument is a dictionary argument.
    /// </summary>
    /// <value>
    /// The custom value specified using the <see cref="KeyValueSeparatorAttribute"/> attribute, or <see cref="KeyValuePairConverter.DefaultSeparator"/>
    /// if no attribute was present, or <see langword="null" /> if this is not a dictionary argument.
    /// </value>
    /// <remarks>
    /// <para>
    ///   This property is only meaningful if the <see cref="Kind"/> property is <see cref="ArgumentKind.Dictionary"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="KeyValueSeparatorAttribute"/>
    public string? KeyValueSeparator => _keyValueSeparator;

    /// <summary>
    /// Gets a value indicating whether this argument is a dictionary argument.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if this the <see cref="Kind"/> property is <see cref="ArgumentKind.Dictionary"/>;
    ///   otherwise, <see langword="false"/>.
    /// </value>
    public bool IsDictionary => _argumentKind == ArgumentKind.Dictionary;

    /// <summary>
    /// Gets a value indicating whether this argument, if it is a dictionary argument, allows duplicate keys.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if this argument allows duplicate keys; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   This property is only meaningful if the <see cref="Kind"/> property is <see cref="ArgumentKind.Dictionary"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="AllowDuplicateDictionaryKeysAttribute"/>
    public bool AllowsDuplicateDictionaryKeys
    {
        get { return _allowDuplicateDictionaryKeys; }
    }

    /// <summary>
    /// Gets the value that the argument was set to in the last call to <see cref="CommandLineParser.Parse(string[],int)"/>.
    /// </summary>
    /// <value>
    ///   The value of the argument that was obtained when the command line arguments were parsed.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The <see cref="Value"/> property provides an alternative method for accessing supplied argument
    ///   values, in addition to using the object returned by <see cref="CommandLineParser.Parse(string[], int)"/>.
    /// </para>
    /// <para>
    ///   If an argument was supplied on the command line, the <see cref="Value"/> property will equal the
    ///   supplied value after conversion to the type specified by the <see cref="ArgumentType"/> property,
    ///   and the <see cref="HasValue"/> property will be <see langword="true"/>.
    /// </para>
    /// <para>
    ///   If an optional argument was not supplied, the <see cref="Value"/> property will equal
    ///   the <see cref="DefaultValue"/> property, and <see cref="HasValue"/> will be <see langword="false"/>.
    /// </para>
    /// <para>
    ///   If the <see cref="Kind"/> property is <see cref="ArgumentKind.MultiValue"/>, the <see cref="Value"/> property will
    ///   return an array with all the values, even if the argument type is a collection type rather than
    ///   an array.
    /// </para>
    /// <para>
    ///   If the <see cref="Kind"/> property is <see cref="ArgumentKind.Dictionary"/>, the <see cref="Value"/> property will
    ///   return a <see cref="Dictionary{TKey, TValue}"/> with all the values, even if the argument type is a different type.
    /// </para>
    /// </remarks>
    public object? Value => _valueHelper?.Value;

    /// <summary>
    /// Gets a value indicating whether the value of this argument was supplied on the command line in the last
    /// call to <see cref="CommandLineParser.Parse(string[],int)"/>.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if this argument's value was supplied on the command line when the arguments were parsed; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   Use this property to determine whether or not an argument was supplied on the command line, or was
    ///   assigned its default value.
    /// </para>
    /// <para>
    ///   When an optional argument is not supplied on the command line, the <see cref="Value"/> property will be equal
    ///   to the <see cref="DefaultValue"/> property, and <see cref="HasValue"/> will be <see langword="false"/>.
    /// </para>
    /// <para>
    ///   It is however possible for the user to supply a value on the command line that matches the default value.
    ///   In that case, although the <see cref="Value"/> property will still be equal to the <see cref="DefaultValue"/>
    ///   property, the <see cref="HasValue"/> property will be <see langword="true"/>. This allows you to distinguish
    ///   between an argument that was supplied or omitted even if the supplied value matches the default.
    /// </para>
    /// </remarks>
    public bool HasValue { get; private set; }

    /// <summary>
    /// Gets the name or alias that was used on the command line to specify this argument.
    /// </summary>
    /// <value>
    /// The name or alias that was used on the command line to specify this argument, or <see langword="null"/> if this argument was specified by position or not specified.
    /// </value>
    /// <remarks>
    /// <para>
    ///   This property can be the value of the <see cref="ArgumentName"/> property, the <see cref="ShortName"/> property,
    ///   or any of the values in the <see cref="Aliases"/> and <see cref="ShortAliases"/> properties.
    /// </para>
    /// <para>
    ///   If the argument names are case-insensitive, the value of this property uses the casing as specified on the command line, not the original casing of the argument name or alias.
    /// </para>
    /// </remarks>
    public string? UsedArgumentName => _usedArgumentName.Length > 0 ? _usedArgumentName.ToString() : null;

    /// <summary>
    /// Gets a value that indicates whether or not this argument accepts <see langword="null" /> values.
    /// </summary>
    /// <value>
    ///   <see langword="true" /> if the <see cref="ArgumentType"/> is a nullable reference type
    ///   or <see cref="Nullable{T}"/>; <see langword="false" /> if the argument is any other
    ///   value type or, for .Net 6.0 and later only, a non-nullable reference type.
    /// </value>
    /// <remarks>
    /// <para>
    ///   For a multi-value argument, this value indicates whether the element type can be
    ///   <see langword="null" />.
    /// </para>
    /// <para>
    ///   For a dictionary argument, this value indicates whether the type of the dictionary's values can be
    ///   <see langword="null" />. Dictionary key types are always non-nullable, as this is a constraint on
    ///   <see cref="Dictionary{TKey, TValue}"/>. This works only if the argument type is <see cref="Dictionary{TKey, TValue}"/>
    ///   or <see cref="IDictionary{TKey, TValue}"/>. For other types that implement <see cref="IDictionary{TKey, TValue}"/>,
    ///   it is not possible to determine the nullability of <c>TValue</c> except if it's
    ///   a value type.
    /// </para>
    /// <para>
    ///   This property indicates what happens when the <see cref="ArgumentConverter"/> used for this argument returns
    ///   <see langword="null" /> from its <see cref="ArgumentConverter.Convert(string, CultureInfo, CommandLineArgument)"/>
    ///   method.
    /// </para>
    /// <para>
    ///   If this property is <see langword="true" />, the argument's value will be set to <see langword="null" />.
    ///   If it's <see langword="false" />, a <see cref="CommandLineArgumentException"/> will be thrown during
    ///   parsing with <see cref="CommandLineArgumentErrorCategory.NullArgumentValue"/>.
    /// </para>
    /// <para>
    ///   If the project containing the command line argument type does not use nullable reference types, or does
    ///   not support them (e.g. on older .Net versions), this property will only be <see langword="false" /> for
    ///   value types other than <see cref="Nullable{T}"/>. Only on .Net 6.0 and later will the property be
    ///   <see langword="false"/> for non-nullable reference types. Although nullable reference types are available
    ///   on .Net Core 3.x, only .Net 6.0 and later will get this behavior due to the necessary runtime support to
    ///   determine nullability of a property or constructor argument.
    /// </para>
    /// </remarks>
    public bool AllowNull => _allowNull;

    /// <summary>
    /// Gets a value that indicates whether argument parsing should be canceled if this
    /// argument is encountered.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if argument parsing should be canceled after this argument;
    /// otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   This value is determined using the <see cref="CommandLineArgumentAttribute.CancelParsing"/>
    ///   property.
    /// </para>
    /// <para>
    ///   If this property is <see langword="true"/>, the <see cref="CommandLineParser"/> will
    ///   stop parsing the command line arguments after seeing this argument, and return
    ///   <see langword="null"/> from the <see cref="CommandLineParser.Parse(string[], int)"/> method
    ///   or one of its overloads. Since no instance of the arguments type is returned, it's
    ///   not possible to determine argument values, or which argument caused the cancellation,
    ///   except by inspecting the <see cref="CommandLineParser.Arguments"/> property.
    /// </para>
    /// <para>
    ///   This property is most commonly useful to implement a "-Help" or "-?" style switch
    ///   argument, where the presence of that argument causes usage help to be printed and
    ///   the program to exit, regardless of whether the rest of the command line is valid
    ///   or not.
    /// </para>
    /// <para>
    ///   The <see cref="CommandLineParser{T}.ParseWithErrorHandling()"/> method and the
    ///   <see cref="CommandLineParser.Parse{T}(string[], ParseOptions?)"/> static helper method
    ///   will print usage information if parsing was canceled through this method.
    /// </para>
    /// <para>
    ///   Canceling parsing in this way is identical to handling the <see cref="CommandLineParser.ArgumentParsed"/>
    ///   event and setting <see cref="System.ComponentModel.CancelEventArgs.Cancel"/> to
    ///   <see langword="true" />.
    /// </para>
    /// <para>
    ///   It's possible to prevent cancellation when an argument has this property set by
    ///   handling the <see cref="CommandLineParser.ArgumentParsed"/> event and setting the
    ///   <see cref="ArgumentParsedEventArgs.OverrideCancelParsing"/> property to 
    ///   <see langword="true"/>.
    /// </para>
    /// </remarks>
    public bool CancelParsing => _cancelParsing;

    /// <summary>
    /// Gets or sets a value that indicates whether the argument is hidden from the usage help.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the argument is hidden from the usage help; otherwise,
    /// <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   A hidden argument will not be included in the usage syntax or the argument description
    ///   list, even if <see cref="DescriptionListFilterMode.All"/> is used. It does not
    ///   affect whether the argument can be used.
    /// </para>
    /// <para>
    ///   This property is always <see langword="false"/> for positional or required arguments,
    ///   which may not be hidden.
    /// </para>
    /// </remarks>
    public bool IsHidden => _isHidden;

    /// <summary>
    /// Gets the argument validators applied to this argument.
    /// </summary>
    /// <value>
    /// A list of objects deriving from the <see cref="ArgumentValidationAttribute"/> class.
    /// </value>
    public IEnumerable<ArgumentValidationAttribute> Validators => _validators;

    /// <summary>
    /// When implemented in a derived class, gets a value that indicates whether this argument
    /// is backed by a property with a public set method.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this argument's value will be stored in a writable property;
    /// otherwise, <see langword="false"/>.
    /// </value>
    protected abstract bool CanSetProperty { get; }

    /// <summary>
    /// Converts the specified string to the <see cref="ElementType"/>.
    /// </summary>
    /// <param name="culture">The culture to use for conversion.</param>
    /// <param name="argumentValue">The string to convert.</param>
    /// <returns>The converted value.</returns>
    /// <remarks>
    /// <para>
    ///   Conversion is done by one of several methods. First, if a <see
    ///   cref="ArgumentConverterAttribute"/> was present on the property, or method that
    ///   defined the argument, the specified <see cref="ArgumentConverter"/> is used.
    ///   Otherwise, the type must implement <see cref="ISpanParsable{TSelf}"/>, or have a
    ///   static Parse(<see cref="string"/>, <see cref="IFormatProvider"/>) or Parse(<see
    ///   cref="string"/>) method, or have a constructor that takes a single parameter of type
    ///   <see cref="string"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="culture"/> is <see langword="null"/>
    /// </exception>
    /// <exception cref="CommandLineArgumentException">
    ///   <paramref name="argumentValue"/> could not be converted to the type specified in the
    ///   <see cref="ArgumentType"/> property.
    /// </exception>
    public object? ConvertToArgumentType(CultureInfo culture, string? argumentValue)
        => ConvertToArgumentType(culture, argumentValue != null, argumentValue, argumentValue.AsSpan());

    /// <summary>
    /// Converts any type to the argument's <see cref="ElementType"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="NotSupportedException">
    ///   The argument's <see cref="ArgumentConverter"/> cannot convert between the type of
    ///   <paramref name="value"/> and the <see cref="ArgumentType"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   If the type of <paramref name="value"/> is directly assignable to <see cref="ArgumentType"/>,
    ///   no conversion is done. If the <paramref name="value"/> is a <see cref="string"/>,
    ///   the same rules apply as for the <see cref="ConvertToArgumentType(CultureInfo, string?)"/>
    ///   method, using <see cref="CultureInfo.InvariantCulture"/>. Other types cannot be
    ///   converted.
    /// </para>
    /// <para>
    ///   This method is used to convert the <see cref="CommandLineArgumentAttribute.DefaultValue"/>
    ///   property to the correct type, and is also used by implementations of the 
    ///   <see cref="ArgumentValidationAttribute"/> class to convert values when needed.
    /// </para>
    /// </remarks>
    /// <exception cref="NotSupportedException">The conversion is not supported.</exception>
    public object? ConvertToArgumentTypeInvariant(object? value)
    {
        if (value == null || _elementTypeWithNullable.IsAssignableFrom(value.GetType()))
        {
            return value;
        }

        var stringValue = value.ToString();
        if (stringValue == null)
        {
            return null;
        }

        return _converter.Convert(stringValue, CultureInfo.InvariantCulture, this);
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="CommandLineArgument"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="CommandLineArgument"/>.</returns>
    /// <remarks>
    /// <para>
    ///   The string value matches the way the argument is displayed in the usage help's command line syntax
    ///   when using the default <see cref="UsageWriter"/>.
    /// </para>
    /// </remarks>
    public override string ToString()
    {
        return (new UsageWriter()).GetArgumentUsage(this);
    }

    /// <summary>
    /// When implemented in a derived class, sets the property for this argument.
    /// </summary>
    /// <param name="target">An instance of the type that defined the argument.</param>
    /// <param name="value">The value of the argument.</param>
    /// <exception cref="InvalidOperationException">
    ///   This argument does not use a writable property.
    /// </exception>
    protected abstract void SetProperty(object target, object? value);

    /// <summary>
    /// When implemented in a derived class, gets the value of the property for this argument.
    /// </summary>
    /// <param name="target">An instance of the type that defined the argument.</param>
    /// <returns>The value of the property</returns>
    /// <exception cref="InvalidOperationException">
    ///   This argument does not use a property.
    /// </exception>
    protected abstract object? GetProperty(object target);

    /// <summary>
    /// When implemented in a derived class, calls the method that defined the property.
    /// </summary>
    /// <param name="value">The argument value.</param>
    /// <returns>The return value of the argument's method.</returns>
    /// <exception cref="InvalidOperationException">
    ///   This argument does not use a method.
    /// </exception>
    protected abstract bool CallMethod(object? value);

    /// <summary>
    /// Determines the value description if one wasn't explicitly given.
    /// </summary>
    /// <param name="type">
    /// The type to get the description for, or null to use the value of the <see cref="ElementType"/>
    /// property.
    /// </param>
    /// <returns>The value description.</returns>
    /// <remarks>
    /// <para>
    ///   This method is responsible for applying the <see cref="ParseOptions.ValueDescriptionTransform"/>,
    ///   if one is specified.
    /// </para>
    /// </remarks>
    protected virtual string DetermineValueDescription(Type? type = null)
    {
        if (Kind == ArgumentKind.Dictionary && type == null)
        {
            var key = DetermineValueDescription(_keyType!.GetUnderlyingType());
            var value = DetermineValueDescription(_valueType!.GetUnderlyingType());
            return $"{key}{KeyValueSeparator}{value}";
        }

        var result = GetDefaultValueDescription(type);
        if (result != null)
        {
            return result;
        }

        var typeName = GetFriendlyTypeName(type ?? ElementType);
        return Parser.Options.ValueDescriptionTransform?.Apply(typeName) ?? typeName;
    }

    internal static ArgumentInfo CreateArgumentInfo(CommandLineParser parser, Type argumentType, bool allowsNull,
        string memberName, CommandLineArgumentAttribute attribute,
        MultiValueSeparatorAttribute? multiValueSeparatorAttribute, DescriptionAttribute? descriptionAttribute,
        bool allowDuplicateDictionaryKeys, KeyValueSeparatorAttribute? keyValueSeparatorAttribute,
        IEnumerable<AliasAttribute>? aliasAttributes, IEnumerable<ShortAliasAttribute>? shortAliasAttributes,
        IEnumerable<ArgumentValidationAttribute>? validationAttributes)
    {
        var argumentName = DetermineArgumentName(attribute.ArgumentName, memberName, parser.Options.ArgumentNameTransform);
        return new ArgumentInfo()
        {
            Parser = parser,
            ArgumentName = argumentName,
            Long = attribute.IsLong,
            Short = attribute.IsShort,
            ShortName = attribute.ShortName,
            ArgumentType = argumentType,
            ElementTypeWithNullable = argumentType,
            Description = descriptionAttribute?.Description,
            ValueDescription = attribute.ValueDescription,
            Position = attribute.Position < 0 ? null : attribute.Position,
            AllowDuplicateDictionaryKeys = allowDuplicateDictionaryKeys,
            MultiValueSeparator = GetMultiValueSeparator(multiValueSeparatorAttribute),
            AllowMultiValueWhiteSpaceSeparator = multiValueSeparatorAttribute != null && multiValueSeparatorAttribute.Separator == null,
            KeyValueSeparator = keyValueSeparatorAttribute?.Separator,
            Aliases = GetAliases(aliasAttributes, argumentName),
            ShortAliases = GetShortAliases(shortAliasAttributes, argumentName),
            DefaultValue = attribute.DefaultValue,
            IsRequired = attribute.IsRequired,
            MemberName = memberName,
            AllowNull = allowsNull,
            CancelParsing = attribute.CancelParsing,
            IsHidden = attribute.IsHidden,
            Validators = validationAttributes ?? Enumerable.Empty<ArgumentValidationAttribute>(),
        };
    }

    private static string GetFriendlyTypeName(Type type)
    {
        // This is used to generate a value description from a type name if no custom value description was supplied.
        if (type.IsGenericType)
        {
            var name = new StringBuilder(type.FullName?.Length ?? 0);
            name.Append(type.Name, 0, type.Name.IndexOf("`", StringComparison.Ordinal));
            name.Append('<');
            // AppendJoin is not supported in .Net Standard 2.0
            bool first = true;
            foreach (Type typeArgument in type.GetGenericArguments())
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    name.Append(", ");
                }

                name.Append(GetFriendlyTypeName(typeArgument));
            }

            name.Append('>');
            return name.ToString();
        }
        else
        {
            return type.Name;
        }
    }

    internal object? ConvertToArgumentType(CultureInfo culture, bool hasValue, string? stringValue, ReadOnlySpan<char> spanValue)
    {
        if (culture == null)
        {
            throw new ArgumentNullException(nameof(culture));
        }

        if (!hasValue)
        {
            if (IsSwitch)
            {
                return true;
            }
            else
            {
                throw _parser.StringProvider.CreateException(CommandLineArgumentErrorCategory.MissingNamedArgumentValue, this);
            }
        }

        try
        {
            var converted = stringValue == null
                ? _converter.Convert(spanValue, culture, this)
                : _converter.Convert(stringValue, culture, this);

            if (converted == null && (!_allowNull || IsDictionary))
            {
                throw _parser.StringProvider.CreateException(CommandLineArgumentErrorCategory.NullArgumentValue, this);
            }

            return converted;
        }
        catch (NotSupportedException ex)
        {
            throw _parser.StringProvider.CreateException(CommandLineArgumentErrorCategory.ArgumentValueConversion, ex, this, stringValue ?? spanValue.ToString());
        }
        catch (FormatException ex)
        {
            throw _parser.StringProvider.CreateException(CommandLineArgumentErrorCategory.ArgumentValueConversion, ex, this, stringValue ?? spanValue.ToString());
        }
    }

    internal bool HasInformation(UsageWriter writer)
    {
        if (!string.IsNullOrEmpty(Description))
        {
            return true;
        }

        if (writer.UseAbbreviatedSyntax && Position == null)
        {
            return true;
        }

        if (writer.UseShortNamesForSyntax)
        {
            if (HasLongName)
            {
                return true;
            }
        }
        else if (HasShortName)
        {
            return true;
        }

        if (writer.IncludeAliasInDescription &&
            ((Aliases != null && Aliases.Count > 0) || (ShortAliases != null && ShortAliases.Count > 0)))
        {
            return true;
        }

        if (writer.IncludeDefaultValueInDescription && DefaultValue != null)
        {
            return true;
        }

        if (writer.IncludeValidatorsInDescription &&
            _validators.Any(v => !string.IsNullOrEmpty(v.GetUsageHelp(this))))
        {
            return true;
        }

        return false;
    }

    internal bool SetValue(CultureInfo culture, bool hasValue, string? stringValue, ReadOnlySpan<char> spanValue)
    {
        _valueHelper ??= CreateValueHelper();

        bool continueParsing;
        if (IsMultiValue && hasValue && MultiValueSeparator != null)
        {
            continueParsing = true;
            spanValue.Split(MultiValueSeparator.AsSpan(), separateValue =>
            {
                string? separateValueString = null;
                PreValidate(ref separateValueString, separateValue);
                var converted = ConvertToArgumentType(culture, true, separateValueString, separateValue);
                continueParsing = _valueHelper.SetValue(this, converted);
                if (!continueParsing)
                {
                    return false;
                }

                Validate(converted, ValidationMode.AfterConversion);
                return true;
            });
        }
        else
        {
            PreValidate(ref stringValue, spanValue);
            var converted = ConvertToArgumentType(culture, hasValue, stringValue, spanValue);
            continueParsing = _valueHelper.SetValue(this, converted);
            Validate(converted, ValidationMode.AfterConversion);
        }

        HasValue = true;
        return continueParsing;
    }

    internal static (CommandLineArgument, bool) CreateAutomaticHelp(CommandLineParser parser)
    {
        if (parser == null)
        {
            throw new ArgumentNullException(nameof(parser));
        }

        var argumentName = DetermineArgumentName(null, parser.StringProvider.AutomaticHelpName(), parser.Options.ArgumentNameTransform);
        var shortName = parser.StringProvider.AutomaticHelpShortName();
        var shortAlias = char.ToLowerInvariant(argumentName[0]);
        var existingArg = parser.GetArgument(argumentName) ??
            (parser.Mode == ParsingMode.LongShort
                ? (parser.GetShortArgument(shortName) ?? parser.GetShortArgument(shortAlias))
                : (parser.GetArgument(shortName.ToString()) ?? parser.GetArgument(shortAlias.ToString())));

        if (existingArg != null)
        {
            return (existingArg, false);
        }

        return (new HelpArgument(parser, argumentName, shortName, shortAlias), true);
    }

    internal static CommandLineArgument? CreateAutomaticVersion(CommandLineParser parser)
    {
        if (parser == null)
        {
            throw new ArgumentNullException(nameof(parser));
        }

        var argumentName = DetermineArgumentName(null, parser.StringProvider.AutomaticVersionName(), parser.Options.ArgumentNameTransform);
        if (parser.GetArgument(argumentName) != null)
        {
            return null;
        }

        return new VersionArgument(parser, argumentName);
    }

    internal object? GetConstructorParameterValue()
    {
        return Value;
    }

    internal void ApplyPropertyValue(object target)
    {
        // Do nothing for method-based values.
        // TODO: Handle new style constructor parameters.
        if (Kind == ArgumentKind.Method)
        {
            return;
        }

        try
        {
            _valueHelper?.ApplyValue(this, target);
        }
        catch (TargetInvocationException ex)
        {
            throw _parser.StringProvider.CreateException(CommandLineArgumentErrorCategory.ApplyValueError, ex.InnerException, this);
        }
        catch (Exception ex)
        {
            throw _parser.StringProvider.CreateException(CommandLineArgumentErrorCategory.ApplyValueError, ex, this);
        }
    }

    internal void Reset()
    {
        if (!IsMultiValue && _defaultValue != null)
        {
            _valueHelper = new SingleValueHelper(_defaultValue);
        }
        else
        {
            _valueHelper = null;
        }

        HasValue = false;
        _usedArgumentName = default;
    }

    internal static void ShowVersion(LocalizedStringProvider stringProvider, Assembly assembly, string friendlyName)
    {
        Console.WriteLine(stringProvider.ApplicationNameAndVersion(assembly, friendlyName));
        var copyright = stringProvider.ApplicationCopyright(assembly);
        if (copyright != null)
        {
            Console.WriteLine(copyright);
        }
    }

    internal void ValidateAfterParsing()
    {
        if (HasValue)
        {
            Validate(null, ValidationMode.AfterParsing);
        }
        else if (IsRequired)
        {
            throw _parser.StringProvider.CreateException(CommandLineArgumentErrorCategory.MissingRequiredArgument, ArgumentName);
        }
    }

    internal void SetUsedArgumentName(ReadOnlyMemory<char> name)
    {
        _usedArgumentName = name;
    }

    private IValueHelper CreateValueHelper()
    {
        Debug.Assert(_valueHelper == null);
        Type type;
        switch (_argumentKind)
        {
        case ArgumentKind.Dictionary:
            type = typeof(DictionaryValueHelper<,>).MakeGenericType(_elementType.GetGenericArguments());
            return (IValueHelper)Activator.CreateInstance(type, _allowDuplicateDictionaryKeys, _allowNull)!;

        case ArgumentKind.MultiValue:
            type = typeof(MultiValueHelper<>).MakeGenericType(_elementTypeWithNullable);
            return (IValueHelper)Activator.CreateInstance(type)!;

        case ArgumentKind.Method:
            return new MethodValueHelper();

        default:
            Debug.Assert(_defaultValue == null);
            return new SingleValueHelper(null);
        }
    }

    internal static IEnumerable<string>? GetAliases(IEnumerable<AliasAttribute>? aliasAttributes, string argumentName)
    {
        if (aliasAttributes == null || !aliasAttributes.Any())
        {
            return null;
        }

        return aliasAttributes.Select(alias =>
        {
            if (string.IsNullOrEmpty(alias.Alias))
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.EmptyAliasFormat, argumentName));
            }

            return alias.Alias;
        });
    }

    internal static IEnumerable<char>? GetShortAliases(IEnumerable<ShortAliasAttribute>? aliasAttributes, string argumentName)
    {
        if (aliasAttributes == null || !aliasAttributes.Any())
        {
            return null;
        }

        return aliasAttributes.Select(alias =>
        {
            if (alias.Alias == '\0')
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.EmptyAliasFormat, argumentName));
            }

            return alias.Alias;
        });
    }

    private static bool AutomaticVersion(CommandLineParser parser)
    {
        ShowVersion(parser.StringProvider, parser.ArgumentsType.Assembly, parser.ApplicationFriendlyName);

        // Cancel parsing but do not show help.
        return false;
    }

    internal static string DetermineArgumentName(string? explicitName, string memberName, NameTransform? transform)
    {
        if (explicitName != null)
        {
            return explicitName;
        }

        return transform?.Apply(memberName) ?? memberName;
    }

    private string? GetDefaultValueDescription(Type? type)
    {
        if (Parser.Options.DefaultValueDescriptions == null || 
            !Parser.Options.DefaultValueDescriptions.TryGetValue(type ?? ElementType, out string? value))
        {
            return null;
        }

        return value;
    }

    private void Validate(object? value, ValidationMode mode)
    {
        foreach (var validator in _validators)
        {
            if (validator.Mode == mode)
            {
                validator.Validate(this, value);
            }
        }
    }

    private void PreValidate(ref string? stringValue, ReadOnlySpan<char> spanValue)
    {
        foreach (var validator in _validators)
        {
            if (validator.Mode == ValidationMode.BeforeConversion)
            {
                if (stringValue == null)
                {
                    if (validator.CanValidateSpan)
                    {
                        validator.ValidateSpan(this, spanValue);
                        continue;
                    }
                    else
                    {
                        stringValue = spanValue.ToString();
                    }
                }

                validator.Validate(this, stringValue);
            }
        }
    }
    private static string? GetMultiValueSeparator(MultiValueSeparatorAttribute? attribute)
    {
        var separator = attribute?.Separator;
        if (string.IsNullOrEmpty(separator))
        {
            return null;
        }
        else
        {
            return separator;
        }
    }
}
