using System;
using System.Collections.Generic;
using System.Text;

namespace Coplt.V8Core.LowLevel;

[Flags]
public enum WriteOptions
{
    NoOptions= 0,
    HintManyWritesExpected = 1 << 0,
    NoNullTermination = 1 << 1,
    PreserveOneByteNull = 1 << 2,
    ReplaceInvalidUtf8 = 1 << 3,
}
