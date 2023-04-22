using System;

namespace Coplt.V8Core.LowLevel;

public class V8UninitializedException : Exception
{
    public V8UninitializedException() { }

    public V8UninitializedException(string message) : base(message) { }

    public V8UninitializedException(string message, Exception inner) : base(message, inner) { }
}