use crate::{
    impl_transmute_cast, HandleScopeObject, HandleScopeOpaque, IsolateOpaque, LocalContextOpaque,
    HANDLE_SCOPE_ISOLATE_VTABLE,
};
use std::ops::DerefMut;

#[repr(C)]
pub struct ContextScopeOpaque(usize);

impl_transmute_cast! { ContextScopeOpaque as v8::ContextScope<'static, v8::HandleScope<'static>> }

#[repr(C)]
pub struct ContextScopeVTable {
    ctor_isolate: unsafe extern "C" fn(
        scope: *mut HandleScopeOpaque,
        ctx: LocalContextOpaque,
    ) -> ContextScopeOpaque,
    isolate: *const ContextScopeImplVTable,
}

#[repr(C)]
pub struct ContextScopeImplVTable {
    drop: unsafe extern "C" fn(ptr: ContextScopeOpaque),
    deref_to_isolate: unsafe extern "C" fn(ptr: *mut ContextScopeOpaque) -> *mut IsolateOpaque,
    deref_to_isolate_scope: unsafe extern "C" fn(ptr: *mut ContextScopeOpaque) -> HandleScopeObject,
    deref_to_context_scope: unsafe extern "C" fn(ptr: *mut ContextScopeOpaque) -> HandleScopeObject,
}

pub const CONTEXT_SCOPE_VTABLE: ContextScopeVTable = ContextScopeVTable {
    ctor_isolate: impls::context_scope_isolate_new,
    isolate: &CONTEXT_SCOPE_IMPL_VTABLE_ISOLATE,
};

pub const CONTEXT_SCOPE_IMPL_VTABLE_ISOLATE: ContextScopeImplVTable = ContextScopeImplVTable {
    drop: impls::context_scope_isolate_drop,
    deref_to_isolate: impls::context_scope_isolate_deref_to_isolate,
    deref_to_isolate_scope: impls::context_scope_isolate_deref_to_isolate_scope,
    deref_to_context_scope: impls::context_scope_isolate_deref_to_context_scope,
};

mod impls {
    use super::*;

    pub unsafe extern "C" fn context_scope_isolate_new(
        scope: *mut HandleScopeOpaque,
        ctx: LocalContextOpaque,
    ) -> ContextScopeOpaque {
        let ctx_scope: v8::ContextScope<'static, v8::HandleScope<'static>> = v8::ContextScope::new(
            &mut *(scope as *mut v8::HandleScope<'static, ()>),
            ctx.into(),
        );
        ctx_scope.into()
    }

    pub unsafe extern "C" fn context_scope_isolate_drop(ptr: ContextScopeOpaque) {
        let ctx_scope: v8::ContextScope<'static, v8::HandleScope<'static>> = ptr.into();
        drop(ctx_scope)
    }

    pub unsafe extern "C" fn context_scope_isolate_deref_to_isolate(
        ptr: *mut ContextScopeOpaque,
    ) -> *mut IsolateOpaque {
        let ctx_scope = &mut *(ptr as *mut v8::ContextScope<'static, v8::HandleScope<'static>>);
        let scope: &mut v8::Isolate = ctx_scope.deref_mut().deref_mut().deref_mut();
        scope as *mut _ as *mut IsolateOpaque
    }

    pub unsafe extern "C" fn context_scope_isolate_deref_to_isolate_scope(
        ptr: *mut ContextScopeOpaque,
    ) -> HandleScopeObject {
        let ctx_scope = &mut *(ptr as *mut v8::ContextScope<'static, v8::HandleScope<'static>>);
        let scope: &mut v8::HandleScope<'static, ()> = ctx_scope.deref_mut().deref_mut();
        HandleScopeObject {
            ptr: scope as *mut _ as *mut HandleScopeOpaque,
            vt: &HANDLE_SCOPE_ISOLATE_VTABLE,
        }
    }

    pub unsafe extern "C" fn context_scope_isolate_deref_to_context_scope(
        ptr: *mut ContextScopeOpaque,
    ) -> HandleScopeObject {
        let ctx_scope = &mut *(ptr as *mut v8::ContextScope<'static, v8::HandleScope<'static>>);
        let scope: &mut v8::HandleScope<'static> = ctx_scope.deref_mut();
        HandleScopeObject {
            ptr: scope as *mut _ as *mut HandleScopeOpaque,
            vt: &HANDLE_SCOPE_ISOLATE_VTABLE,
        }
    }
}
