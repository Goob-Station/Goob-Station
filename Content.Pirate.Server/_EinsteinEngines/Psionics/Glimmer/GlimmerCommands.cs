using Content.Server.Administration;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.Psionics.Glimmer;

[AdminCommand(AdminFlags.Logs)]
public sealed class GlimmerShowCommand : IConsoleCommand
{
    public string Command => "glimmershow";
    public string Description => Loc.GetString("command-glimmershow-description");
    public string Help => Loc.GetString("command-glimmershow-help");
    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        shell.WriteLine(entMan.EntitySysManager.GetEntitySystem<GlimmerSystem>().GlimmerOutput.ToString("#.##"));
    }
}

[AdminCommand(AdminFlags.Debug)]
public sealed class GlimmerSetCommand : IConsoleCommand
{
    // SetGlimmerOutput cannot accept inputs greater than or equal to this number. The equation spits out imaginary number solutions past this point.
    private const int MaxGlimmer = 1000;

    public string Command => "glimmerset";
    public string Description => Loc.GetString("command-glimmerset-description");
    public string Help => Loc.GetString("command-glimmerset-help");

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1
            || !float.TryParse(args[0], out var glimmerValue)
            || glimmerValue >= MaxGlimmer || glimmerValue < 0)
            return;

        var entMan = IoCManager.Resolve<IEntityManager>();
        var glimmerSystem = entMan.System<GlimmerSystem>();
        glimmerSystem.SetGlimmerOutput(glimmerValue);
    }
}
