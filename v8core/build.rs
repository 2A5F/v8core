fn main() {
    csbindgen::Builder::default()
        .input_extern_file("./src/lib.rs")
        .input_extern_file("./src/shared_ptr.rs")
        .input_extern_file("./src/platform.rs")
        .input_extern_file("./src/isolate.rs")
        .input_extern_file("./src/handle_scope.rs")
        .input_extern_file("./src/context.rs")
        .input_extern_file("./src/context_scope.rs")
        .input_extern_file("./src/script.rs")
        .input_extern_file("./src/value.rs")
        .input_extern_file("./src/string.rs")
        .csharp_dll_name("v8core")
        .csharp_namespace("Coplt.V8Core.LowLevel.Gen")
        .generate_csharp_file("../Core.V8/LowLevel/gen/v8.g.cs")
        .unwrap();
}
