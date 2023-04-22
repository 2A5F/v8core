using System.Runtime.CompilerServices;
using System.Threading;
using Coplt.V8Core.LowLevel.Gen;

namespace Coplt.V8Core.LowLevel;

/// <summary>
/// Must use Dispose
/// </summary>
public unsafe ref struct ContextScope
{
    internal ContextScopeOpaque ptr;
    internal readonly ContextScopeImplVTable* vt;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ContextScope(HandleScope<Isolate> scope, LocalContext ctx)
    {
        vt = V8.ContextScopeVTable->isolate;
        ptr = V8.ContextScopeVTable->ctor_isolate(&scope.ptr, ctx.ptr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ContextScope Create(HandleScope<Isolate> scope, LocalContext ctx)
        => new(scope, ctx);

    #region Dispose

    private int disposed;

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0) return;
        vt->drop(ptr);
    }

    #endregion
}

public static unsafe partial class V8
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IsolateOpaque* DerefToIsolatePtr(this ContextScope self) => self.vt->deref_to_isolate(&self.ptr);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static HandleScopeObject DerefToIsolateScopePtr(this ContextScope self) => self.vt->deref_to_isolate_scope(&self.ptr);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static HandleScopeObject DerefToContextScopePtr(this ContextScope self) => self.vt->deref_to_context_scope(&self.ptr);
}

public static unsafe partial class V8
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IsolateRef AsIsolate(this ContextScope self) => new(self.DerefToIsolatePtr());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HandleScope<Isolate> AsIsolateScope(this ContextScope self)
    {
        var obj = self.DerefToIsolateScopePtr();
        return new(*obj.ptr, obj.vt);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HandleScope<Context> AsHandleScope(this ContextScope self)
    {
        var obj = self.DerefToContextScopePtr();
        return new(*obj.ptr, obj.vt);
    }
}
