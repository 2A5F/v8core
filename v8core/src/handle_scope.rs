use crate::{impl_transmute_cast, IsolateOpaque, OwnedIsolateOpaque};
use std::ops::DerefMut;

#[repr(C)]
pub struct HandleScopeOpaque(usize);

impl_transmute_cast! { HandleScopeOpaque as v8::HandleScope<'static, ()> }

#[repr(C)]
pub struct HandleScopeVTable {
    ctor_isolate: unsafe extern "C" fn(ptr: *mut OwnedIsolateOpaque) -> HandleScopeOpaque,
    deref_to_isolate: unsafe extern "C" fn(ptr: *mut HandleScopeOpaque) -> *mut IsolateOpaque,
    isolate: *const HandleScopeImplVTable,
}

#[repr(C)]
pub struct HandleScopeObject {
    pub ptr: *mut HandleScopeOpaque,
    pub vt: *const HandleScopeImplVTable,
}

#[repr(C)]
pub struct HandleScopeImplVTable {
    drop: unsafe extern "C" fn(ptr: HandleScopeOpaque),
}

pub const HANDLE_SCOPE_VTABLE: HandleScopeVTable = HandleScopeVTable {
    ctor_isolate: impls::handle_scope_isolate_new,
    deref_to_isolate: impls::handle_scope_isolate_deref_to_isolate,
    isolate: &HANDLE_SCOPE_ISOLATE_VTABLE,
};

pub const HANDLE_SCOPE_ISOLATE_VTABLE: HandleScopeImplVTable = HandleScopeImplVTable {
    drop: impls::handle_scope_isolate_drop,
};

mod impls {
    use super::*;

    pub unsafe extern "C" fn handle_scope_isolate_drop(ptr: HandleScopeOpaque) {
        let scope: v8::HandleScope<'static, ()> = ptr.into();
        drop(scope)
    }

    pub unsafe extern "C" fn handle_scope_isolate_new(
        isolate: *mut OwnedIsolateOpaque,
    ) -> HandleScopeOpaque {
        let isolate = &mut *(isolate as *mut v8::OwnedIsolate);
        let scope = v8::HandleScope::new(isolate);
        scope.into()
    }

    pub unsafe extern "C" fn handle_scope_isolate_deref_to_isolate(
        ptr: *mut HandleScopeOpaque,
    ) -> *mut IsolateOpaque {
        let scope = &mut *(ptr as *mut v8::HandleScope<'static, ()>);
        let scope: &mut v8::Isolate = scope.deref_mut();
        scope as *mut _ as *mut IsolateOpaque
    }
}
