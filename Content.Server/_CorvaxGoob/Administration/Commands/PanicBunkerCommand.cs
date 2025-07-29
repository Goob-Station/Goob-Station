using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared._CorvaxGoob.CCCVars;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Server._CorvaxGoob.Administration.Commands;

[AdminCommand(AdminFlags.Server)]
public sealed class PanicBunkerDenyVpnCommand : LocalizedCommands
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    public override string Command => "panicbunker_deny_vpn";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Loc.GetString("shell-need-exactly-one-argument"));
            return;
        }

        if (!bool.TryParse(args[0], out var deny))
        {
            shell.WriteError(Loc.GetString("shell-argument-must-be-boolean"));
            return;
        }

        _cfg.SetCVar(CCCVars.PanicBunkerDenyVPN, deny);
        shell.WriteLine(Loc.GetString(deny ? "panicbunker-command-deny-vpn-enabled" : "panicbunker-command-deny-vpn-disabled"));
    }
}
