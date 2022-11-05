// Copyright (c) Sven Groot (Ookii.org)
using System;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Provides a custom value description for use in the usage help for an argument created from a constructor parameter.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   The value description is a short (typically one word) description that indicates the type of value that
    ///   the user should supply. By default the type of the parameter is used. If the type is an array type, the
    ///   array's element type is used. If the type is a nullable type, the nullable type's underlying type is used.
    /// </para>
    /// <para>
    ///   If you want to override the value description for all arguments of a specific type, 
    ///   use the <see cref="ParseOptions.DefaultValueDescriptions"/> property.
    /// </para>
    /// <para>
    ///   The value description is used when printing usage. For example, the usage for an argument named Sample with
    ///   a value description of String would look like "-Sample &lt;String&gt;".
    /// </para>
    /// <note>
    ///   This is not the long description used to describe the purpose of the argument. That should be specified
    ///   using the <see cref="System.ComponentModel.DescriptionAttribute"/> attribute.
    /// </note>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class ValueDescriptionAttribute : Attribute
    {
        private readonly string _valueDescription;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueDescriptionAttribute"/> class.
        /// </summary>
        /// <param name="valueDescription">The custom value description.</param>
        public ValueDescriptionAttribute(string valueDescription)
        {
            if (valueDescription == null)
            {
                throw new ArgumentNullException(nameof(valueDescription));
            }

            _valueDescription = valueDescription;
        }

        /// <summary>
        /// Gets the custom value description.
        /// </summary>
        /// <value>
        /// The custom value description.
        /// </value>
        public string ValueDescription
        {
            get { return _valueDescription; }
        }
    }
}
