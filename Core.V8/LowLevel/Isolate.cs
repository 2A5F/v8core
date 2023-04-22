using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Coplt.V8Core.LowLevel.Gen;

namespace Coplt.V8Core.LowLevel;

public sealed unsafe class Isolate : IDisposable
{
    internal OwnedIsolateOpaque ptr;
    internal Thread currentThread;

    #region Ctor

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Isolate()
    {
        V8.AssertInitialized();
        ptr = V8.IsolateVTable->ctor_default();
        currentThread = Thread.CurrentThread;
    }

    public struct CreateParams
    {
        internal IsolateCreateParams _params;

        /// <summary>
        /// Enables the host application to provide a mechanism for recording statistics counters.
        /// </summary>
        /// <param name="cb"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CounterLookupCallbackUnsafe(delegate* unmanaged[Cdecl]<byte*, int*> cb)
        {
            _params.counter_lookup_callback = cb;
        }

        /// <summary>
        /// Whether calling Atomics.wait (a function that may block) is allowed in this isolate. This can also be configured via SetAllowAtomicsWait.
        /// </summary>
        public bool? AllowAtomicsWait
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _params.allow_atomics_wait.ToBool();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _params.allow_atomics_wait = value.ToOptionBool();
        }

        /// <summary>
        /// Termination is postponed when there is no active SafeForTerminationScope.
        /// </summary>
        public bool? OnlyTerminateInSafeScope
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _params.only_terminate_in_safe_scope.ToBool();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _params.only_terminate_in_safe_scope = value.ToOptionBool();
        }

        /// <summary>
        /// The following parameters describe the offsets for addressing type info for wrapped API objects and are used by the fast C API (for details see v8-fast-api-calls.h).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EmbedderWrapperTypeInfoOffsets(int embedder_wrapper_type_index, int embedder_wrapper_object_index)
        {
            _params.set_embedder_wrapper_type_info_offsets = true;
            _params.embedder_wrapper_type_index = embedder_wrapper_type_index;
            _params.embedder_wrapper_object_index = embedder_wrapper_object_index;
        }

        /// <summary>
        /// Configures the constraints with reasonable default values based on the provided lower and upper bounds.
        /// <para>By default V8 starts with a small heap and dynamically grows it to match the set of live objects. This may lead to ineffective garbage collections at startup if the live set is large. Setting the initial heap size avoids such garbage collections. Note that this does not affect young generation garbage collections.</para>
        /// <para>When the heap size approaches max, V8 will perform series of garbage collections and invoke the NearHeapLimitCallback. If the garbage collections do not help and the callback does not increase the limit, then V8 will crash with V8::FatalProcessOutOfMemory.The heap size includes both the young and the old generation.</para>
        /// <para>The heap size includes both the young and the old generation.</para>
        /// </summary>
        /// <param name="initial">The initial heap size or zero in bytes</param>
        /// <param name="max">The hard limit for the heap size in bytes</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HeapLimits(int initial, int max) => HeapLimits((nuint)initial, (nuint)max);

        /// <summary>
        /// Configures the constraints with reasonable default values based on the provided lower and upper bounds.
        /// <para>By default V8 starts with a small heap and dynamically grows it to match the set of live objects. This may lead to ineffective garbage collections at startup if the live set is large. Setting the initial heap size avoids such garbage collections. Note that this does not affect young generation garbage collections.</para>
        /// <para>When the heap size approaches max, V8 will perform series of garbage collections and invoke the NearHeapLimitCallback. If the garbage collections do not help and the callback does not increase the limit, then V8 will crash with V8::FatalProcessOutOfMemory.The heap size includes both the young and the old generation.</para>
        /// <para>The heap size includes both the young and the old generation.</para>
        /// </summary>
        /// <param name="initial">The initial heap size or zero in bytes</param>
        /// <param name="max">The hard limit for the heap size in bytes</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HeapLimits(nuint initial, nuint max)
        {
            _params.set_heap_limits = true;
            _params.heap_limits_initial = initial;
            _params.heap_limits_max = max;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Isolate(CreateParams createParams)
    {
        V8.AssertInitialized();
        ptr = V8.IsolateVTable->ctor(createParams._params);
        currentThread = Thread.CurrentThread;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Isolate CreateOnCurrentThread() => new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Isolate CreateOnCurrentThread(CreateParams createParams) => new(createParams);

    #endregion

    #region Dispose

    private int disposed;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (Interlocked.Exchange(ref disposed, 1) != 0) return;
        V8.IsolateVTable->drop(ptr);
    }

    ~Isolate() => Dispose();

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal IsolateOpaque* DerefPtr()
    {
        fixed (OwnedIsolateOpaque* p = &ptr)
        {
            return V8.IsolateVTable->deref(p);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IsolateRef AsRef() => new(DerefPtr());
}

public unsafe ref struct IsolateRef
{
    internal readonly IsolateOpaque* ptr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal IsolateRef(IsolateOpaque* ptr)
    {
        this.ptr = ptr;
    }
}

