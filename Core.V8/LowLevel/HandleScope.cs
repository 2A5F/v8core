using System.Runtime.CompilerServices;
using System.Threading;
using Coplt.V8Core.LowLevel.Gen;

namespace Coplt.V8Core.LowLevel;


public static class HandleScope
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HandleScope<Isolate> Create(Isolate isolate)
        => new(isolate);
}

/// <summary>
/// Must use Dispose
/// </summary>
// ReSharper disable once UnusedTypeParameter
public unsafe ref struct HandleScope<T>
{
    internal HandleScopeOpaque ptr;
    internal readonly HandleScopeImplVTable* vt;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal HandleScope(Isolate isolate)
    {
        vt = V8.HandleScopeVTable->isolate;
        fixed (OwnedIsolateOpaque* isolate_ptr = &isolate.ptr)
        {
            ptr = V8.HandleScopeVTable->ctor_isolate(isolate_ptr);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal HandleScope(HandleScopeOpaque ptr, HandleScopeImplVTable* vt)
    {
        this.ptr = ptr;
        this.vt = vt;
    }

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
    internal static IsolateOpaque* DerefToIsolate(this HandleScope<Isolate> self)
        => HandleScopeVTable->deref_to_isolate(&self.ptr);
}

public static unsafe partial class V8
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IsolateRef AsIsolate(this HandleScope<Isolate> self)
        => new(self.DerefToIsolate());
}
