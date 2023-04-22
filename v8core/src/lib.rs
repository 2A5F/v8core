pub mod context;
pub mod context_scope;
pub mod handle_scope;
pub mod isolate;
pub mod platform;
pub mod script;
pub mod shared_ptr;
pub mod string;
pub mod value;
pub use context::*;
pub use context_scope::*;
pub use handle_scope::*;
pub use isolate::*;
pub use platform::*;
pub use script::*;
pub use shared_ptr::*;
use std::ops::{Deref, DerefMut};
pub use string::*;
pub use value::*;

#[macro_export]
macro_rules! impl_transmute_cast {
    { $src_type:ty as $tgt_type:ty } => {
        impl From<$tgt_type> for $src_type {
            fn from(value: $tgt_type) -> Self {
                unsafe { std::mem::transmute(value) }
            }
        }
        impl From<$src_type> for $tgt_type {
            fn from(value: $src_type) -> Self {
                unsafe { std::mem::transmute(value) }
            }
        }
    };
    { <$($params:tt),+> $src_type:ty as $tgt_type:ty } => {
        impl<$($params),*> From<$tgt_type> for $src_type {
            fn from(value: $tgt_type) -> Self {
                unsafe { transmute(value) }
            }
        }
        impl<$($params),*> From<$src_type> for $tgt_type {
            fn from(value: $src_type) -> Self {
                unsafe { transmute(value) }
            }
        }
    };
}

#[repr(C)]
pub struct RootVTable {
    v8: *const V8VTable,
    platform: *const PlatformVTable,
    isolate: *const IsolateVTable,
    handle_scope: *const HandleScopeVTable,
    context: *const ContextVTable,
    context_scope: *const ContextScopeVTable,
    script: *const ScriptVTable,
    value: *const ValueVTable,
    string: *const StringVTable,
}

#[repr(C)]
pub struct V8VTable {
    initialize_platform: unsafe extern "C" fn(ptr: PlatformOpaque),
    initialize: unsafe extern "C" fn(),
    auto_ensures_init: unsafe extern "C" fn(),
    is_initialized: unsafe extern "C" fn() -> bool,
    current_platform: unsafe extern "C" fn() -> PlatformOpaque,
    version: unsafe extern "C" fn() -> ByteSlice,
}

pub const V8_VTABLE: V8VTable = V8VTable {
    initialize_platform: impls::v8_initialize_platform,
    initialize: impls::v8_initialize,
    auto_ensures_init: impls::auto_ensures_init,
    is_initialized: impls::is_initialized,
    current_platform: impls::v8_get_current_platform,
    version: impls::v8_version,
};

pub const ROOT_VTABLE: RootVTable = RootVTable {
    v8: &V8_VTABLE,
    platform: &PLATFORM_VTABLE,
    isolate: &ISOLATE_VTABLE,
    handle_scope: &HANDLE_SCOPE_VTABLE,
    context: &CONTEXT_VTABLE,
    context_scope: &CONTEXT_SCOPE_VTABLE,
    script: &SCRIPT_VTABLE,
    value: &VALUE_VTABLE,
    string: &STRING_VTABLE,
};

#[no_mangle]
pub extern "C" fn coplt_v8core_get_root_vtable() -> *const RootVTable {
    &ROOT_VTABLE
}

mod impls {
    use super::*;
    use std::sync::atomic::AtomicBool;

    static INITED: AtomicBool = AtomicBool::new(false);

    pub unsafe extern "C" fn v8_initialize_platform(ptr: PlatformOpaque) {
        let shared: v8::SharedRef<v8::Platform> = ptr.into();
        v8::V8::initialize_platform(shared);
    }

    pub unsafe extern "C" fn v8_initialize() {
        INITED.store(true, std::sync::atomic::Ordering::Relaxed);
        v8::V8::initialize();
    }

