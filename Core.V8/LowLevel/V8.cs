using System;
using Coplt.V8Core.LowLevel.Gen;
using System.Runtime.CompilerServices;
using System.Text;

namespace Coplt.V8Core.LowLevel;

public static unsafe partial class V8
{
    internal static readonly RootVTable* RootVTable = NativeMethods.coplt_v8core_get_root_vtable();
    internal static readonly V8VTable* V8VTable = RootVTable->v8;
    internal static readonly PlatformVTable* PlatformVTable = RootVTable->platform;
    internal static readonly IsolateVTable* IsolateVTable = RootVTable->isolate;
    internal static readonly HandleScopeVTable* HandleScopeVTable = RootVTable->handle_scope;
    internal static readonly ContextVTable* ContextVTable = RootVTable->context;
    internal static readonly ContextScopeVTable* ContextScopeVTable = RootVTable->context_scope;
    internal static readonly ScriptVTable* ScriptVTable = RootVTable->script;
    internal static readonly ValueVTable* ValueVTable = RootVTable->value;
    internal static readonly StringVTable* StringVTable = RootVTable->@string;

    /// <summary>
    /// Manually init
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InitializePlatform(Platform platform)
    {
        var cloned = PlatformVTable->clone(platform.ptr);
        V8VTable->initialize_platform(cloned);
    }

    /// <summary>
    /// Manually init
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Initialize()
    {
        V8VTable->initialize();
    }

    /// <summary>
    /// Use this to auto-init or manually call InitializePlatform and then call Initialize
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AutoEnsuresInit()
    {
        V8VTable->auto_ensures_init();
    }

    /// <summary>
    /// Check if initialized
    /// </summary>
    public static bool IsInitialized
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => V8VTable->is_initialized();
    }

    /// <summary>
    /// Guaranteed to be initialized
    /// </summary>
    /// <exception cref="V8UninitializedException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AssertInitialized()
    {
        if (!IsInitialized) throw new V8UninitializedException();
    }

    /// <summary>
    /// Get the v8 version
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> GetVersionChar()
    {
        var slice = V8VTable->version();
        return new ReadOnlySpan<byte>(slice.ptr, (int)slice.len);
    }

    private static readonly Lazy<string> _version = new(() => Encoding.UTF8.GetString(GetVersionChar()));

    /// <summary>
    /// Get the v8 version
    /// </summary>
    public static string Version
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _version.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Platform GetCurrentPlatform() => new(V8VTable->current_platform());

    private static readonly Lazy<Platform> _current_platform = new(GetCurrentPlatform);

    /// <summary>
    ///Sets the v8::Platform to use. This should be invoked before V8 is initialized.
    /// </summary>
    public static Platform CurrentPlatform
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            AssertInitialized();
            return _current_platform.Value;
        }
    }

}
