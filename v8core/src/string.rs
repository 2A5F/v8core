use crate::{impl_transmute_cast, CharSlice, CharSliceMut, HandleScopeOpaque, IsolateOpaque};
use std::ffi::{c_int, c_void};

#[repr(C)]
pub struct LocalStringOpaque(*mut c_void);

impl_transmute_cast! { LocalStringOpaque as v8::Local<'static, v8::String> }

#[repr(C)]
pub struct StringVTable {
    ctor_utf16: unsafe extern "C" fn(
        scope: *mut HandleScopeOpaque,
        buffer: *const CharSlice,
        ret: *mut LocalStringOpaque,
    ) -> bool,
    len: unsafe extern "C" fn(ptr: LocalStringOpaque) -> usize,
    read_utf16: unsafe extern "C" fn(
        ptr: LocalStringOpaque,
        scope: *mut IsolateOpaque,
        buffer: *mut CharSliceMut,
        start: usize,
        options: c_int,
    ) -> usize,
}

pub const STRING_VTABLE: StringVTable = StringVTable {
    ctor_utf16: impls::string_new_utf16,
    len: impls::string_len,
    read_utf16: impls::string_read_utf16,
};

mod impls {
    use super::*;

    pub unsafe extern "C" fn string_new_utf16(
        scope: *mut HandleScopeOpaque,
        buffer: *const CharSlice,
        ret: *mut LocalStringOpaque,
    ) -> bool {
        let scope = &mut *(scope as *mut v8::HandleScope<'static, ()>);

        let s = v8::String::new_external_twobyte_static(scope, &*buffer);
        match s {
            Some(v) => {
                *ret = v.into();
                true
            }
            None => false,
        }
    }

    pub unsafe extern "C" fn string_len(ptr: LocalStringOpaque) -> usize {
        let s: v8::Local<'static, v8::String> = ptr.into();
        s.length()
    }

    pub unsafe extern "C" fn string_read_utf16(
        ptr: LocalStringOpaque,
        scope: *mut IsolateOpaque,
        buffer: *mut CharSliceMut,
        start: usize,
        options: c_int,
    ) -> usize {
        let s: v8::Local<'static, v8::String> = ptr.into();
        let scope = &mut *(scope as *mut v8::Isolate);
        let options = v8::WriteOptions::from_bits(options).unwrap_or_default();

        s.write(scope, &mut *buffer, start, options)
    }
}
