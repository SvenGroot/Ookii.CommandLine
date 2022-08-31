// Copyright (c) Sven Groot (Ookii.org)
using System;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Defines a custom key-value separator for dictionary arguments.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   By default, dictionary arguments use the equals sign ('=') as a separator. By using this
    ///   attribute, you can choose a custom separator. This separator cannot appear in the key,
    ///   but can appear in the value.
    /// </para>
    /// <para>
    ///   This attribute is ignored if the dictionary argument uses the <see cref="System.ComponentModel.TypeConverterAttribute"/>
    ///   attribute.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class KeyValueSeparatorAttribute : Attribute
    {
        private readonly string _separator;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueSeparatorAttribute"/> class.
        /// </summary>
        /// <param name="separator">The separator to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="separator"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="separator"/> is an empty string.</exception>
        public KeyValueSeparatorAttribute(string separator)
        {
            if (separator == null)
                throw new ArgumentNullException(nameof(separator));

            if (separator.Length == 0)
                throw new ArgumentException(Properties.Resources.EmptyKeyValueSeparator, nameof(separator));

            _separator = separator;
        }

        /// <summary>
        /// Gets the separator.
        /// </summary>
        public string Separator => _separator;
    }
}
