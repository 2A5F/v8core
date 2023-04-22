use crate::{impl_transmute_cast, OptionBool};
use std::ffi::{c_char, c_int};

#[repr(C)]
pub struct IsolateOpaque;

#[repr(C)]
pub struct OwnedIsolateOpaque(usize);

impl_transmute_cast! { OwnedIsolateOpaque as v8::OwnedIsolate }

#[repr(C)]
pub struct IsolateCreateParams {
    counter_lookup_callback: Option<extern "C" fn(name: *const c_char) -> *mut i32>,
    allow_atomics_wait: OptionBool,
    only_terminate_in_safe_scope: OptionBool,
    set_embedder_wrapper_type_info_offsets: bool,
    embedder_wrapper_type_index: c_int,
    embedder_wrapper_object_index: c_int,
    set_heap_limits: bool,
    heap_limits_initial: usize,
    heap_limits_max: usize,
}

#[repr(C)]
pub struct IsolateVTable {
    drop: unsafe extern "C" fn(ptr: OwnedIsolateOpaque),
    ctor: unsafe extern "C" fn(params: IsolateCreateParams) -> OwnedIsolateOpaque,
    ctor_default: unsafe extern "C" fn() -> OwnedIsolateOpaque,
    deref: unsafe extern "C" fn(ptr: *mut OwnedIsolateOpaque) -> *mut IsolateOpaque,
}

pub const ISOLATE_VTABLE: IsolateVTable = IsolateVTable {
    drop: impls::isolate_drop,
    ctor: impls::isolate_new,
    ctor_default: impls::isolate_new_default,
    deref: impls::isolate_deref,
};

mod impls {
    use std::ops::DerefMut;

    use super::*;

    pub unsafe extern "C" fn isolate_drop(ptr: OwnedIsolateOpaque) {
        let iso: v8::OwnedIsolate = ptr.into();
        drop(iso)
    }

    pub unsafe extern "C" fn isolate_new_default() -> OwnedIsolateOpaque {
        let iso = v8::Isolate::new(Default::default());
        iso.into()
    }

    pub unsafe extern "C" fn isolate_new(params: IsolateCreateParams) -> OwnedIsolateOpaque {
        let mut cp = v8::CreateParams::default();
        if let Some(v) = params.counter_lookup_callback {
            cp = cp.counter_lookup_callback(v);
        }
        if let Some(v) = params.allow_atomics_wait.as_opt() {
            cp = cp.allow_atomics_wait(v);
        }
        if let Some(v) = params.only_terminate_in_safe_scope.as_opt() {
            cp = cp.only_terminate_in_safe_scope(v);
        }
        if params.set_embedder_wrapper_type_info_offsets {
            cp = cp.embedder_wrapper_type_info_offsets(
                params.embedder_wrapper_type_index,
                params.embedder_wrapper_object_index,
            );
        }
        if params.set_heap_limits {
            cp = cp.heap_limits(params.heap_limits_initial, params.heap_limits_max);
        }
        let iso = v8::Isolate::new(cp);
        iso.into()
    }

    pub unsafe extern "C" fn isolate_deref(ptr: *mut OwnedIsolateOpaque) -> *mut IsolateOpaque {
        let iso = &mut *(ptr as *mut v8::OwnedIsolate);
        let iso: &mut v8::Isolate = iso.deref_mut();
        iso as *mut _ as *mut IsolateOpaque
    }
}
