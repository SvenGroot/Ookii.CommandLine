// Copyright (c) Sven Groot (Ookii.org)
using Ookii.CommandLine.Terminal;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ookii.CommandLine.Commands
{
    /// <summary>
    /// Provides options for the <see cref="CommandManager"/> class.
    /// </summary>
    public class CommandOptions : ParseOptions
    {
        /// <summary>
        /// Gets the default value for the <see cref="CommandDescriptionIndent"/> property.
        /// </summary>
        public const int DefaultCommandDescriptionIndent = 8;

        /// <summary>
        /// Gets or sets the <see cref="IEqualityComparer{T}"/> used to compare command names.
        /// </summary>
        /// <value>
        /// The <see cref="IEqualityComparer{T}"/> used to compare command names. The default value is <see cref="StringComparer.OrdinalIgnoreCase"/>.
        /// </value>
        public IComparer<string> CommandNameComparer { get; set; } = StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// Gets or sets the color applied to the <see cref="LocalizedStringProvider.CommandDescription"/>.
        /// </summary>
        /// <value>
        ///   The virtual terminal sequence for a color. The default value is
        ///   <see cref="TextFormat.ForegroundGreen"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The color will only be used if the <see cref="WriteUsageOptions.UseColor"/> property is
        ///   <see langword="true"/>; otherwise, it will be replaced with an empty string.
        /// </para>
        /// <para>
        ///   If the string contains anything other than virtual terminal sequences, those parts
        ///   will be included in the output, but only when the <see cref="WriteUsageOptions.UseColor"/> property is
        ///   <see langword="true"/>.
        /// </para>
        /// <para>
        ///   The portion of the string that has color will end with the <see cref="WriteUsageOptions.ColorReset"/>.
        /// </para>
        /// <para>
        ///   With the default value, only the command name portion of the string has color; the
        ///   application name does not.
        /// </para>
        /// </remarks>
        public string CommandDescriptionColor { get; set; } = TextFormat.ForegroundGreen;

        /// <summary>
        /// Gets or sets the number of characters by which to indent the all but the first line of command descriptions.
        /// </summary>
        /// <value>
        /// The number of characters by which to indent the all but the first line of command descriptions. The default value is 8.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This value should be adjusted to match the return value specified by the
        ///   <see cref="LocalizedStringProvider.CommandDescription"/> property.
        /// </para>
        /// <para>
        ///   This value is not used if <see cref="ParseOptions.Out"/> is not a <see cref="LineWrappingTextWriter"/>, or the
        ///   maximum line length is less than 30.
        /// </para>
        /// </remarks>
        public int CommandDescriptionIndent { get; set; } = DefaultCommandDescriptionIndent;

        /// <summary>
        /// Gets or sets a value that indicates whether a version command should automatically be
        /// created.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to automatically create a version command; otherwise,
        /// <see langword="false"/>. The default is <see langword="true"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If this property is true, a command named "version" will be automatically added to
        ///   the list of available commands, unless a command with that name already exists.
        ///   When invoked, the command will show version information for the application, based
        ///   on the entry point assembly.
        /// </para>
        /// </remarks>
        public bool AutoVersionCommand { get; set; } = true;

    }
}
