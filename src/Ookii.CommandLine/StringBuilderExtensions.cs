#if NETSTANDARD2_0

using System.Xml.Linq;
using System;
using System.Text;
using System.Collections.Generic;

namespace Ookii.CommandLine;

static class StringBuilderExtensions
{
    // This already exists in all targets except .Net Standard 2.0
    public static void AppendJoin(this StringBuilder builder, string separator, IEnumerable<string> values)
    {
        bool first = true;
        foreach (var value in values)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                builder.Append(separator);
            }

            builder.Append(value);
        }
    }
}

#endif
