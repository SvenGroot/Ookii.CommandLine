using System;
using System.Reflection;

namespace Ookii.CommandLine;

/// <summary>
/// Sets the friendly name of the application to be used in the output of the "-Version"
/// argument or "version" subcommand.
/// </summary>
/// <remarks>
/// <para>
///   This attribute is used when a "-Version" argument is automatically added to the arguments
///   of your application, and by the automatically added "version" subcommand. It can be applied to
///   the type defining command line arguments, or to the assembly that contains it.
/// </para>
/// <para>
///   If not present, the automatic "-Version" argument and "version" command will use the 
///   value of the <see cref="AssemblyTitleAttribute"/> attribute, or the assembly name of the
///   assembly containing the arguments type.
/// </para>
/// <para>
///   For the "version" subcommand, this attribute must be applied to the entry point assembly of
///   your application.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="true"/>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
public class ApplicationFriendlyNameAttribute : Attribute
{
    private readonly string _name;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationFriendlyNameAttribute"/>
    /// attribute.
    /// </summary>
    /// <param name="name">The friendly name of the application.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    public ApplicationFriendlyNameAttribute(string name)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Gets the friendly name of the application.
    /// </summary>
    /// <value>
    /// The friendly name of the application.
    /// </value>
    public string Name => _name;
}
