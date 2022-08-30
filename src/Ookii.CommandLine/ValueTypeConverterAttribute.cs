using System;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Specifies a <see cref="System.ComponentModel.TypeConverter"/> to use for the values of a 
    /// dictionary argument.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   This attribute can be used along with <see cref="KeyTypeConverterAttribute"/> to
    ///   customize the parsing of a dictionary argument without having to write a custom
    ///   <see cref="System.ComponentModel.TypeConverter"/> that returns a <see cref="System.Collections.Generic.KeyValuePair{TKey, TValue}"/>.
    /// </para>
    /// <para>
    ///   This attribute is ignored if the argument uses the <see cref="System.ComponentModel.TypeConverterAttribute"/>
    ///   or if the argument is not a dictionary argument.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class ValueTypeConverterAttribute : Attribute
    {
        private readonly string _converterTypeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTypeConverterAttribute"/> class.
        /// </summary>
        /// <param name="converterType">The type of the custom <see cref="System.ComponentModel.TypeConverter"/> to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="converterType"/> is <see langword="null"/>.</exception>
        public ValueTypeConverterAttribute(Type converterType)
        {
            _converterTypeName = converterType?.AssemblyQualifiedName ?? throw new ArgumentNullException(nameof(converterType));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTypeConverterAttribute"/> class.
        /// </summary>
        /// <param name="converterTypeName">The type name of the custom <see cref="System.ComponentModel.TypeConverter"/> to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="converterTypeName"/> is <see langword="null"/>.</exception>
        public ValueTypeConverterAttribute(string converterTypeName)
        {
            _converterTypeName = converterTypeName ?? throw new ArgumentNullException(nameof(converterTypeName));
        }

        /// <summary>
        /// Gets the type of the custom <see cref="System.ComponentModel.TypeConverter"/> to use.
        /// </summary>
        public string ConverterTypeName => _converterTypeName;
    }
}
