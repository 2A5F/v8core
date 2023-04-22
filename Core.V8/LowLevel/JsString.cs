using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Coplt.V8Core.LowLevel.Gen;

namespace Coplt.V8Core.LowLevel;

public unsafe ref struct LocalJsString
{
    internal LocalStringOpaque ptr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal LocalJsString(LocalStringOpaque ptr) => this.ptr = ptr;

    #region TryCreateUtf16

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryCreateUtf16(HandleScope<Isolate> scope, string str, out LocalJsString res)
    {
        ReadOnlySpan<char> span = str;
        fixed (char* slice_start = &span[0])
        {
            var slice = new CharSlice { ptr = (ushort*)slice_start, len = (nuint)span.Length };
            LocalStringOpaque ptr = default;
            var r = V8.StringVTable->ctor_utf16(&scope.ptr, &slice, &ptr);
            if (r) res = new(ptr);
            else res = default;
            return r;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryCreateUtf16(ContextScope scope, string str, out LocalJsString res)
        => TryCreateUtf16(scope.AsIsolateScope(), str, out res);

    #endregion

    #region CreateUtf16

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalJsString CreateUtf16(HandleScope<Isolate> scope, string str)
        => TryCreateUtf16(scope, str, out var res) ? res : throw new CreateJsStringFailedException();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalJsString CreateUtf16(ContextScope scope, string str)
        => CreateUtf16(scope.AsIsolateScope(), str);

    #endregion

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (int)this.GetLength();
    }
}

public class CreateJsStringFailedException : Exception
{
    public CreateJsStringFailedException() { }

    public CreateJsStringFailedException(string message) : base(message) { }

    public CreateJsStringFailedException(string message, Exception inner) : base(message, inner) { }
}

public static unsafe partial class V8
{
    #region GetLength

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint GetLength(this LocalJsString self) => StringVTable->len(self.ptr);

    #endregion

    #region ToString

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToString(this LocalJsString self, IsolateRef scope, WriteOptions options = WriteOptions.NoOptions)
        => string.Create(self.Length, (self.ptr, scope: (IntPtr)scope.ptr, options), static (span, data) =>
        {
            var ptr = data.ptr;
            var scope = (IsolateOpaque*)data.scope;
            var options = data.options;
            fixed (char* slice_start = &span[0])
            {
                var buffer = new CharSliceMut
                {
                    ptr = (ushort*)slice_start,
                    len = (nuint)span.Length,
                };
                StringVTable->read_utf16(ptr, scope, &buffer, 0, (int)options);
            }
        });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToString(this LocalJsString self, HandleScope<Isolate> scope, WriteOptions options = WriteOptions.NoOptions)
        => self.ToString(scope.AsIsolate(), options);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToString(this LocalJsString self, ContextScope scope, WriteOptions options = WriteOptions.NoOptions)
        => self.ToString(scope.AsIsolate(), options);

    #endregion

    #region ReadUtf16

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint ReadUtf16(this LocalJsString self, IsolateRef scope, Span<char> buffer, WriteOptions options = WriteOptions.NoOptions)
    {
        fixed (char* slice_start = &buffer[0])
        {
            var slice = new CharSliceMut
            {
                ptr = (ushort*)slice_start,
                len = (nuint)buffer.Length,
            };
            return StringVTable->read_utf16(self.ptr, scope.ptr, &slice, 0, (int)options);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint ReadUtf16(this LocalJsString self, HandleScope<Isolate> scope, Span<char> buffer, WriteOptions options = WriteOptions.NoOptions)
        => self.ReadUtf16(scope.AsIsolate(), buffer, options);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint ReadUtf16(this LocalJsString self, ContextScope scope, Span<char> buffer, WriteOptions options = WriteOptions.NoOptions)
        => self.ReadUtf16(scope.AsIsolate(), buffer, options);

    #endregion
}
