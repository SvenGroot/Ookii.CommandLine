﻿// Copyright (c) Sven Groot (Ookii.org)
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Specifies a <see cref="TypeConverter"/> to use for the keys of a dictionary argument.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   This attribute can be used along with the <see cref="ValueTypeConverterAttribute"/>
    ///   attribute to customize the parsing of a dictionary argument without having to write a
    ///   custom <see cref="TypeConverter"/> that returns a <see cref="KeyValuePair{TKey, TValue}"/>.
    /// </para>
    /// <para>
    ///   This attribute is ignored if the argument uses the <see cref="TypeConverterAttribute"/>
    ///   or if the argument is not a dictionary argument.
    /// </para>
    /// </remarks>
    /// <seealso cref="KeyValuePairConverter{TKey, TValue}"/>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class KeyTypeConverterAttribute : Attribute
    {
        private readonly string _converterTypeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyTypeConverterAttribute"/> class.
        /// </summary>
        /// <param name="converterType">The type of the custom <see cref="TypeConverter"/> to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="converterType"/> is <see langword="null"/>.</exception>
        public KeyTypeConverterAttribute(Type converterType)
        {
            _converterTypeName = converterType?.AssemblyQualifiedName ?? throw new ArgumentNullException(nameof(converterType));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyTypeConverterAttribute"/> class.
        /// </summary>
        /// <param name="converterTypeName">The type name of the custom <see cref="TypeConverter"/> to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="converterTypeName"/> is <see langword="null"/>.</exception>
        public KeyTypeConverterAttribute(string converterTypeName)
        {
            _converterTypeName = converterTypeName ?? throw new ArgumentNullException(nameof(converterTypeName));
        }

        /// <summary>
        /// Gets the name of the type of the custom <see cref="TypeConverter"/> to use.
        /// </summary>
        /// <value>
        /// The name of a type derived from the <see cref="TypeConverter"/> class.
        /// </value>
        public string ConverterTypeName => _converterTypeName;
    }
}
