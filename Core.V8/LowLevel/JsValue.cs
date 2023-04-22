using System;
using Coplt.V8Core.LowLevel.Gen;
using System.Runtime.CompilerServices;

namespace Coplt.V8Core.LowLevel;

public unsafe ref struct JsValueRef
{
    internal ValueOpaque* ptr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal JsValueRef(ValueOpaque* ptr) => this.ptr = ptr;

    public bool IsUndefined
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.GetIsUndefined();
    }

    public bool IsNull
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.GetIsNull();
    }

    public bool IsNullOrUndefined
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.GetIsNullOrUndefined();
    }

    public bool IsTrue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.GetIsTrue();
    }

    public bool IsFalse
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.GetIsFalse();
    }

    public bool IsName
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.GetIsName();
    }

    public bool IsString
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.GetIsString();
    }

    public bool IsSymbol
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.GetIsSymbol();
    }
}

public unsafe ref struct LocalJsValue
{
    internal LocalValueOpaque ptr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal LocalJsValue(LocalValueOpaque ptr) => this.ptr = ptr;

    public bool IsUndefined
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.AsRef().IsUndefined;
    }

    public bool IsNull
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.AsRef().GetIsNull();
    }

    public bool IsNullOrUndefined
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.AsRef().GetIsNullOrUndefined();
    }

    public bool IsTrue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.AsRef().GetIsTrue();
    }

    public bool IsFalse
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.AsRef().GetIsFalse();
    }

    public bool IsName
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.AsRef().GetIsName();
    }

    public bool IsString
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.AsRef().GetIsString();
    }

    public bool IsSymbol
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.AsRef().GetIsSymbol();
    }
}

public class JsValueCastToJsStringFailedException : Exception
{
    public JsValueCastToJsStringFailedException()
    {
    }

    public JsValueCastToJsStringFailedException(string message) : base(message)
    {
    }

    public JsValueCastToJsStringFailedException(string message, Exception inner) : base(message, inner)
    {
    }
}

public static unsafe partial class V8
{
    #region Deref

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ValueOpaque* DerefPtr(this LocalJsValue self) => ValueVTable->deref(self.ptr);

    #endregion
}

public static unsafe partial class V8
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsValueRef AsRef(this LocalJsValue self) => new(self.DerefPtr());

    #region TypeOf

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalJsString TypeOf(this JsValueRef self, HandleScope<Context> scope) => new(ValueVTable->type_of(self.ptr, &scope.ptr));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalJsString TypeOf(this JsValueRef self, ContextScope scope) => TypeOf(self, scope.AsHandleScope());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalJsString TypeOf(this LocalJsValue self, HandleScope<Context> scope) => TypeOf(self.AsRef(), scope);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalJsString TypeOf(this LocalJsValue self, ContextScope scope) => TypeOf(self.AsRef(), scope);

    #endregion

    #region CheckType

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetIsUndefined(this JsValueRef self) => ValueVTable->is_undefined(self.ptr);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetIsNull(this JsValueRef self) => ValueVTable->is_null(self.ptr);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetIsNullOrUndefined(this JsValueRef self) => ValueVTable->is_null_or_undefined(self.ptr);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetIsTrue(this JsValueRef self) => ValueVTable->is_true(self.ptr);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetIsFalse(this JsValueRef self) => ValueVTable->is_false(self.ptr);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetIsName(this JsValueRef self) => ValueVTable->is_name(self.ptr);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetIsString(this JsValueRef self) => ValueVTable->is_string(self.ptr);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetIsSymbol(this JsValueRef self) => ValueVTable->is_symbol(self.ptr);

    #endregion

    #region Cast

    #region TryAsJsString

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryAsJsString(this JsValueRef self, HandleScope<Context> scope, out LocalJsString res)
    {
        LocalStringOpaque ptr = default;
        var r = ValueVTable->to_string(self.ptr, &scope.ptr, &ptr);
        if (r) res = new(ptr);
        else res = default;
        return r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryAsJsString(this JsValueRef self, ContextScope scope, out LocalJsString res)
        => TryAsJsString(self, scope.AsHandleScope(), out res);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryAsJsString(this LocalJsValue self, HandleScope<Context> scope, out LocalJsString res)
        => TryAsJsString(self.AsRef(), scope, out res);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryAsJsString(this LocalJsValue self, ContextScope scope, out LocalJsString res)
        => TryAsJsString(self, scope.AsHandleScope(), out res);

    #endregion

    #region AsJsString

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalJsString AsJsString(this JsValueRef self, HandleScope<Context> scope)
        => TryAsJsString(self, scope, out var res) ? res : throw new JsValueCastToJsStringFailedException();


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalJsString AsJsString(this JsValueRef self, ContextScope scope)
        => AsJsString(self, scope.AsHandleScope());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalJsString AsJsString(this LocalJsValue self, HandleScope<Context> scope)
        => AsJsString(self.AsRef(), scope);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalJsString AsJsString(this LocalJsValue self, ContextScope scope)
        => AsJsString(self, scope.AsHandleScope());

    #endregion

    #endregion

}
