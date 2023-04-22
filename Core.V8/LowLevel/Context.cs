using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Coplt.V8Core.LowLevel.Gen;

namespace Coplt.V8Core.LowLevel;

public class Context
{

}

public unsafe ref struct LocalContext
{
    internal LocalContextOpaque ptr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal LocalContext(HandleScope<Isolate> scope)
    {
        ptr = V8.ContextVTable->ctor(&scope.ptr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalContext Create(HandleScope<Isolate> scope)
        => new(scope);
}
