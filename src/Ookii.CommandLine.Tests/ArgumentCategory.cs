using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Tests;

enum ArgumentCategory
{
    [Description("The first category.")]
    Category1,
    [Description("The second category.")]
    Category2,
    // No description
    Category3,
    // Unused
    [Description("The fourth category.")]
    Category4,
}
