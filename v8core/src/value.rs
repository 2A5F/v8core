use crate::{impl_transmute_cast, HandleScopeOpaque, LocalStringOpaque};
use std::{ops::Deref, ffi::c_void};

#[repr(C)]
pub struct ValueOpaque;

#[repr(C)]
pub struct LocalValueOpaque(*mut c_void);

impl_transmute_cast! { LocalValueOpaque as v8::Local<'static, v8::Value> }

#[repr(C)]
pub struct ValueVTable {
    deref: unsafe extern "C" fn(ptr: LocalValueOpaque) -> *const ValueOpaque,

    // type check
    type_of: unsafe extern "C" fn(
        ptr: *const ValueOpaque,
        scope: *mut HandleScopeOpaque,
    ) -> LocalStringOpaque,
    is_undefined: unsafe extern "C" fn(ptr: *const ValueOpaque) -> bool,
    is_null: unsafe extern "C" fn(ptr: *const ValueOpaque) -> bool,
    is_null_or_undefined: unsafe extern "C" fn(ptr: *const ValueOpaque) -> bool,
    is_true: unsafe extern "C" fn(ptr: *const ValueOpaque) -> bool,
    is_false: unsafe extern "C" fn(ptr: *const ValueOpaque) -> bool,
    is_name: unsafe extern "C" fn(ptr: *const ValueOpaque) -> bool,
    is_string: unsafe extern "C" fn(ptr: *const ValueOpaque) -> bool,
    is_symbol: unsafe extern "C" fn(ptr: *const ValueOpaque) -> bool,

    // cast
    to_string: unsafe extern "C" fn(
        ptr: *const ValueOpaque,
        scope: *mut HandleScopeOpaque,
        ret: *mut LocalStringOpaque,
    ) -> bool,
}

pub const VALUE_VTABLE: ValueVTable = ValueVTable {
    deref: impls::value_deref,

    // type check
    type_of: impls::type_check::type_of,
    is_undefined: impls::type_check::is_undefined,
    is_null: impls::type_check::is_null,
    is_null_or_undefined: impls::type_check::is_null_or_undefined,
    is_true: impls::type_check::is_true,
    is_false: impls::type_check::is_false,
    is_name: impls::type_check::is_name,
    is_string: impls::type_check::is_string,
    is_symbol: impls::type_check::is_symbol,

    // cast
    to_string: impls::cast::to_string,
};

mod impls {
    use super::*;

    pub unsafe extern "C" fn value_deref(ptr: LocalValueOpaque) -> *const ValueOpaque {
        let val: v8::Local<'static, v8::Value> = ptr.into();
        let val: &v8::Value = val.deref();
        val as *const v8::Value as *const ValueOpaque
    }

    pub mod type_check {
        use super::*;

        pub unsafe extern "C" fn type_of(
            ptr: *const ValueOpaque,
            scope: *mut HandleScopeOpaque,
        ) -> LocalStringOpaque {
            let val = &*(ptr as *const v8::Value);
            let scope = &mut *(scope as *mut v8::HandleScope<'static>);
            let r = val.type_of(scope);
            r.into()
        }

        pub unsafe extern "C" fn is_undefined(ptr: *const ValueOpaque) -> bool {
            let val = &*(ptr as *const v8::Value);
            val.is_undefined()
        }

        pub unsafe extern "C" fn is_null(ptr: *const ValueOpaque) -> bool {
            let val = &*(ptr as *const v8::Value);
            val.is_null()
        }

        pub unsafe extern "C" fn is_null_or_undefined(ptr: *const ValueOpaque) -> bool {
            let val = &*(ptr as *const v8::Value);
            val.is_null_or_undefined()
        }

        pub unsafe extern "C" fn is_true(ptr: *const ValueOpaque) -> bool {
            let val = &*(ptr as *const v8::Value);
            val.is_true()
        }

        pub unsafe extern "C" fn is_false(ptr: *const ValueOpaque) -> bool {
            let val = &*(ptr as *const v8::Value);
            val.is_false()
        }

        pub unsafe extern "C" fn is_name(ptr: *const ValueOpaque) -> bool {
            let val = &*(ptr as *const v8::Value);
            val.is_name()
        }

        pub unsafe extern "C" fn is_string(ptr: *const ValueOpaque) -> bool {
            let val = &*(ptr as *const v8::Value);
            val.is_string()
        }

        pub unsafe extern "C" fn is_symbol(ptr: *const ValueOpaque) -> bool {
            let val = &*(ptr as *const v8::Value);
            val.is_symbol()
        }
    }

    pub mod cast {
        use super::*;

        pub unsafe extern "C" fn to_string(
            ptr: *const ValueOpaque,
            scope: *mut HandleScopeOpaque,
            ret: *mut LocalStringOpaque,
        ) -> bool {
            let val = &*(ptr as *const v8::Value);
            let scope = &mut *(scope as *mut v8::HandleScope<'static>);
            let r = val.to_string(scope);
            match r {
                Some(v) => {
                    *ret = v.into();
                    true
                }
                None => false,
            }
        }
    }
}
