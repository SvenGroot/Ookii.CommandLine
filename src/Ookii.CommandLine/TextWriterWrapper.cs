using System;
using System.IO;

namespace Ookii.CommandLine
{
    // .Net Framework 2.0 does not have Func<T>.
    internal delegate TextWriter CreateDelegate();

    internal class TextWriterWrapper : IDisposable
    {
        private readonly TextWriter _writer;
        private bool _needDispose;

        public TextWriterWrapper(TextWriter? stream, CreateDelegate createIfNull)
        {
            if (stream == null)
            {
                _writer = createIfNull();
                _needDispose = true;
            }
            else
            {
                _writer = stream;
            }
        }

        public TextWriter Writer => _writer;

        protected virtual void Dispose(bool disposing)
        {
            if (_needDispose)
            {
                if (disposing)
                {
                    _writer.Dispose();
                }

                _needDispose = false;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
