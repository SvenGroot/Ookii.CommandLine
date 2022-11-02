using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Validation
{
    public class RequiresAnyAttribute : ClassValidationAttribute
    {
        private string[] _requiredArguments;

        public RequiresAnyAttribute(string requiredArgument1, string requiredArgument2)
        {
            if (requiredArgument1 == null)
                throw new ArgumentNullException(nameof(requiredArgument1));

            if (requiredArgument2 == null)
                throw new ArgumentNullException(nameof(requiredArgument2));

            // This constructor exists to avoid a warning about non-CLS compliant types.
            _requiredArguments = new[] { requiredArgument1, requiredArgument2 };
        }

        public RequiresAnyAttribute(params string[] requiredArguments)
        {
            if (_requiredArguments == null)
                throw new ArgumentNullException(nameof(requiredArguments));

            // TODO: Error message.
            if (_requiredArguments.Length < 1)
                throw new ArgumentException(null, nameof(requiredArguments));

            _requiredArguments = requiredArguments;
        }

        public override CommandLineArgumentErrorCategory ErrorCategory
            => CommandLineArgumentErrorCategory.MissingRequiredArgument;

        public override bool IsValid(CommandLineParser parser)
            => _requiredArguments.Any(name => parser.GetArgument(name)?.HasValue ?? false);

        // TODO: Override GetErrorMessage
    }
}
