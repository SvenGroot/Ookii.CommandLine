using System;
using System.Text;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Extension methods for the <see cref="NameTransform"/> enumeration.
    /// </summary>
    public static class NameTransformExtensions
    {
        /// <summary>
        /// Applies the specified transformation to a name.
        /// </summary>
        /// <param name="transform">The transformation to apply.</param>
        /// <param name="name">The name to transform.</param>
        /// <param name="suffixToStrip">
        ///   An optional suffix to remove from the string before transformation. Only used if
        ///   <paramref name="suffixToStrip"/> is not <see cref="NameTransform.None"/>.
        /// </param>
        /// <returns>The transformed name.</returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        public static string Apply(this NameTransform transform, string name, string? suffixToStrip = null)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            int count = name.Length;
            if (transform != NameTransform.None && suffixToStrip != null && name.EndsWith(suffixToStrip))
            {
                count = name.Length - suffixToStrip.Length;
            }

            return transform switch
            {
                NameTransform.PascalCase => ToPascalOrCamelCase(name, true, count),
                NameTransform.CamelCase => ToPascalOrCamelCase(name, false, count),
                NameTransform.SnakeCase => ToSnakeOrDashCase(name, '_', count),
                NameTransform.DashCase => ToSnakeOrDashCase(name, '-', count),
                _ => name,
            };
        }

        private static string ToPascalOrCamelCase(string name, bool pascalCase, int count)
        {
            // Remove any underscores, and the first letter (if pascal case) and any letter after an
            // underscore is converted to uppercase. Other letters are unchanged.
            var toUpper = pascalCase;
            var toLower = !pascalCase; // Only for the first character.
            var first = true;
            var builder = new StringBuilder(name.Length);
            for (int i = 0; i < count; i++)
            {
                var ch = name[i];
                if (ch == '_')
                {
                    toUpper = !first || pascalCase;
                    continue;
                }
                else if (!char.IsLetter(ch))
                {
                    // Also uppercase/lowercase after non-letters.
                    builder.Append(ch);
                    toUpper = pascalCase;
                    toLower = !pascalCase;
                    continue;
                }

                first = false;
                if (toUpper)
                {
                    builder.Append(char.ToUpperInvariant(ch));
                    toUpper = false;
                }
                else if (toLower)
                {
                    builder.Append(char.ToLowerInvariant(ch));
                    toLower = false;
                }
                else
                {
                    builder.Append(ch);
                }
            }

            return builder.ToString();
        }

        private static string ToSnakeOrDashCase(string name, char separator, int count)
        {
            var needSeparator = false;
            var first = true;
            // Add some leeway to add separators.
            var builder = new StringBuilder(name.Length * 2);
            for (int i = 0; i < count; ++i)
            {
                var ch = name[i];
                if (ch == '_')
                {
                    needSeparator = !first;
                }
                else if (!char.IsLetter(ch))
                {
                    needSeparator = false;
                    first = true;
                    builder.Append(ch);
                }
                else
                {
                    if (needSeparator || (char.IsUpper(ch) && !first))
                    {
                        builder.Append(separator);
                        needSeparator = false;
                    }

                    builder.Append(char.ToLowerInvariant(ch));
                    first = false;
                }
            }

            return builder.ToString();
        }
    }
}
