use crate::{impl_transmute_cast, SharedPtrOpaque};

#[repr(C)]
pub struct PlatformOpaque(SharedPtrOpaque);

impl_transmute_cast! { PlatformOpaque as v8::SharedRef<v8::Platform> }

#[repr(C)]
pub struct PlatformVTable {
    drop: unsafe extern "C" fn(ptr: PlatformOpaque),
    clone: unsafe extern "C" fn(ptr: PlatformOpaque) -> PlatformOpaque,
    ctor: unsafe extern "C" fn(thread_pool_size: u32, idle_task_support: bool) -> PlatformOpaque,
    ctor_single_threaded: unsafe extern "C" fn(idle_task_support: bool) -> PlatformOpaque,
}

pub const PLATFORM_VTABLE: PlatformVTable = PlatformVTable {
    drop: impls::platform_drop,
    clone: impls::platform_clone,
    ctor: impls::platform_new,
    ctor_single_threaded: impls::platform_new_single_threaded,
};

mod impls {
    use super::*;

    pub unsafe extern "C" fn platform_drop(ptr: PlatformOpaque) {
        let shared: v8::SharedRef<v8::Platform> = ptr.into();
        drop(shared)
    }

    pub unsafe extern "C" fn platform_clone(ptr: PlatformOpaque) -> PlatformOpaque {
        let shared: v8::SharedRef<v8::Platform> = ptr.into();
        shared.clone().into()
    }

    pub unsafe extern "C" fn platform_new(
        thread_pool_size: u32,
        idle_task_support: bool,
    ) -> PlatformOpaque {
        let plt = v8::Platform::new(thread_pool_size, idle_task_support);
        let shared = plt.make_shared();
        shared.into()
    }

    pub unsafe extern "C" fn platform_new_single_threaded(
        idle_task_support: bool,
    ) -> PlatformOpaque {
        let plt = v8::Platform::new_single_threaded(idle_task_support);
        let shared = plt.make_shared();
        shared.into()
    }
}
