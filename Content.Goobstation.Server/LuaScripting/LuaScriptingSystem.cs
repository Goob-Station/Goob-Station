using NLua;

namespace Content.Goobstation.Server.LuaScripting;

public sealed class LuaScriptingSystem : EntitySystem
{
    [Dependency] private readonly ILogManager _logManager = default!;
    private Lua _luaState = default!;

    public override void Initialize()
    {
        base.Initialize();
        _luaState = new Lua();

        // Expose C# functions to Lua
        _luaState["Log"] = (Action<string>)(msg => _logManager.GetSawmill("lua").Info(msg));
        ExecuteScript("Log(\"test\")");
    }

    public void ExecuteScript(string script)
    {
        try
        {
            _luaState.DoString(script);
        }
        catch (Exception e)
        {
            _logManager.GetSawmill("lua").Error($"Lua Error: {e.Message}");
        }
    }
    public override void Shutdown()
    {
        _luaState?.Dispose();
        base.Shutdown();
    }
}
