using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Sets the friendly name of the application to be used in the output of the "-Version"
    /// argument.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   This attribute is used when a "-Version" argument is automatically added to the arguments
    ///   of your application. It can be applied to the type defining command line arguments, or
    ///   to the assembly.
    /// </para>
    /// <para>
    ///   If not present, the automatic "-Version" argument will use the assembly name.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class ApplicationFriendlyNameAttribute : Attribute
    {
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationFriendlyNameAttribute"/>
        /// attribute.
        /// </summary>
        /// <param name="name">The friendly name of the application.</param>
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
}
