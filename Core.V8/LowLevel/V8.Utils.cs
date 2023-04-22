using Coplt.V8Core.LowLevel.Gen;
using System;
using System.Runtime.CompilerServices;

namespace Coplt.V8Core.LowLevel;

public static unsafe partial class V8
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool? ToBool(this OptionBool self) => self switch
    {
        OptionBool.None => null,
        OptionBool.False => false,
        OptionBool.True => true,
        _ => throw new ArgumentOutOfRangeException()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static OptionBool ToOptionBool(this bool? self) => self switch
    {
        null => OptionBool.None,
        false => OptionBool.False,
        true => OptionBool.True,
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static OptionBool ToOptionBool(this bool self) => self switch
    {
        false => OptionBool.False,
        true => OptionBool.True,
    };

}
