using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Coplt.V8Core.LowLevel.Gen;

namespace Coplt.V8Core.LowLevel;

public static partial class V8
{
    public sealed unsafe class Platform : IDisposable, IEquatable<Platform>
    {
        internal readonly PlatformOpaque ptr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Platform(PlatformOpaque ptr) => this.ptr = ptr;

        /// <summary>
        /// Create new instance of the default v8::Platform implementation
        /// </summary>
        /// <param name="thread_pool_size">is the number of worker threads to allocate for background jobs. If a value of zero is passed, a suitable default based on the current number of processors online will be chosen</param>
        /// <param name="idle_task_support"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Platform(uint thread_pool_size, bool idle_task_support)
        {
            ptr = PlatformVTable->ctor(thread_pool_size, idle_task_support);
        }

        /// <summary>
        /// Create new instance of the default v8::Platform implementation and disables the worker thread pool
        /// <para>It must be used with the –single-threaded V8 flag.</para>
        /// </summary>
        /// <param name="idle_task_support"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Platform(bool idle_task_support)
        {
            ptr = PlatformVTable->ctor_single_threaded(idle_task_support);
        }

        /// <summary>
        /// shared_ptr add ref
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Platform Clone()
        {
            var n_ptr = PlatformVTable->clone(ptr);
            return new Platform(n_ptr);
        }

        #region Dispose

        private int disposed;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            if (Interlocked.Exchange(ref disposed, 1) != 0) return;
            PlatformVTable->drop(ptr);
        }

        ~Platform() => Dispose();

        public bool Equals(Platform? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ptr.Equals(other.ptr);
        }

        #endregion

        #region Equals

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is Platform other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ptr.GetHashCode();
        }

        public static bool operator ==(Platform? left, Platform? right) => EqualityComparer<Platform>.Default.Equals(left!, right!);

        public static bool operator !=(Platform? left, Platform? right) => !(left == right);

        #endregion
    }

}
