use crate::{impl_transmute_cast, HandleScopeOpaque};

#[repr(C)]
pub struct LocalContextOpaque(usize);

impl_transmute_cast! { LocalContextOpaque as v8::Local<'static, v8::Context> }

#[repr(C)]
pub struct ContextVTable {
    ctor: unsafe extern "C" fn(scope: *mut HandleScopeOpaque) -> LocalContextOpaque,
}

pub const CONTEXT_VTABLE: ContextVTable = ContextVTable {
    ctor: impls::context_new,
};

mod impls {
    use super::*;

    pub unsafe extern "C" fn context_new(scope: *mut HandleScopeOpaque) -> LocalContextOpaque {
        let scope = &mut *(scope as *mut v8::HandleScope<'static, ()>);
        let ctx = v8::Context::new(scope);
        ctx.into()
    }
}
