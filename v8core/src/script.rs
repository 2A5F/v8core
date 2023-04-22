use crate::{impl_transmute_cast, HandleScopeOpaque, LocalStringOpaque, LocalValueOpaque};

#[repr(C)]
pub struct LocalScriptOpaque(usize);

impl_transmute_cast! { LocalScriptOpaque as v8::Local<'static, v8::Script> }

#[repr(C)]
pub struct ScriptVTable {
    ctor_compile: unsafe extern "C" fn(
        scope: *mut HandleScopeOpaque,
        source: LocalStringOpaque,
        ret: *mut LocalScriptOpaque,
    ) -> bool,
    run: unsafe extern "C" fn(
        ptr: LocalScriptOpaque,
        scope: *mut HandleScopeOpaque,
        ret: *mut LocalValueOpaque,
    ) -> bool,
}

pub const SCRIPT_VTABLE: ScriptVTable = ScriptVTable {
    ctor_compile: impls::script_compile,
    run: impls::script_run,
};

mod impls {
    use super::*;

    pub unsafe extern "C" fn script_compile(
        scope: *mut HandleScopeOpaque,
        source: LocalStringOpaque,
        ret: *mut LocalScriptOpaque,
    ) -> bool {
        let scope = &mut *(scope as *mut v8::HandleScope<'static>);
        let source = source.into();
        let s = v8::Script::compile(scope, source, None);
        match s {
            Some(v) => {
                *ret = v.into();
                true
            }
            None => false,
        }
    }

    pub unsafe extern "C" fn script_run(
        ptr: LocalScriptOpaque,
        scope: *mut HandleScopeOpaque,
        ret: *mut LocalValueOpaque,
    ) -> bool {
        let script: v8::Local<'static, v8::Script> = ptr.into();
        let scope = &mut *(scope as *mut v8::HandleScope<'static>);
        let r = script.run(scope);
        match r {
            Some(v) => {
                *ret = v.into();
                true
            }
            None => false,
        }
    }
}
