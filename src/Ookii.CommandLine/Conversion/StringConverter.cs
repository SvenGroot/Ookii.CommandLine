using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Conversion;

internal class StringConverter : ArgumentConverter
{
    public static readonly StringConverter Instance = new();

    public override object? Convert(string value, CultureInfo culture) => value;
}
