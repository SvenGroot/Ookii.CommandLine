using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Indicates what argument parsing rules should be used to interpret the command line.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   To set the parsing mode for a <see cref="CommandLineParser"/>, use the <see cref="ParseOptionsAttribute.Mode"/>
    ///   property or the <see cref="ParseOptions.Mode"/> property.
    /// </para>
    /// </remarks>
    public enum ParsingMode
    {
        /// <summary>
        /// Use the normal Ookii.CommandLine parsing rules.
        /// </summary>
        Default,
        /// <summary>
        /// Allow arguments to have both long and short names, using the <see cref="CommandLineParser.LongArgumentNamePrefix"/>
        /// to specify a long name, and the regular <see cref="CommandLineParser.ArgumentNamePrefixes"/>
        /// to specify a short name.
        /// </summary>
        LongShort
    }
}
