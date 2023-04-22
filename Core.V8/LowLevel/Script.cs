using System;
using System.Runtime.CompilerServices;
using Coplt.V8Core.LowLevel.Gen;

namespace Coplt.V8Core.LowLevel;

public unsafe ref struct LocalScript
{
    internal LocalScriptOpaque ptr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal LocalScript(LocalScriptOpaque ptr) => this.ptr = ptr;

    #region TryCompile

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryCompile(HandleScope<Context> scope, LocalJsString source, out LocalScript res)
    {
        LocalScriptOpaque ptr = default;
        var r = V8.ScriptVTable->ctor_compile(&scope.ptr, source.ptr, &ptr);
        if (r) res = new(ptr);
        else res = default;
        return r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryCompile(ContextScope scope, LocalJsString source, out LocalScript res)
        => TryCompile(scope.AsHandleScope(), source, out res);

    #endregion

    #region Compile

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalScript Compile(HandleScope<Context> scope, LocalJsString source)
        => TryCompile(scope, source, out var res) ? res : throw new CompileScriptFailedException();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalScript Compile(ContextScope scope, LocalJsString source)
        => Compile(scope.AsHandleScope(), source);

    #endregion
}

#region Exception

public class CompileScriptFailedException : Exception
{
    public CompileScriptFailedException() { }

    public CompileScriptFailedException(string message) : base(message) { }

    public CompileScriptFailedException(string message, Exception inner) : base(message, inner) { }
}

public class ScriptEvalFailedException : Exception
{
    public ScriptEvalFailedException() { }

    public ScriptEvalFailedException(string message) : base(message) { }

    public ScriptEvalFailedException(string message, Exception inner) : base(message, inner) { }
}

#endregion

public static unsafe partial class V8 {

    #region TryRun

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryRun(this LocalScript self, HandleScope<Context> scope, out LocalJsValue res)
    {
        LocalValueOpaque ptr = default;
        var r = ScriptVTable->run(self.ptr, &scope.ptr, &ptr);
        if (r) res = new(ptr);
        else res = default;
        return r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryRun(this LocalScript self, ContextScope scope, out LocalJsValue res)
        => TryRun(self, scope.AsHandleScope(), out res);

    #endregion

    #region Run

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalJsValue Run(this LocalScript self, HandleScope<Context> scope)
        => TryRun(self, scope, out var res) ? res : throw new ScriptEvalFailedException();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalJsValue Run(this LocalScript self, ContextScope scope)
        => Run(self, scope.AsHandleScope());

    #endregion
}
