using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Indicates which arguments should be included in the description list when printing usage.
    /// </summary>
    /// <seealso cref="WriteUsageOptions.ArgumentDescriptionListFilter"/>
    public enum DescriptionListFilterMode
    {
        /// <summary>
        /// Include arguments that have any information that is not included in the syntax,
        /// such as aliases, a default value, or a description.
        /// </summary>
        Information,
        /// <summary>
        /// Include only arguments that have a description.
        /// </summary>
        Description,
        /// <summary>
        /// Include all arguments.
        /// </summary>
        All,
        /// <summary>
        /// Omit the description list entirely.
        /// </summary>
        None
    }
}
