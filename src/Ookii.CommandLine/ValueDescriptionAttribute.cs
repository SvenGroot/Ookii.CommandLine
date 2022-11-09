// Copyright (c) Sven Groot (Ookii.org)
using System;
using System.ComponentModel;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Provides a custom value description for use in the usage help for an argument created from a constructor parameter.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   The value description is a short, typically one-word description that indicates the
    ///   type of value that the user should supply.
    /// </para>
    /// <para>
    ///   If not specified here, it is retrieved from the <see cref="ParseOptions.DefaultValueDescriptions"/>
    ///   property, and if not found there, the type of the property is used, applying the
    ///   <see cref="NameTransform"/> specified by the <see cref="ParseOptions.ValueDescriptionTransform"/>
    ///   property or the <see cref="ParseOptionsAttribute.ValueDescriptionTransform"/> property.
    ///   If this is a multi-value argument, the element type is used. If the type is <see cref="Nullable{T}"/>,
    ///   its underlying type is used.
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
    ///   using the <see cref="DescriptionAttribute"/> attribute.
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
        /// <exception cref="ArgumentNullException">
        /// <paramref name="valueDescription"/> is <see langword="null"/>.
        /// </exception>
        public ValueDescriptionAttribute(string valueDescription)
        {
            _valueDescription = valueDescription ?? throw new ArgumentNullException(nameof(valueDescription));
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
