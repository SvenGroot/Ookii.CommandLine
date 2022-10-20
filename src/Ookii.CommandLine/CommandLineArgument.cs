// Copyright (c) Sven Groot (Ookii.org)
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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

        private interface ICollectionHelper
        {
            Array Values { get; }
            void Add(object? value);
            void ApplyValues(object collection);
        }

        private class CollectionHelper<T> : ICollectionHelper
        {
            // The actual element type may not be nullable. This is handled by the allow null check
            // when parsing the value. Here, we always treat the values as if they're nullable.
            private readonly List<T?> _values = new();

            public Array Values
            {
                get { return _values.ToArray(); }
            }

            public void Add(object? value)
            {
                _values.Add((T?)value);
            }

            public void ApplyValues(object collectionObject)
            {
                var collection = (ICollection<T?>)collectionObject;
                foreach (T? value in _values)
                    collection.Add(value);
            }
        }

        private interface IDictionaryHelper
        {
            object Dictionary { get; }
            void Add(string argumentName, object keyValuePair);
            void ApplyValues(object dictionary);
        }

        private class DictionaryHelper<TKey, TValue> : IDictionaryHelper
            where TKey : notnull
        {
            // The actual value type may not be nullable. This is handled by the allow null check.
            private readonly Dictionary<TKey, TValue?> _dictionary = new();
            private readonly bool _allowDuplicateKeys;
            private readonly bool _allowNullValues;

            public DictionaryHelper(bool allowDuplicateKeys, bool allowNullValues)
            {
                _allowDuplicateKeys = allowDuplicateKeys;
                _allowNullValues = allowNullValues;
            }

            public object Dictionary
            {
                get { return _dictionary; }
            }

            public void Add(string argumentName, object keyValuePair)
            {
                var pair = (KeyValuePair<TKey, TValue>)keyValuePair;

                // With the KeyValuePairConverter, these should already be checked, but it's still
                // checked here to deal with custom converters.
                if (pair.Key == null || (!_allowNullValues && pair.Value == null))
                    throw new CommandLineArgumentException(String.Format(CultureInfo.CurrentCulture, Properties.Resources.NullArgumentValueFormat, argumentName), argumentName, CommandLineArgumentErrorCategory.NullArgumentValue);

                if (_allowDuplicateKeys)
                    _dictionary[pair.Key] = pair.Value;
                else
                    _dictionary.Add(pair.Key, pair.Value);
            }

            public void ApplyValues(object dictionaryObject)
            {
                var dictionary = (IDictionary<TKey, TValue?>)dictionaryObject;
                foreach (var pair in _dictionary)
                    dictionary.Add(pair.Key, pair.Value);
            }
        }

        #endregion

        private readonly CommandLineParser _parser;
        private readonly TypeConverter _converter;
        private readonly PropertyInfo? _property;
        private readonly string _valueDescription;
        private readonly string _argumentName;
        private readonly char _shortName;
        private readonly IList<string>? _aliases;
        private readonly Type _argumentType;
        private readonly Type _elementType;
        private readonly string? _description;
        private readonly bool _isRequired;
        private readonly string _memberName;
        private readonly object? _defaultValue;
        private readonly bool _isMultiValue;
        private readonly bool _isDictionary;
        private readonly bool _allowDuplicateDictionaryKeys;
        private readonly string? _multiValueSeparator;
        private readonly string? _keyValueSeparator;
        private readonly bool _allowNull;
        private readonly bool _cancelParsing;
        private object? _value;
        private struct ArgumentInfo
        {
            public CommandLineParser Parser { get; set; }
            public PropertyInfo? Property { get; set; }
            public ParameterInfo? Parameter { get; set; }
            public string MemberName { get; set; }
            public string ArgumentName { get; set; }
            public char ShortName { get; set; }
            public IList<string>? Aliases { get; set; }
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
        }

        private CommandLineArgument(ArgumentInfo info, ParsingMode mode)
        {
            // If this method throws anything other than a NotSupportedException, it constitutes a bug in the Ookii.CommandLine library.
            _parser = info.Parser;
            _property = info.Property;
            _memberName = info.MemberName;
            _argumentName = info.ArgumentName;
            if (mode == ParsingMode.LongShort)
                _shortName = info.ShortName;

            _aliases = info.Aliases;
            _argumentType = info.ArgumentType;
            _elementType = info.ArgumentType;
            _description = info.Description;
            _defaultValue = info.DefaultValue;
            _isRequired = info.IsRequired;
            _multiValueSeparator = info.MultiValueSeparator;
            _allowNull = info.AllowNull;
            _cancelParsing = info.CancelParsing;
            Position = info.Position;
            var converterType = info.ConverterType;

            Type? dictionaryType = null;
            Type? collectionType = null;
            if( _argumentType.IsGenericType && _argumentType.GetGenericTypeDefinition() == typeof(Dictionary<,>) )
            {
                dictionaryType = _argumentType;
            }
            else if( _argumentType.IsArray )
            {
                if( _argumentType.GetArrayRank() != 1 )
                    throw new NotSupportedException(Properties.Resources.InvalidArrayRank);
                _isMultiValue = true;
                _elementType = _argumentType.GetElementType()!;
                collectionType = _argumentType;
                if( _property != null && _property.GetSetMethod() == null )
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.PropertyIsReadOnlyFormat, _argumentName));
            }
            else if( _property != null && _property.GetSetMethod() == null ) // Don't use CanWrite because that returns true for properties with a private set accessor.
            {
                dictionaryType = TypeHelper.FindGenericInterface(_argumentType, typeof(IDictionary<,>));
                if( dictionaryType == null )
                {
                    collectionType = TypeHelper.FindGenericInterface(_argumentType, typeof(ICollection<>));
                    if( collectionType == null )
                    {
                        throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.PropertyIsReadOnlyFormat, _argumentName));
                    }
                    else
                    {
                        _isMultiValue = true;
                        _elementType = collectionType.GetGenericArguments()[0];
                    }
                }
            }

            if (dictionaryType != null)
            {
                _isDictionary = true;
                _isMultiValue = true;
                _allowDuplicateDictionaryKeys = info.AllowDuplicateDictionaryKeys;
                Type[] genericArguments = dictionaryType.GetGenericArguments();
                _elementType = typeof(KeyValuePair<,>).MakeGenericType(genericArguments);
                _allowNull = DetermineDictionaryValueTypeAllowsNull(dictionaryType, info.Property, info.Parameter);
                _keyValueSeparator = info.KeyValueSeparator ?? KeyValuePairConverter.DefaultSeparator;
                if (converterType == null)
                {
                    converterType = typeof(KeyValuePairConverter<,>).MakeGenericType(dictionaryType.GetGenericArguments());
                    _converter = (TypeConverter)Activator.CreateInstance(converterType, _argumentName, _allowNull, info.KeyConverterType, info.ValueConverterType, _keyValueSeparator)!;
                }

                _valueDescription = info.ValueDescription ?? string.Format(CultureInfo.CurrentCulture, "{0}{1}{2}", GetFriendlyTypeName(genericArguments[0]), _keyValueSeparator, GetFriendlyTypeName(genericArguments[1]));
            }
            else if (collectionType != null)
            {
                _allowNull = DetermineCollectionElementTypeAllowsNull(collectionType, info.Property, info.Parameter);
            }

            if ( _valueDescription == null )
                _valueDescription = info.ValueDescription ?? GetFriendlyTypeName(_elementType);

            if (_converter == null)
                _converter = CreateConverter(converterType);
            _defaultValue = DetermineDefaultValue(info.DefaultValue);
        }

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
        /// </remarks>
        public string ArgumentName
        {
            get { return _argumentName; }
        }

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
        /// Gets a value that indicates whether the argument has a short name.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the argument has a short name; otherwise, <see langword="false"/>.
        /// </value>
        public bool HasShortName => _shortName != '\0';

        /// <summary>
        /// Gets the alternative names for this command line argument.
        /// </summary>
        /// <value>
        /// A list of alternative names for this command line argument, or <see langword="null"/> if none were specified.
        /// </value>
        public IList<string>? Aliases
        {
            get { return _aliases; }
        }

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
            get { return _description ?? ""; }
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi")]
        public bool IsMultiValue
        {
            get { return _isMultiValue; }
        }

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi")]
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
        public bool IsDictionary
        {
            get { return _isDictionary; }
        }

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
        public object? Value
        {
            get
            {
                if( HasValue && _isDictionary )
                    return ((IDictionaryHelper)_value!).Dictionary;
                else if( HasValue && _isMultiValue )
                    return ((ICollectionHelper)_value!).Values;
                else
                    return _value;
            }
        }

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
        public bool HasValue { get; internal set; }

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
        /// Gets a value that indicates whether argument parsing should be cancelled if this
        /// argument is encountered.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if argument parsing should be cancelled after this argument;
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
        ///   usage information if parsing was cancelled through this method.
        /// </para>
        /// <para>
        ///   Cancelling parsing in this way is identical to handling the <see cref="CommandLineParser.ArgumentParsed"/>
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
        /// Converts the specified string to the argument type, as specified in the <see cref="ArgumentType"/> property.
        /// </summary>
        /// <param name="culture">The culture to use to convert the argument.</param>
        /// <param name="argument">The string to convert.</param>
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
        ///   <paramref name="argument"/> could not be converted to the type specified in the <see cref="ArgumentType"/> property.
        /// </exception>
        public object? ConvertToArgumentType(CultureInfo culture, string argument)
        {
            if( culture == null )
                throw new ArgumentNullException(nameof(culture));

            try
            {
                var converted = _converter.ConvertFrom(null, culture, argument);
                if (converted == null && (!_allowNull || _isDictionary))
                    throw new CommandLineArgumentException(String.Format(CultureInfo.CurrentCulture, Properties.Resources.NullArgumentValueFormat, ArgumentName), ArgumentName, CommandLineArgumentErrorCategory.NullArgumentValue);

                return converted;
            }
            catch( NotSupportedException ex )
            {
                throw new CommandLineArgumentException(string.Format(System.Globalization.CultureInfo.CurrentCulture, Properties.Resources.ArgumentConversionErrorFormat, argument, ArgumentName, ValueDescription), ArgumentName, CommandLineArgumentErrorCategory.ArgumentValueConversion, ex);
            }
            catch( FormatException ex )
            {
                throw new CommandLineArgumentException(string.Format(System.Globalization.CultureInfo.CurrentCulture, Properties.Resources.ArgumentConversionErrorFormat, argument, ArgumentName, ValueDescription), ArgumentName, CommandLineArgumentErrorCategory.ArgumentValueConversion, ex);
            }
            catch( Exception ex )
            {
                // Yeah, I don't like catching Exception, but unfortunately BaseNumberConverter (e.g. used for int) can *throw* a System.Exception (not a derived class) so there's nothing I can do about it.
                if( ex.InnerException is FormatException )
                    throw new CommandLineArgumentException(string.Format(System.Globalization.CultureInfo.CurrentCulture, Properties.Resources.ArgumentConversionErrorFormat, argument, ArgumentName, ValueDescription), ArgumentName, CommandLineArgumentErrorCategory.ArgumentValueConversion, ex);
                else
                    throw;
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
            string argumentName = _parser.ArgumentNamePrefixes[0] + ArgumentName;
            if( Position != null )
                argumentName = string.Format(CultureInfo.CurrentCulture, options.OptionalArgumentFormat, argumentName); // for positional parameters, the name itself is optional

            string argument = argumentName;
            if( !IsSwitch )
            {
                char separator = (_parser.AllowWhiteSpaceValueSeparator && options.UseWhiteSpaceValueSeparator) ? ' ' : _parser.NameValueSeparator;
                string argumentValue = string.Format(CultureInfo.CurrentCulture, options.ValueDescriptionFormat, ValueDescription);
                argument = argumentName + separator + argumentValue;
            }
            if( IsMultiValue )
                argument += options.ArraySuffix;
            if( IsRequired )
                return argument;
            else
                return string.Format(CultureInfo.CurrentCulture, options.OptionalArgumentFormat, argument);
        }

        internal void SetValue(CultureInfo culture, string? value)
        {
            if( IsMultiValue && _multiValueSeparator != null )
            {
                if (value == null)
                    throw new CommandLineArgumentException(string.Format(System.Globalization.CultureInfo.CurrentCulture, Properties.Resources.MissingValueForNamedArgumentFormat, ArgumentName), ArgumentName, CommandLineArgumentErrorCategory.MissingNamedArgumentValue);

                string[] values = value!.Split(new[] { _multiValueSeparator }, StringSplitOptions.None);
                foreach( string separateValue in values )
                    SetValueCore(culture, separateValue);
            }
            else
                SetValueCore(culture, value);
        }

        internal static CommandLineArgument Create(CommandLineParser parser, ParameterInfo parameter)
        {
            if( parser == null )
                throw new ArgumentNullException(nameof(parser));
            if( parameter?.Name == null )
                throw new ArgumentNullException(nameof(parameter));

            var typeConverterAttribute = TypeHelper.GetAttribute<TypeConverterAttribute>(parameter);
            var keyTypeConverterAttribute = TypeHelper.GetAttribute<KeyTypeConverterAttribute>(parameter);
            var valueTypeConverterAttribute = TypeHelper.GetAttribute<ValueTypeConverterAttribute>(parameter);
            var argumentNameAttribute = TypeHelper.GetAttribute<ArgumentNameAttribute>(parameter);
            var argumentName = argumentNameAttribute?.ArgumentName ?? parameter.Name;
            var info = new ArgumentInfo()
            {
                Parser = parser,
                Parameter = parameter,
                ArgumentName = argumentName,
                ShortName = argumentNameAttribute?.ShortName ?? '\0',
                ArgumentType = parameter.ParameterType,
                Description = TypeHelper.GetAttribute<DescriptionAttribute>(parameter)?.Description,
                DefaultValue = (parameter.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault ? parameter.DefaultValue : null,
                ValueDescription = TypeHelper.GetAttribute<ValueDescriptionAttribute>(parameter)?.ValueDescription,
                AllowDuplicateDictionaryKeys = Attribute.IsDefined(parameter, typeof(AllowDuplicateDictionaryKeysAttribute)),
                ConverterType = typeConverterAttribute == null ? null : Type.GetType(typeConverterAttribute.ConverterTypeName, true),
                KeyConverterType = keyTypeConverterAttribute == null ? null : Type.GetType(keyTypeConverterAttribute.ConverterTypeName, true),
                ValueConverterType = valueTypeConverterAttribute == null ? null : Type.GetType(valueTypeConverterAttribute.ConverterTypeName, true),
                MultiValueSeparator = GetMultiValueSeparator(TypeHelper.GetAttribute<MultiValueSeparatorAttribute>(parameter)),
                KeyValueSeparator = TypeHelper.GetAttribute<KeyValueSeparatorAttribute>(parameter)?.Separator,
                Aliases = GetAliases(Attribute.GetCustomAttributes(parameter, typeof(AliasAttribute)), argumentName),
                Position = parameter.Position,
                IsRequired = !parameter.IsOptional,
                MemberName = parameter.Name,
                AllowNull = DetermineAllowsNull(parameter),
            };

            return new CommandLineArgument(info, parser.Mode);
        }

        internal static CommandLineArgument Create(CommandLineParser parser, PropertyInfo property)
        {
            if( parser == null )
                throw new ArgumentNullException(nameof(parser));
            if( property == null )
                throw new ArgumentNullException(nameof(property));
            var attribute = TypeHelper.GetAttribute<CommandLineArgumentAttribute>(property);
            if( attribute == null )
                throw new ArgumentException(Properties.Resources.MissingArgumentAttribute, nameof(property));

            var typeConverterAttribute = TypeHelper.GetAttribute<TypeConverterAttribute>(property);
            var keyTypeConverterAttribute = TypeHelper.GetAttribute<KeyTypeConverterAttribute>(property);
            var valueTypeConverterAttribute = TypeHelper.GetAttribute<ValueTypeConverterAttribute>(property);
            var argumentName = attribute.ArgumentName ?? property.Name;
            var info = new ArgumentInfo()
            {
                Parser = parser,
                Property = property,
                ArgumentName = argumentName,
                ShortName = attribute.ShortName,
                ArgumentType = property.PropertyType,
                Description = TypeHelper.GetAttribute<DescriptionAttribute>(property)?.Description,
                ValueDescription = attribute.ValueDescription,  // If null, the ctor will sort it out.
                Position = attribute.Position < 0 ? null : attribute.Position,
                AllowDuplicateDictionaryKeys = Attribute.IsDefined(property, typeof(AllowDuplicateDictionaryKeysAttribute)),
                ConverterType = typeConverterAttribute == null ? null : Type.GetType(typeConverterAttribute.ConverterTypeName, true),
                KeyConverterType = keyTypeConverterAttribute == null ? null : Type.GetType(keyTypeConverterAttribute.ConverterTypeName, true),
                ValueConverterType = valueTypeConverterAttribute == null ? null : Type.GetType(valueTypeConverterAttribute.ConverterTypeName, true),
                MultiValueSeparator = GetMultiValueSeparator(TypeHelper.GetAttribute<MultiValueSeparatorAttribute>(property)),
                KeyValueSeparator = TypeHelper.GetAttribute<KeyValueSeparatorAttribute>(property)?.Separator,
                Aliases = GetAliases(Attribute.GetCustomAttributes(property, typeof(AliasAttribute)), argumentName),
                DefaultValue = attribute.DefaultValue,
                IsRequired = attribute.IsRequired,
                MemberName = property.Name,
                AllowNull = DetermineAllowsNull(property),
                CancelParsing = attribute.CancelParsing,
            };

            return new CommandLineArgument(info, parser.Mode);
        }

        internal void ApplyPropertyValue(object target)
        {
            if( target == null )
                throw new ArgumentNullException(nameof(target));

            // Do nothing for parameter-based values
            if( _property != null )
            {
                try
                {
                    if( _isDictionary && _property.GetSetMethod() == null )
                    {
                        var dictionary = (System.Collections.IDictionary?)_property.GetValue(target, null);
                        if (dictionary == null)
                            throw new InvalidOperationException();

                        dictionary.Clear();
                        if( HasValue )
                            ((IDictionaryHelper)_value!).ApplyValues(dictionary);
                    }
                    else if( !_isDictionary && _isMultiValue && !ArgumentType.IsArray )
                    {
                        object? collection = _property.GetValue(target, null);
                        if (collection == null)
                            throw new InvalidOperationException();

                        System.Collections.IList? list = collection as System.Collections.IList;
                        if( list != null )
                            list.Clear();
                        else
                            typeof(ICollection<>).MakeGenericType(ElementType).GetMethod("Clear")?.Invoke(collection, null);

                        if( HasValue )
                            ((ICollectionHelper)_value!).ApplyValues(collection);
                    }
                    else if( HasValue || Value != null ) // Don't set the value of an unspecified argument with a null default value.
                        _property.SetValue(target, Value, null);
                }
                catch( TargetInvocationException ex )
                {
                    throw new CommandLineArgumentException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.SetValueErrorFormat, ArgumentName, ex.InnerException?.Message), ArgumentName, CommandLineArgumentErrorCategory.ApplyValueError, ex.InnerException);
                }
            }
        }

        internal void Reset()
        {
            _value = IsMultiValue ? null : DefaultValue;
            HasValue = false;
            UsedArgumentName = null;
        }

        private void SetValueCore(CultureInfo culture, string? value)
        {
            object? convertedValue;
            if( value == null )
            {
                if( IsSwitch )
                    convertedValue = true;
                else
                    throw new CommandLineArgumentException(string.Format(System.Globalization.CultureInfo.CurrentCulture, Properties.Resources.MissingValueForNamedArgumentFormat, ArgumentName), ArgumentName, CommandLineArgumentErrorCategory.MissingNamedArgumentValue);
            }
            else
                convertedValue = ConvertToArgumentType(culture, value);

            if( IsDictionary )
            {
                // Value is not null because IsDictionary and IsSwitch are mutually exclusive.
                // ConvertedValue is not null because ConvertToArgumentType ensures that if the argument is a dictionary.
                SetDictionaryValue(value!, convertedValue!);
            }
            else if( IsMultiValue )
            {
                SetCollectionValue(convertedValue);
            }
            else if( !HasValue || _parser.AllowDuplicateArguments )
            {
                _value = convertedValue;
                HasValue = true;
            }
            else
                throw new CommandLineArgumentException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.DuplicateArgumentFormat, ArgumentName), ArgumentName, CommandLineArgumentErrorCategory.DuplicateArgument);
        }

        private static string? GetMultiValueSeparator(MultiValueSeparatorAttribute? attribute)
        {
            var separator = attribute?.Separator;
            if( string.IsNullOrEmpty(separator) )
                return null;
            else
                return separator;
        }

        private static string GetFriendlyTypeName(Type type)
        {
            // This is used to generate a value description from a type name if no custom value description was supplied.
            if( type.IsGenericType )
            {
                // We print Nullable<T> as just T.
                if( type.GetGenericTypeDefinition() == typeof(Nullable<>) )
                    return GetFriendlyTypeName(type.GetGenericArguments()[0]);
                else
                {
                    StringBuilder name = new StringBuilder(type.FullName?.Length ?? 0);
                    name.Append(type.Name, 0, type.Name.IndexOf("`", StringComparison.Ordinal));
                    name.Append('<');
                    // If only I was targetting .Net 4, I could use string.Join for this.
                    bool first = true;
                    foreach( Type typeArgument in type.GetGenericArguments() )
                    {
                        if( first )
                            first = false;
                        else
                            name.Append(", ");
                        name.Append(GetFriendlyTypeName(typeArgument));
                    }
                    name.Append('>');
                    return name.ToString();
                }
            }
            else
                return type.Name;
        }

        private TypeConverter CreateConverter(Type? converterType)
        {
            var converter = converterType == null ? TypeDescriptor.GetConverter(_elementType) : (TypeConverter?)Activator.CreateInstance(converterType);
            if( converter == null || !converter.CanConvertFrom(typeof(string)) )
                throw new NotSupportedException(string.Format(System.Globalization.CultureInfo.CurrentCulture, Properties.Resources.NoTypeConverterForArgumentFormat, _argumentName, _elementType));

            return converter;
        }

        private object? DetermineDefaultValue(object? defaultValue)
        {
            if( defaultValue == null || _elementType.IsAssignableFrom(defaultValue.GetType()) )
                return defaultValue;
            else
            {
                if( !_converter.CanConvertFrom(defaultValue.GetType()) )
                    throw new NotSupportedException(string.Format(System.Globalization.CultureInfo.CurrentCulture, Properties.Resources.IncorrectDefaultValueTypeFormat, _argumentName));
                return _converter.ConvertFrom(defaultValue);
            }
        }

        private void SetDictionaryValue(string value, object convertedValue)
        {
            if( !HasValue )
            {
                Debug.Assert(_value == null);
                // _elementType is KeyValuePair<TKey, TValue>, so we use that to get the generic arguments for the dictionary.
                _value = Activator.CreateInstance(typeof(DictionaryHelper<,>).MakeGenericType(_elementType.GetGenericArguments()), _allowDuplicateDictionaryKeys, _allowNull);
                HasValue = true;
            }

            try
            {
                ((IDictionaryHelper)_value!).Add(_argumentName, convertedValue);
            }
            catch( ArgumentException ex )
            {
                throw new CommandLineArgumentException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.InvalidDictionaryValueFormat, value, ArgumentName, ex.Message), ArgumentName, CommandLineArgumentErrorCategory.InvalidDictionaryValue, ex);
            }
        }

        private void SetCollectionValue(object? convertedValue)
        {
            if( !HasValue )
            {
                Debug.Assert(_value == null);
                _value = Activator.CreateInstance(typeof(CollectionHelper<>).MakeGenericType(ElementType));
                HasValue = true;
            }
            ((ICollectionHelper)_value!).Add(convertedValue);
        }

        private static IList<string>? GetAliases(Attribute[] aliasAttributes, string argumentName)
        {
            if( aliasAttributes == null || aliasAttributes.Length == 0 )
                return null;

            string[] aliases = new string[aliasAttributes.Length];
            for( int x = 0; x < aliasAttributes.Length; ++x )
            {
                aliases[x] = ((AliasAttribute)aliasAttributes[x]).Alias;
                if( string.IsNullOrEmpty(aliases[x]) )
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.EmptyAliasFormat, argumentName));
            }

            return aliases;
        }

        private static bool DetermineDictionaryValueTypeAllowsNull(Type type, PropertyInfo? property, ParameterInfo? parameter)
        {
            var valueTypeNull = DetermineValueTypeNullable(type.GetGenericArguments()[1]);
            if (valueTypeNull != null)
                return valueTypeNull.Value;

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
                    info = context.Create(property);
                else
                    info = context.Create(parameter!);

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
                return valueTypeNull.Value;

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
                    info = context.Create(property);
                else
                    info = context.Create(parameter!);

                if (actualType.IsArray)
                    return info.ElementType?.ReadState != NullabilityState.NotNull;
                else
                    return info.GenericTypeArguments[0].ReadState != NullabilityState.NotNull;
            }
#endif

            return true;
        }

        private static bool DetermineAllowsNull(ParameterInfo parameter)
        {
            var valueTypeNull = DetermineValueTypeNullable(parameter.ParameterType);
            if (valueTypeNull != null)
                return valueTypeNull.Value;

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
                return valueTypeNull.Value;

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
    }
}
