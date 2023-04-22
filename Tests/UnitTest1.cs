using Coplt.V8Core.LowLevel;

namespace Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
        V8.AutoEnsuresInit();
    }

    [Test]
    public void Test1()
    {
        Assert.True(V8.IsInitialized);
        Console.WriteLine(V8.Version);
    }

    [Test]
    public void Test2()
    {
        using var isolate = Isolate.CreateOnCurrentThread();
        using var isolate_scope = HandleScope.Create(isolate);
        var ctx = LocalContext.Create(isolate_scope);
        using var ctx_scope = ContextScope.Create(isolate_scope, ctx);

        var code = LocalJsString.CreateUtf16(ctx_scope, "'Hello' + ' World!'");
        Console.WriteLine(code.ToString(ctx_scope));

        var script = LocalScript.Compile(ctx_scope, code);
        var result = script.Run(ctx_scope);
        var result_str = result.AsJsString(ctx_scope);
        Console.WriteLine(result_str.ToString(ctx_scope));
    }
}
