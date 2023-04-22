using Coplt.V8Core.LowLevel;

V8.AutoEnsuresInit();

using var isolate = Isolate.CreateOnCurrentThread();
using var isolate_scope = HandleScope.Create(isolate);
var ctx = LocalContext.Create(isolate_scope);
using var ctx_scope = ContextScope.Create(isolate_scope, ctx);

var code = LocalJsString.CreateUtf16(ctx_scope, "`fuck ${12 ** 37}`");
Console.WriteLine(code.ToString(ctx_scope));

var script = LocalScript.Compile(ctx_scope, code);
var result = script.Run(ctx_scope);
var result_str = result.AsJsString(ctx_scope);
Console.WriteLine(result_str.ToString(ctx_scope));
