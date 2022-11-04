// Copyright (c) Sven Groot (Ookii.org)
using System;

namespace Ookii.CommandLine
{
    internal static class DisposableWrapper
    {
        public static DisposableWrapper<T> Create<T>(T? obj, Func<T> createIfNull)
            where T : IDisposable
        {
            return new DisposableWrapper<T>(obj, createIfNull);
        }
    }

    internal class DisposableWrapper<T> : IDisposable
        where T : IDisposable
    {
        private readonly T _inner;
        private bool _needDispose;

        public DisposableWrapper(T? inner, Func<T> createIfNull)
        {
            if (inner == null)
            {
                _inner = createIfNull();
                _needDispose = true;
            }
            else
            {
                _inner = inner;
            }
        }

        public T Inner => _inner;

        protected virtual void Dispose(bool disposing)
        {
            if (_needDispose)
            {
                if (disposing)
                {
                    _inner.Dispose();
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
