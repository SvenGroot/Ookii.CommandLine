using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Tests;

class DerivedDescriptionAttribute : DescriptionAttribute
{
    public override string Description => "The second category.";
}

enum ArgumentCategory
{
    [Description("The first category.")]
    Category1,
    [DerivedDescription()]
    Category2,
    // No description
    Category3,
    // Unused
    [Description("The fourth category.")]
    Category4,
}
