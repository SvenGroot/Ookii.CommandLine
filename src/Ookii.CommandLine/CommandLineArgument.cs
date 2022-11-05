// Copyright (c) Sven Groot (Ookii.org)
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

namespace Ookii.CommandLine
{
    /// <summary>
    /// Provides information about command line arguments that are recognized by a <see cref="CommandLineParser"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public sealed class CommandLineArgument
    {
        #region Nested types

        private interface IValueHelper
        {
            object? Value { get; }
            bool SetValue(CommandLineArgument argument, CultureInfo culture, object? value);
            void ApplyValue(object target, PropertyInfo property);
        }

        private class SingleValueHelper : IValueHelper
        {
            public SingleValueHelper(object? initialValue)
            {
                Value = initialValue;
            }

            public object? Value { get; private set; }

            public void ApplyValue(object target, PropertyInfo property)
            {
                property.SetValue(target, Value);
            }

            public bool SetValue(CommandLineArgument argument, CultureInfo culture, object? value)
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

            public void ApplyValue(object target, PropertyInfo property)
            {
                if (property.PropertyType.IsArray)
                {
                    property.SetValue(target, Value);
                    return;
                }

                object? collection = property.GetValue(target, null);
                if (collection == null)
                {
                    throw new InvalidOperationException();
                }

                var list = (ICollection<T?>)collection;
                list.Clear();
                foreach (var value in _values)
                {
                    list.Add(value);
                }
            }

            public bool SetValue(CommandLineArgument argument, CultureInfo culture, object? value)
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

            public void ApplyValue(object target, PropertyInfo property)
            {
                if (property.GetSetMethod() != null)
                {
                    property.SetValue(target, _dictionary);
                    return;
                }

                var dictionary = (IDictionary<TKey, TValue?>?)property.GetValue(target, null);
                if (dictionary == null)
                {
                    throw new InvalidOperationException();
                }

                dictionary.Clear();
                foreach (var pair in _dictionary)
                {
                    dictionary.Add(pair.Key, pair.Value);
                }
            }

            public bool SetValue(CommandLineArgument argument, CultureInfo culture, object? value)
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

            public void ApplyValue(object target, PropertyInfo property)
            {
                throw new InvalidOperationException();
            }

            public bool SetValue(CommandLineArgument argument, CultureInfo culture, object? value)
            {
                Value = value;
                var info = argument._method!.Value;
                int parameterCount = (info.HasValueParameter ? 1 : 0) + (info.HasParserParameter ? 1 : 0);
                var parameters = new object?[parameterCount];
                int index = 0;
                if (info.HasValueParameter)
                {
                    parameters[index] = Value;
                    ++index;
                }

                if (info.HasParserParameter)
                {
                    parameters[index] = argument._parser;
                }

                var returnValue = info.Method.Invoke(null, parameters);
                if (returnValue == null)
                {
                    return true;
                }
                else
                {
                    return (bool)returnValue;
                }
            }
        }

        private struct ArgumentInfo
        {
            public CommandLineParser Parser { get; set; }
            public PropertyInfo? Property { get; set; }
            public MethodArgumentInfo? Method { get; set; }
            public ParameterInfo? Parameter { get; set; }
            public string MemberName { get; set; }
            public string ArgumentName { get; set; }
            public bool Long { get; set; }
            public bool Short { get; set; }
            public char ShortName { get; set; }
            public IEnumerable<string>? Aliases { get; set; }
            public IEnumerable<char>? ShortAliases { get; set; }
            public Type ArgumentType { get; set; }
            public Type? ConverterType { get; set; }
            public Type? KeyConverterType { get; set; }
            public Type? ValueConverterType { get; set; }
            public int? Position { get; set; }
            public bool IsRequired { get; set; }
            public object? DefaultValue { get; set; }
            public string? Description { get; set; }
            public string? ValueDescription { get; set; }
            public string? MultiValueSeparator { get; set; }
            public string? KeyValueSeparator { get; set; }
            public bool AllowDuplicateDictionaryKeys { get; set; }
            public bool AllowNull { get; set; }
            public bool CancelParsing { get; set; }
            public bool IsHidden { get; set; }
            public IEnumerable<ArgumentValidationAttribute> Validators { get; set; }
        }

        private struct MethodArgumentInfo
        {
            public MethodInfo Method { get; set; }
            public bool HasValueParameter { get; set; }
            public bool HasParserParameter { get; set; }
        }

        #endregion

        private readonly CommandLineParser _parser;
        private readonly TypeConverter _converter;
        private readonly PropertyInfo? _property;
        private readonly MethodArgumentInfo? _method;
        private readonly string _valueDescription;
        private readonly string _argumentName;
        private readonly bool _hasLongName = true;
        private readonly char _shortName;
        private readonly ReadOnlyCollection<string>? _aliases;
        private readonly ReadOnlyCollection<char>? _shortAliases;
        private readonly Type _argumentType;
        private readonly Type _elementType;
        private readonly string? _description;
        private readonly bool _isRequired;
        private readonly string _memberName;
        private readonly object? _defaultValue;
        private readonly ArgumentKind _argumentKind;
        private readonly bool _allowDuplicateDictionaryKeys;
        private readonly string? _multiValueSeparator;
        private readonly string? _keyValueSeparator;
        private readonly bool _allowNull;
        private readonly bool _cancelParsing;
        private readonly bool _isHidden;
        private readonly IEnumerable<ArgumentValidationAttribute> _validators;
        private IValueHelper? _valueHelper;

        private CommandLineArgument(ArgumentInfo info, IDictionary<Type, string>? defaultValueDescriptions)
        {
            // If this method throws anything other than a NotSupportedException, it constitutes a bug in the Ookii.CommandLine library.
            _parser = info.Parser;
            _property = info.Property;
            _method = info.Method;
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
            _elementType = info.ArgumentType;
            _description = info.Description;
            _isRequired = info.IsRequired;
            _multiValueSeparator = info.MultiValueSeparator;
            _allowNull = info.AllowNull;
            _cancelParsing = info.CancelParsing;
            _validators = info.Validators;
            // Required or positional arguments cannot be hidden.
            _isHidden = info.IsHidden && !info.IsRequired && info.Position == null;
            Position = info.Position;
            var converterType = info.ConverterType;

            if (_method == null)
            {
                var (collectionType, dictionaryType, elementType) = DetermineMultiValueType();

                if (dictionaryType != null)
                {
                    Debug.Assert(elementType != null);
                    _argumentKind = ArgumentKind.Dictionary;
                    _elementType = elementType!;
                    _allowDuplicateDictionaryKeys = info.AllowDuplicateDictionaryKeys;
                    _allowNull = DetermineDictionaryValueTypeAllowsNull(dictionaryType, info.Property, info.Parameter);
                    _keyValueSeparator = info.KeyValueSeparator ?? KeyValuePairConverter.DefaultSeparator;
                    var genericArguments = dictionaryType.GetGenericArguments();
                    if (converterType == null)
                    {
                        converterType = typeof(KeyValuePairConverter<,>).MakeGenericType(genericArguments);
                        _converter = (TypeConverter)Activator.CreateInstance(converterType, _parser.StringProvider, _argumentName, _allowNull, info.KeyConverterType, info.ValueConverterType, _keyValueSeparator)!;
                    }

                    var valueDescription = info.ValueDescription ?? GetDefaultValueDescription(elementType!, defaultValueDescriptions);
                    if (valueDescription == null)
                    {
                        var key = DetermineValueDescription(genericArguments[0], defaultValueDescriptions);
                        var value = DetermineValueDescription(genericArguments[1], defaultValueDescriptions);
                        valueDescription = $"{key}{_keyValueSeparator}{value}";
                    }

                    _valueDescription = valueDescription;
                }
                else if (collectionType != null)
                {
                    Debug.Assert(elementType != null);
                    _argumentKind = ArgumentKind.MultiValue;
                    _elementType = elementType!;
                    _allowNull = DetermineCollectionElementTypeAllowsNull(collectionType, info.Property, info.Parameter);
                }
            }
            else
            {
                _argumentKind = ArgumentKind.Method;
            }

            if (_valueDescription == null)
            {
                _valueDescription = info.ValueDescription ?? DetermineValueDescription(_elementType, defaultValueDescriptions);
            }

            if (_converter == null)
            {
                _converter = CreateConverter(converterType);
            }

            _defaultValue = ConvertToArgumentTypeInvariant(info.DefaultValue);
        }

        /// <summary>
        /// Gets the <see cref="CommandLineParser"/> that this argument belongs to.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="CommandLineParser"/> class.
        /// </value>
        public CommandLineParser Parser => _parser;

        /// <summary>
        /// Gets the name of the property or constructor parameter that defined this command line argument.
        /// </summary>
        /// <value>
        /// The name of the property or constructor parameter that defined this command line argument.
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
        ///   generated by <see cref="CommandLineParser.WriteUsage(System.IO.TextWriter,int,WriteUsageOptions)"/>.
        /// </para>
        /// <para>
        ///   If the <see cref="CommandLineParser.Mode"/> property is <see cref="ParsingMode.LongShort"/>,
        ///   and the <see cref="HasLongName"/> property is <see langword="false"/>, this returns
        ///   the long name of the argument.
        /// </para>
        /// </remarks>
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
        /// property is not <see cref="ParsingMode.LongShort"/> or the <see cref="HasLongName"/>
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
        ///   Otherwise, this property always returns <see langword="false"/>.
        /// </para>
        /// </remarks>
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
        ///   this property is not used and always returns <see langword="true"/>.
        /// </para>
        /// </remarks>
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
        ///   will always return an empty collection .
        /// </para>
        /// </remarks>
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
        ///   will always return an empty collection .
        /// </para>
        /// </remarks>
        public ReadOnlyCollection<char>? ShortAliases => _shortAliases;

        /// <summary>
        /// Gets the type of the argument.
        /// </summary>
        /// <value>
        /// The <see cref="Type"/> of the argument.
        /// </value>
        public Type ArgumentType
        {
            get { return _argumentType; }
        }

        /// <summary>
        /// Gets the element type of the argument.
        /// </summary>
        /// <value>
        /// If the <see cref="IsMultiValue"/> property is <see langword="true"/>, the <see cref="Type"/> of each individual value; otherwise, the same value as <see cref="ArgumentType"/>.
        /// </value>
        public Type ElementType
        {
            get { return _elementType; }
        }

        /// <summary>
        /// Gets the position of this argument.
        /// </summary>
        /// <value>
        /// The position of this argument, or <see langword="null"/> if this is not a positional argument.
        /// </value>
        /// <remarks>
        /// <para>
        ///   A positional argument is created either using a constructor parameter on the command line arguments type,
        ///   or by using the <see cref="CommandLineArgumentAttribute.Position"/> property to create a named
        ///   positional argument.
        /// </para>
        /// <para>
        ///   The <see cref="Position"/> property reflects the actual position of the positional argument. For positional
        ///   arguments created from properties this doesn't need to match the value of the <see cref="CommandLineArgumentAttribute.Position"/> property.
        /// </para>
        /// </remarks>
        public int? Position { get; internal set; }

        /// <summary>
        /// Gets a value that indicates whether the argument is required.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the argument's value must be specified on the command line; <see langword="false"/> if the argument may be omitted.
        /// </value>
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
        ///   This property is used only when generating usage information using <see cref="CommandLineParser.WriteUsage(System.IO.TextWriter,int,WriteUsageOptions)"/>.
        /// </para>
        /// <para>
        ///   To set the description of an argument, apply the <see cref="System.ComponentModel.DescriptionAttribute"/> attribute to the constructor parameter 
        ///   or the property that defines the argument.
        /// </para>
        /// </remarks>
        public string Description
        {
            get { return _description ?? string.Empty; }
        }

        /// <summary>
        /// Gets the description of the property's value to use when printing usage information.
        /// </summary>
        /// <value>
        /// The description of the value.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The value description is a short (typically one word) description that indicates the type of value that
        ///   the user should supply. By default the type of the property is used. If the type is an array type, the
        ///   array's element type is used. If the type is a nullable type, its underlying type is used.
        /// </para>
        /// <para>
        ///   The value description is used when printing usage. For example, the usage for an argument named Sample with
        ///   a value description of String would look like "-Sample &lt;String&gt;".
        /// </para>
        /// <note>
        ///   This is not the long description used to describe the purpose of the argument. That can be retrieved
        ///   using the <see cref="Description"/> property.
        /// </note>
        /// </remarks>
        public string ValueDescription
        {
            get { return _valueDescription; }
        }

        /// <summary>
        /// Gets a value indicating whether this argument is a switch argument.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if the argument is a switch argument; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   A switch argument is an argument that doesn't need a value; instead, its value is <see langword="true"/> or
        ///   <see langword="false"/> depending on whether the argument is present on the command line.
        /// </para>
        /// <para>
        ///   A argument is a switch argument when it is not positional, and its <see cref="CommandLineArgument.ElementType"/> is either <see cref="Boolean"/> or a nullable <see cref="Boolean"/>.
        /// </para>
        /// </remarks>
        public bool IsSwitch
        {
            get { return Position == null && (ElementType == typeof(bool) || ElementType == typeof(bool?)); }
        }

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
        ///   of a property, which will be invoked when the argument is set.
        /// </para>
        /// <para>
        ///   Otherwise, the value will be <see cref="ArgumentKind.SingleValue"/>.
        /// </para>
        /// </remarks>
        public ArgumentKind Kind => _argumentKind;

        /// <summary>
        /// Gets a value indicating whether this argument is a multi-value argument.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if the argument is a multi-value argument; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   A multi-value argument can accept multiple values by having the argument supplied more than once.
        /// </para>
        /// <para>
        ///   An argument is a multi-value argument if its <see cref="ArgumentType"/> is an array or the argument was defined by a read-only property whose type
        ///   implements the <see cref="ICollection{T}"/> generic interface, or when the <see cref="IsDictionary"/> property is <see langword="true"/>.
        /// </para>
        /// </remarks>
        public bool IsMultiValue => _argumentKind == ArgumentKind.MultiValue || _argumentKind == ArgumentKind.Dictionary;

        /// <summary>
        /// Gets the separator for the values if this argument is a multi-value argument
        /// </summary>
        /// <value>
        /// The separator for multi-value arguments, or <see langword="null"/> if no separator is used.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This property is only meaningful if the <see cref="IsMultiValue"/> property is <see langword="true"/>.
        /// </para>
        /// </remarks>
        public string? MultiValueSeparator
        {
            get { return _multiValueSeparator; }
        }

        /// <summary>
        /// Gets the separator for key/value pairs if this argument is a dictionary argument.
        /// </summary>
        /// <value>
        /// The custom value specified using the <see cref="KeyValueSeparatorAttribute"/> attribute, or <see cref="KeyValuePairConverter.DefaultSeparator"/>
        /// if no attribute was present, or <see langword="null" /> if this is not a dictionary argument.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This property is only meaningful if the <see cref="IsDictionary"/> property is <see langword="true"/>.
        /// </para>
        /// </remarks>
        public string? KeyValueSeparator => _keyValueSeparator;

        /// <summary>
        /// Gets a value indicating whether this argument is a dictionary argument.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this argument is a dictionary argument; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   A dictionary argument is an argument whose values have the form "key=value", which get added to a dictionary based on the key.
        /// </para>
        /// <para>
        ///   An argument is a dictionary argument when its <see cref="ArgumentType"/> is <see cref="Dictionary{TKey,TValue}"/>, or it was defined by
        ///   a read-only property whose type implements the <see cref="IDictionary{TKey,TValue}"/> property.
        /// </para>
        /// </remarks>
        public bool IsDictionary => _argumentKind == ArgumentKind.Dictionary;

        /// <summary>
        /// Gets a value indicating whether this argument, if it is a dictionary argument, allows duplicate keys.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this argument allows duplicate keys; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This property is only meaningful if the <see cref="IsDictionary"/> property is <see langword="true"/>.
        /// </para>
        /// </remarks>
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
        ///   If the <see cref="IsMultiValue"/> property is <see langword="true"/>, the <see cref="Value"/> property will
        ///   return an array with all the values, even if the argument type is a collection type rather than
        ///   an array.
        /// </para>
        /// <para>
        ///   If the <see cref="IsDictionary"/> property is <see langword="true"/>, the <see cref="Value"/> property will
        ///   return a <see cref="Dictionary{TKey, TValue}"/> with all the values, even if the argument type is a different type.
        /// </para>
        /// </remarks>
        public object? Value => _valueHelper?.Value;

        /// <summary>
        /// Gets a value indicating whether the value of this argument was supplied on the command line in the last
        /// call to <see cref="CommandLineParser.Parse(string[],int)"/>.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this argument's value was supplied on the command line when the arguments were parsed; otherwise, <see langword="false"/>.
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
        ///   If the argument names are case-insensitive, the value of this property uses the casing as specified on the command line, not the original casing of the argument name or alias.
        /// </para>
        /// </remarks>
        public string? UsedArgumentName { get; internal set; }

        /// <summary>
        /// Gets a value that indicates whether or not this argument accepts <see langword="null" /> values.
        /// </summary>
        /// <value>
        ///   <see langword="true" /> if the <see cref="ArgumentType"/> is a nullable reference type; <see langword="false" />
        ///   if the argument is a value type (except for <see cref="Nullable{T}"/> or (.Net 6.0 and later only) a 
        ///   non-nullable reference type.
        /// </value>
        /// <remarks>
        /// <para>
        ///   For a multi-value argument (array or collection), this value indicates whether the element type can be
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
        ///   This property indicates what happens when the <see cref="TypeConverter"/> used for this argument returns
        ///   <see langword="null" /> from its <see cref="TypeConverter.ConvertFrom(ITypeDescriptorContext?, CultureInfo?, object)"/>
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
        ///   value types (other than <see cref="Nullable{T}"/>. Only on .Net 6.0 and later will the property be
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
        /// otherwise, <see langword="false"/>. This value is determined using the <see cref="CommandLineArgumentAttribute.CancelParsing"/>
        /// property.
        /// </value>
        /// <remarks>
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
        ///   The <see cref="CommandLineParser.Parse{T}(string[], ParseOptions?)"/> static helper method will print
        ///   usage information if parsing was canceled through this method.
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
        ///   list, even if <see cref="DescriptionListFilterMode.All"/> is used.
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
        /// Converts the specified string to the argument type, as specified in the <see cref="ArgumentType"/> property.
        /// </summary>
        /// <param name="culture">The culture to use to convert the argument.</param>
        /// <param name="argumentValue">The string to convert.</param>
        /// <returns>The argument, converted to the type specified by the <see cref="ArgumentType"/> property.</returns>
        /// <remarks>
        /// <para>
        ///   The <see cref="TypeConverter"/> for the type specified by <see cref="ArgumentType"/> is used to do the conversion.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="culture"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="CommandLineArgumentException">
        ///   <paramref name="argumentValue"/> could not be converted to the type specified in the <see cref="ArgumentType"/> property.
        /// </exception>
        public object? ConvertToArgumentType(CultureInfo culture, string? argumentValue)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            if (argumentValue == null)
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
                var converted = _converter.ConvertFrom(null, culture, argumentValue);
                if (converted == null && (!_allowNull || IsDictionary))
                {
                    throw _parser.StringProvider.CreateException(CommandLineArgumentErrorCategory.NullArgumentValue, this);
                }

                return converted;
            }
            catch (NotSupportedException ex)
            {
                throw _parser.StringProvider.CreateException(CommandLineArgumentErrorCategory.ArgumentValueConversion, ex, this, argumentValue);
            }
            catch (FormatException ex)
            {
                throw _parser.StringProvider.CreateException(CommandLineArgumentErrorCategory.ArgumentValueConversion, ex, this, argumentValue);
            }
            catch (Exception ex)
            {
                // Yeah, I don't like catching Exception, but unfortunately BaseNumberConverter (e.g. used for int) can *throw* a System.Exception (not a derived class) so there's nothing I can do about it.
                if (ex.InnerException is FormatException)
                {
                    throw _parser.StringProvider.CreateException(CommandLineArgumentErrorCategory.ArgumentValueConversion, ex, this, argumentValue);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Converts any type to the argument value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        /// <exception cref="NotSupportedException">
        ///   The argument's <see cref="TypeConverter"/> cannot convert between the type of
        ///   <paramref name="value"/> and the <see cref="ArgumentType"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        ///   If the type of <paramref name="value"/> is directly assignable to <see cref="ArgumentType"/>,
        ///   no conversion is done. Otherwise, the <see cref="TypeConverter"/> for the argument
        ///   is used.
        /// </para>
        /// <para>
        ///   This method is used to convert the <see cref="CommandLineArgumentAttribute.DefaultValue"/>
        ///   property to the correct type, and is also used by implementations of the 
        ///   <see cref="ArgumentValidationAttribute"/> class to convert values when needed.
        /// </para>
        /// </remarks>
        public object? ConvertToArgumentTypeInvariant(object? value)
        {
            if (value == null || _elementType.IsAssignableFrom(value.GetType()))
            {
                return value;
            }
            else
            {
                if (!_converter.CanConvertFrom(value.GetType()))
                {
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.TypeConversionErrorFormat, value.GetType().FullName, _argumentType.FullName, _argumentName));
                }

                return _converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
            }
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="CommandLineArgument"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="CommandLineArgument"/>.</returns>
        /// <remarks>
        /// <para>
        ///   The string value matches the way the argument is displayed in the usage help's command line syntax
        ///   when using the default <see cref="WriteUsageOptions"/>.
        /// </para>
        /// </remarks>
        public override string ToString()
        {
            return ToString(new WriteUsageOptions());
        }

        internal string ToString(WriteUsageOptions options)
        {
            if (IsRequired)
            {
                return _parser.StringProvider.ArgumentSyntax(this, options);
            }
            else
            {
                return _parser.StringProvider.OptionalArgumentSyntax(this, options);
            }
        }

        internal bool HasInformation(WriteUsageOptions options)
        {
            if (!string.IsNullOrEmpty(Description))
            {
                return true;
            }

            if (options.UseAbbreviatedSyntax && Position == null)
            {
                return true;
            }

            if (options.UseShortNamesForSyntax)
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

            if (options.IncludeAliasInDescription &&
                ((Aliases != null && Aliases.Count > 0) || (ShortAliases != null && ShortAliases.Count > 0)))
            {
                return true;
            }

            if (options.IncludeDefaultValueInDescription && DefaultValue != null)
            {
                return true;
            }

            if (options.IncludeValidatorsInDescription &&
                _validators.Any(v => !string.IsNullOrEmpty(v.GetUsageHelp(this))))
            {
                return true;
            }

            return false;
        }

        internal bool SetValue(CultureInfo culture, string? value)
        {
            if (HasValue && !IsMultiValue && !_parser.AllowDuplicateArguments)
            {
                throw _parser.StringProvider.CreateException(CommandLineArgumentErrorCategory.DuplicateArgument, this);
            }

            _valueHelper ??= CreateValueHelper();

            bool continueParsing;
            if (IsMultiValue && value != null && MultiValueSeparator != null)
            {
                continueParsing = true;
                string[] values = value.Split(new[] { MultiValueSeparator }, StringSplitOptions.None);
                foreach (string separateValue in values)
                {
                    Validate(separateValue, ValidationMode.BeforeConversion);
                    var converted = ConvertToArgumentType(culture, separateValue);
                    continueParsing = _valueHelper.SetValue(this, culture, converted);
                    if (!continueParsing)
                    {
                        break;
                    }

                    Validate(converted, ValidationMode.AfterConversion);
                }
            }
            else
            {
                Validate(value, ValidationMode.BeforeConversion);
                var converted = ConvertToArgumentType(culture, value);
                continueParsing = _valueHelper.SetValue(this, culture, converted);
                Validate(converted, ValidationMode.AfterConversion);
            }

            HasValue = true;
            return continueParsing;
        }

        internal static CommandLineArgument Create(CommandLineParser parser, ParameterInfo parameter, IDictionary<Type, string>? defaultValueDescriptions)
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            if (parameter?.Name == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var typeConverterAttribute = parameter.GetCustomAttribute<TypeConverterAttribute>();
            var keyTypeConverterAttribute = parameter.GetCustomAttribute<KeyTypeConverterAttribute>();
            var valueTypeConverterAttribute = parameter.GetCustomAttribute<ValueTypeConverterAttribute>();
            var argumentNameAttribute = parameter.GetCustomAttribute<ArgumentNameAttribute>();
            var argumentName = DetermineArgumentName(argumentNameAttribute?.ArgumentName, parameter.Name, parser.NameTransform);
            var info = new ArgumentInfo()
            {
                Parser = parser,
                Parameter = parameter,
                ArgumentName = argumentName,
                Long = argumentNameAttribute?.Long ?? true,
                Short = argumentNameAttribute?.Short ?? false,
                ShortName = argumentNameAttribute?.ShortName ?? '\0',
                ArgumentType = parameter.ParameterType,
                Description = parameter.GetCustomAttribute<DescriptionAttribute>()?.Description,
                DefaultValue = (parameter.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault ? parameter.DefaultValue : null,
                ValueDescription = parameter.GetCustomAttribute<ValueDescriptionAttribute>()?.ValueDescription,
                AllowDuplicateDictionaryKeys = Attribute.IsDefined(parameter, typeof(AllowDuplicateDictionaryKeysAttribute)),
                ConverterType = typeConverterAttribute == null ? null : Type.GetType(typeConverterAttribute.ConverterTypeName, true),
                KeyConverterType = keyTypeConverterAttribute == null ? null : Type.GetType(keyTypeConverterAttribute.ConverterTypeName, true),
                ValueConverterType = valueTypeConverterAttribute == null ? null : Type.GetType(valueTypeConverterAttribute.ConverterTypeName, true),
                MultiValueSeparator = GetMultiValueSeparator(parameter.GetCustomAttribute<MultiValueSeparatorAttribute>()),
                KeyValueSeparator = parameter.GetCustomAttribute<KeyValueSeparatorAttribute>()?.Separator,
                Aliases = GetAliases(parameter.GetCustomAttributes<AliasAttribute>(), argumentName),
                ShortAliases = GetShortAliases(parameter.GetCustomAttributes<ShortAliasAttribute>(), argumentName),
                Position = parameter.Position,
                IsRequired = !parameter.IsOptional,
                MemberName = parameter.Name,
                AllowNull = DetermineAllowsNull(parameter),
                Validators = parameter.GetCustomAttributes<ArgumentValidationAttribute>(),
            };

            return new CommandLineArgument(info, defaultValueDescriptions);
        }

        internal static CommandLineArgument Create(CommandLineParser parser, PropertyInfo property, IDictionary<Type, string>? defaultValueDescriptions)
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            return Create(parser, property, null, property.PropertyType, DetermineAllowsNull(property), defaultValueDescriptions);
        }

        internal static CommandLineArgument Create(CommandLineParser parser, MethodInfo method, IDictionary<Type, string>? defaultValueDescriptions)
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            var infoTuple = DetermineMethodArgumentInfo(method);
            if (infoTuple == null)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.InvalidMethodSignatureFormat, method.Name));
            }

            var (methodInfo, argumentType, allowsNull) = infoTuple.Value;
            return Create(parser, null, methodInfo, argumentType, allowsNull, defaultValueDescriptions);
        }

        private static CommandLineArgument Create(CommandLineParser parser, PropertyInfo? property, MethodArgumentInfo? method,
            Type argumentType, bool allowsNull, IDictionary<Type, string>? defaultValueDescriptions)
        {
            var member = ((MemberInfo?)property ?? method?.Method)!;
            var attribute = member.GetCustomAttribute<CommandLineArgumentAttribute>();
            if (attribute == null)
            {
                throw new ArgumentException(Properties.Resources.MissingArgumentAttribute, nameof(method));
            }

            var typeConverterAttribute = member.GetCustomAttribute<TypeConverterAttribute>();
            var keyTypeConverterAttribute = member.GetCustomAttribute<KeyTypeConverterAttribute>();
            var valueTypeConverterAttribute = member.GetCustomAttribute<ValueTypeConverterAttribute>();
            var argumentName = DetermineArgumentName(attribute.ArgumentName, member.Name, parser.NameTransform);
            var info = new ArgumentInfo()
            {
                Parser = parser,
                Property = property,
                Method = method,
                ArgumentName = argumentName,
                Long = attribute.IsLong,
                Short = attribute.IsShort,
                ShortName = attribute.ShortName,
                ArgumentType = argumentType,
                Description = member.GetCustomAttribute<DescriptionAttribute>()?.Description,
                ValueDescription = attribute.ValueDescription,  // If null, the constructor will sort it out.
                Position = attribute.Position < 0 ? null : attribute.Position,
                AllowDuplicateDictionaryKeys = Attribute.IsDefined(member, typeof(AllowDuplicateDictionaryKeysAttribute)),
                ConverterType = typeConverterAttribute == null ? null : Type.GetType(typeConverterAttribute.ConverterTypeName, true),
                KeyConverterType = keyTypeConverterAttribute == null ? null : Type.GetType(keyTypeConverterAttribute.ConverterTypeName, true),
                ValueConverterType = valueTypeConverterAttribute == null ? null : Type.GetType(valueTypeConverterAttribute.ConverterTypeName, true),
                MultiValueSeparator = GetMultiValueSeparator(member.GetCustomAttribute<MultiValueSeparatorAttribute>()),
                KeyValueSeparator = member.GetCustomAttribute<KeyValueSeparatorAttribute>()?.Separator,
                Aliases = GetAliases(member.GetCustomAttributes<AliasAttribute>(), argumentName),
                ShortAliases = GetShortAliases(member.GetCustomAttributes<ShortAliasAttribute>(), argumentName),
                DefaultValue = attribute.DefaultValue,
                IsRequired = attribute.IsRequired,
                MemberName = member.Name,
                AllowNull = allowsNull,
                CancelParsing = attribute.CancelParsing,
                IsHidden = attribute.IsHidden,
                Validators = member.GetCustomAttributes<ArgumentValidationAttribute>(),
            };

            return new CommandLineArgument(info, defaultValueDescriptions);
        }

        internal static CommandLineArgument? CreateAutomaticHelp(CommandLineParser parser, IDictionary<Type, string>? defaultValueDescriptions)
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            var argumentName = DetermineArgumentName(null, parser.StringProvider.AutomaticHelpName(), parser.NameTransform);
            var shortName = parser.StringProvider.AutomaticHelpShortName();
            var shortAlias = char.ToLowerInvariant(argumentName[0]);
            if (parser.GetArgument(argumentName) != null ||
                (parser.Mode == ParsingMode.LongShort
                ? (parser.GetShortArgument(shortName) != null ||
                   parser.GetShortArgument(shortAlias) != null)
                : (parser.GetArgument(shortName.ToString()) != null ||
                   parser.GetArgument(shortAlias.ToString()) != null)))
            {
                return null;
            }

            var memberName = nameof(AutomaticHelp);
            var info = new ArgumentInfo()
            {
                Parser = parser,
                Method = new()
                {
                    Method = typeof(CommandLineArgument).GetMethod(memberName, BindingFlags.NonPublic | BindingFlags.Static)!,
                },
                ArgumentName = argumentName,
                Long = true,
                Short = true,
                ShortName = parser.StringProvider.AutomaticHelpShortName(),
                ArgumentType = typeof(bool),
                Description = parser.StringProvider.AutomaticHelpDescription(),
                MemberName = memberName,
                CancelParsing = true,
                Validators = Enumerable.Empty<ArgumentValidationAttribute>(),
            };

            if (parser.Mode == ParsingMode.LongShort)
            {
                info.ShortAliases = new[] { shortAlias };
            }
            else
            {
                info.Aliases = new[] { shortName.ToString(), shortAlias.ToString() };
            }

            return new CommandLineArgument(info, defaultValueDescriptions);
        }

        internal static CommandLineArgument? CreateAutomaticVersion(CommandLineParser parser, IDictionary<Type, string>? defaultValueDescriptions)
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            var argumentName = DetermineArgumentName(null, parser.StringProvider.AutomaticVersionName(), parser.NameTransform);
            if (parser.GetArgument(argumentName) != null)
            {
                return null;
            }

            var memberName = nameof(AutomaticVersion);
            var info = new ArgumentInfo()
            {
                Parser = parser,
                Method = new()
                {
                    Method = typeof(CommandLineArgument).GetMethod(memberName, BindingFlags.NonPublic | BindingFlags.Static)!,
                    HasParserParameter = true,
                },
                ArgumentName = argumentName,
                Long = true,
                ArgumentType = typeof(bool),
                Description = parser.StringProvider.AutomaticVersionDescription(),
                MemberName = memberName,
                Validators = Enumerable.Empty<ArgumentValidationAttribute>(),
            };

            return new CommandLineArgument(info, defaultValueDescriptions);
        }

        internal object? GetConstructorParameterValue()
        {
            return Value;
        }

        internal void ApplyPropertyValue(object target)
        {
            // Do nothing for parameter-based values
            if (_property == null)
            {
                return;
            }

            try
            {
                if (_valueHelper != null)
                {
                    _valueHelper.ApplyValue(target, _property);
                }
            }
            catch (TargetInvocationException ex)
            {
                throw _parser.StringProvider.CreateException(CommandLineArgumentErrorCategory.ApplyValueError, ex.InnerException, this);
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
            UsedArgumentName = null;
        }

        internal static void ShowVersion(Assembly assembly, string friendlyName)
        {
            var versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            var version = versionAttribute?.InformationalVersion ?? assembly.GetName().Version?.ToString() ?? string.Empty;
            var copyRightAttribute = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();

            Console.WriteLine($"{friendlyName} {version}");
            if (copyRightAttribute != null)
            {
                Console.WriteLine(copyRightAttribute.Copyright);
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

        private static string GetFriendlyTypeName(Type type)
        {
            // This is used to generate a value description from a type name if no custom value description was supplied.
            if (type.IsGenericType)
            {
                // We print Nullable<T> as just T.
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return GetFriendlyTypeName(type.GetGenericArguments()[0]);
                }
                else
                {
                    StringBuilder name = new StringBuilder(type.FullName?.Length ?? 0);
                    name.Append(type.Name, 0, type.Name.IndexOf("`", StringComparison.Ordinal));
                    name.Append('<');
                    // If only I was targeting .Net 4, I could use string.Join for this.
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
            }
            else
            {
                return type.Name;
            }
        }

        private TypeConverter CreateConverter(Type? converterType)
        {
            var converter = converterType == null ? TypeDescriptor.GetConverter(_elementType) : (TypeConverter?)Activator.CreateInstance(converterType);
            if (converter == null || !converter.CanConvertFrom(typeof(string)))
            {
                throw new NotSupportedException(string.Format(System.Globalization.CultureInfo.CurrentCulture, Properties.Resources.NoTypeConverterForArgumentFormat, _argumentName, _elementType));
            }

            return converter;
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
                type = typeof(MultiValueHelper<>).MakeGenericType(ElementType);
                return (IValueHelper)Activator.CreateInstance(type)!;

            case ArgumentKind.Method:
                return new MethodValueHelper();

            default:
                Debug.Assert(_defaultValue == null);
                return new SingleValueHelper(null);
            }
        }

        private static IEnumerable<string>? GetAliases(IEnumerable<AliasAttribute> aliasAttributes, string argumentName)
        {
            if (!aliasAttributes.Any())
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

        private static IEnumerable<char>? GetShortAliases(IEnumerable<ShortAliasAttribute> aliasAttributes, string argumentName)
        {
            if (!aliasAttributes.Any())
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

        private static bool DetermineDictionaryValueTypeAllowsNull(Type type, PropertyInfo? property, ParameterInfo? parameter)
        {
            var valueTypeNull = DetermineValueTypeNullable(type.GetGenericArguments()[1]);
            if (valueTypeNull != null)
            {
                return valueTypeNull.Value;
            }

#if NET6_0_OR_GREATER
            // Type is the IDictionary<,> implemented interface, not the actual type of the property
            // or parameter, which is what we need here.
            var actualType = property?.PropertyType ?? parameter?.ParameterType;

            // We can only determine the nullability state if the property or parameter's actual
            // type is Dictionary<,> or IDictionary<,>. Otherwise, we just assume nulls are
            // allowed.
            if (actualType != null && actualType.IsGenericType &&
                (actualType.GetGenericTypeDefinition() == typeof(Dictionary<,>) || actualType.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
            {
                var context = new NullabilityInfoContext();
                NullabilityInfo info;
                if (property != null)
                {
                    info = context.Create(property);
                }
                else
                {
                    info = context.Create(parameter!);
                }

                return info.GenericTypeArguments[1].ReadState != NullabilityState.NotNull;
            }
#endif

            return true;
        }

        private static bool DetermineCollectionElementTypeAllowsNull(Type type, PropertyInfo? property, ParameterInfo? parameter)
        {
            Type elementType = type.IsArray ? type.GetElementType()! : type.GetGenericArguments()[0];
            var valueTypeNull = DetermineValueTypeNullable(elementType);
            if (valueTypeNull != null)
            {
                return valueTypeNull.Value;
            }

#if NET6_0_OR_GREATER
            // Type is the ICollection<> implemented interface, not the actual type of the property
            // or parameter, which is what we need here.
            var actualType = property?.PropertyType ?? parameter?.ParameterType;

            // We can only determine the nullability state if the property or parameter's actual
            // type is an array or ICollection<>. Otherwise, we just assume nulls are allowed.
            if (actualType != null && (actualType.IsArray || (actualType.IsGenericType &&
                actualType.GetGenericTypeDefinition() == typeof(ICollection<>))))
            {
                var context = new NullabilityInfoContext();
                NullabilityInfo info;
                if (property != null)
                {
                    info = context.Create(property);
                }
                else
                {
                    info = context.Create(parameter!);
                }

                if (actualType.IsArray)
                {
                    return info.ElementType?.ReadState != NullabilityState.NotNull;
                }
                else
                {
                    return info.GenericTypeArguments[0].ReadState != NullabilityState.NotNull;
                }
            }
#endif

            return true;
        }

        private static bool DetermineAllowsNull(ParameterInfo parameter)
        {
            var valueTypeNull = DetermineValueTypeNullable(parameter.ParameterType);
            if (valueTypeNull != null)
            {
                return valueTypeNull.Value;
            }

#if NET6_0_OR_GREATER
            var context = new NullabilityInfoContext();
            var info = context.Create(parameter);
            return info.WriteState != NullabilityState.NotNull;
#else
            return true;
#endif
        }

        private static bool DetermineAllowsNull(PropertyInfo property)
        {
            var valueTypeNull = DetermineValueTypeNullable(property.PropertyType);
            if (valueTypeNull != null)
            {
                return valueTypeNull.Value;
            }

#if NET6_0_OR_GREATER
            var context = new NullabilityInfoContext();
            var info = context.Create(property);
            return info.WriteState != NullabilityState.NotNull;
#else
            return true;
#endif
        }

        private static bool? DetermineValueTypeNullable(Type type)
        {
            if (type.IsValueType)
            {
                return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            }

            return null;
        }

        // Returns a tuple of (collectionType, dictionaryType, elementType)
        private (Type?, Type?, Type?) DetermineMultiValueType()
        {
            // If the type is Dictionary<TKey, TValue> it doesn't matter if the property is
            // read-only or not.
            if (_argumentType.IsGenericType && _argumentType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var elementType = typeof(KeyValuePair<,>).MakeGenericType(_argumentType.GetGenericArguments());
                return (null, _argumentType, elementType);
            }

            if (_argumentType.IsArray)
            {
                if (_argumentType.GetArrayRank() != 1)
                {
                    throw new NotSupportedException(Properties.Resources.InvalidArrayRank);
                }

                if (_property != null && _property.GetSetMethod() == null)
                {
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.PropertyIsReadOnlyFormat, _argumentName));
                }

                var elementType = _argumentType.GetElementType()!;
                return (_argumentType, null, elementType);
            }

            // The interface approach requires a read-only property. If it's read-write, treat it
            // like a non-multi-value argument.
            // Don't use CanWrite because that returns true for properties with a private set
            // accessor.
            if (_property == null || _property.GetSetMethod() != null)
            {
                return (null, null, null);
            }

            var dictionaryType = TypeHelper.FindGenericInterface(_argumentType, typeof(IDictionary<,>));
            if (dictionaryType != null)
            {
                var elementType = typeof(KeyValuePair<,>).MakeGenericType(dictionaryType.GetGenericArguments());
                return (null, dictionaryType, elementType);
            }

            var collectionType = TypeHelper.FindGenericInterface(_argumentType, typeof(ICollection<>));
            if (collectionType != null)
            {
                var elementType = collectionType.GetGenericArguments()[0];
                return (collectionType, null, elementType);
            }

            // This is a read-only property with an unsupported type.
            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.PropertyIsReadOnlyFormat, _argumentName));
        }

        private static (MethodArgumentInfo, Type, bool)? DetermineMethodArgumentInfo(MethodInfo method)
        {
            var parameters = method.GetParameters();
            if (!method.IsStatic ||
                (method.ReturnType != typeof(bool) && method.ReturnType != typeof(void)) ||
                parameters.Length > 2)
            {
                return null;
            }

            bool allowsNull = false;
            var argumentType = typeof(bool);
            var info = new MethodArgumentInfo() { Method = method };
            if (parameters.Length == 2)
            {
                argumentType = parameters[0].ParameterType;
                if (parameters[1].ParameterType != typeof(CommandLineParser))
                {
                    return null;
                }

                info.HasValueParameter = true;
                info.HasParserParameter = true;
            }
            else if (parameters.Length == 1)
            {
                if (parameters[0].ParameterType == typeof(CommandLineParser))
                {
                    info.HasParserParameter = true;
                }
                else
                {
                    argumentType = parameters[0].ParameterType;
                    info.HasValueParameter = true;
                }
            }

            if (info.HasValueParameter)
            {
                allowsNull = DetermineAllowsNull(parameters[0]);
            }

            return (info, argumentType, allowsNull);
        }

        private static void AutomaticHelp()
        {
            // Intentionally blank.
        }

        private static bool AutomaticVersion(CommandLineParser parser)
        {
            ShowVersion(parser.ArgumentsType.Assembly, parser.ApplicationFriendlyName);

            // Cancel parsing but do not show help.
            return false;
        }

        private static string DetermineArgumentName(string? explicitName, string memberName, NameTransform transform)
        {
            if (explicitName != null)
            {
                return explicitName;
            }

            return transform.Apply(memberName);
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

        private static string? GetDefaultValueDescription(Type type, IDictionary<Type, string>? defaultValueDescriptions)
        {
            if (defaultValueDescriptions == null)
            {
                return null;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments()[0];
            }

            if (defaultValueDescriptions.TryGetValue(type, out string? value))
            {
                return value;
            }

            return null;
        }

        private static string DetermineValueDescription(Type type, IDictionary<Type, string>? defaultValueDescriptions)
        {
            return GetDefaultValueDescription(type, defaultValueDescriptions) ?? GetFriendlyTypeName(type);
        }
    }
}
