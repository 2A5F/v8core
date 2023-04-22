cargo build --release

Copy-Item "./target/release/v8core.dll" -Destination "./Core.V8"