    pub unsafe extern "C" fn auto_ensures_init() {
        if INITED.swap(true, std::sync::atomic::Ordering::Relaxed) {
            return;
        }
        let plt = v8::new_default_platform(0, true).make_shared();
        v8::V8::initialize_platform(plt);
        v8::V8::initialize();
    }

    pub unsafe extern "C" fn is_initialized() -> bool {
        INITED.load(std::sync::atomic::Ordering::Relaxed)
    }

    pub unsafe extern "C" fn v8_version() -> ByteSlice {
        let bytes = v8::V8::get_version().as_bytes();
        ByteSlice::new(bytes)
    }

    pub unsafe extern "C" fn v8_get_current_platform() -> PlatformOpaque {
        v8::V8::get_current_platform().into()
    }
}

#[repr(C)]
#[derive(Debug)]
pub struct ByteSlice {
    ptr: *const u8,
    len: usize,
}

impl ByteSlice {
    pub fn new(slice: &[u8]) -> Self {
        Self {
            ptr: slice.as_ptr(),
            len: slice.len(),
        }
    }
}

impl Deref for ByteSlice {
    type Target = [u8];

    fn deref(&self) -> &Self::Target {
        unsafe { std::slice::from_raw_parts(self.ptr, self.len) }
    }
}

#[repr(C)]
#[derive(Debug)]
pub struct CharSlice {
    ptr: *const u16,
    len: usize,
}

impl CharSlice {
    pub fn new(slice: &[u16]) -> Self {
        Self {
            ptr: slice.as_ptr(),
            len: slice.len(),
        }
    }
}

impl Deref for CharSlice {
    type Target = [u16];

    fn deref(&self) -> &Self::Target {
        unsafe { std::slice::from_raw_parts(self.ptr, self.len) }
    }
}

#[repr(C)]
#[derive(Debug)]
pub struct CharSliceMut {
    ptr: *mut u16,
    len: usize,
}

impl CharSliceMut {
    pub fn new(slice: &mut [u16]) -> Self {
        Self {
            ptr: slice.as_mut_ptr(),
            len: slice.len(),
        }
    }
}

impl Deref for CharSliceMut {
    type Target = [u16];

    fn deref(&self) -> &Self::Target {
        unsafe { std::slice::from_raw_parts(self.ptr, self.len) }
    }
}

impl DerefMut for CharSliceMut {
    fn deref_mut(&mut self) -> &mut Self::Target {
        unsafe { std::slice::from_raw_parts_mut(self.ptr, self.len) }
    }
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd, Eq, Ord, Hash)]
enum OptionBool {
    None = 255,
    False = 0,
    True = 1,
}

impl OptionBool {
    pub fn as_opt(self) -> Option<bool> {
        self.into()
    }
}

impl From<OptionBool> for Option<bool> {
    fn from(value: OptionBool) -> Self {
        match value {
            OptionBool::None => None,
            OptionBool::False => Some(false),
            OptionBool::True => Some(true),
        }
    }
}

impl From<Option<bool>> for OptionBool {
    fn from(value: Option<bool>) -> Self {
        match value {
            None => Self::None,
            Some(false) => Self::False,
            Some(true) => Self::True,
        }
    }
}

impl Default for OptionBool {
    fn default() -> Self {
        Self::None
    }
}

#[test]
fn test() {
    let platform = v8::new_default_platform(0, false).make_shared();
    v8::V8::initialize_platform(platform);
    v8::V8::initialize();
    
    let isolate = &mut v8::Isolate::new(Default::default());
    
    let scope = &mut v8::HandleScope::new(isolate);
    let context = v8::Context::new(scope);
    let scope = &mut v8::ContextScope::new(scope, context);
    
    let code = v8::String::new(scope, "`fuck ${12 ** 37}`").unwrap();
    println!("javascript code: {}", code.to_rust_string_lossy(scope));
    
    let script = v8::Script::compile(scope, code, None).unwrap();
    let result = script.run(scope).unwrap();
    let result = result.to_string(scope).unwrap();
    println!("result: {}", result.to_rust_string_lossy(scope));
}
