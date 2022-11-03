using Ookii.CommandLine.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine
{
    internal sealed class OptionsRestorer : IDisposable
    {
        private readonly ParseOptions _options;
        private readonly VirtualTerminalSupport? _vtSupport;

        public OptionsRestorer(ParseOptions options, VirtualTerminalSupport? vtSupport)
        {
            _options = options;
            _vtSupport = vtSupport;
        }

        public bool ResetUseColor { get; set; }
        public bool ResetUseErrorColor { get; set; }
        public bool ResetOut { get; set; }
        public bool ResetCommandName { get; set; }

        public void Dispose()
        {
            _vtSupport?.Dispose();
            if (ResetUseColor)
                _options.UsageOptions.UseColor = null;

            if (ResetUseErrorColor)
                _options.UseErrorColor = null;

            if (ResetOut)
                _options.Out = null;

            if (ResetCommandName)
                _options.UsageOptions.CommandName = null;
        }
    }
}
