using System.Globalization;
using System.Linq;
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.VoiceChat;

[AdminCommand(AdminFlags.Admin)]
public sealed class VoiceOfGodCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly VoiceChatSystem _voiceChat = default!;

    private const string RadiusMode = "radius";
    private const string DepartmentMode = "department";
    private const string StationMode = "station";
    private const float MaxRadius = 100f;

    public override string Command => "voiceofgod";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not { } admin)
        {
            shell.WriteError(Loc.GetString("shell-cannot-run-command-from-server"));
            return;
        }

        if (args.Length == 0)
        {
            _voiceChat.StopGodVoice(admin);
            return;
        }

        switch (args[0].ToLowerInvariant())
        {
            case RadiusMode:
                if (args.Length < 2 ||
                    !float.TryParse(args[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var radius) ||
                    radius <= 0f || radius > MaxRadius)
                {
                    shell.WriteError(Loc.GetString("cmd-voiceofgod-bad-radius", ("max", MaxRadius)));
                    return;
                }

                _voiceChat.StartGodVoice(admin, _voiceChat.GodTargetRadius(radius));
                return;

            case DepartmentMode:
                if (args.Length < 2 || !_prototype.TryIndex(new ProtoId<DepartmentPrototype>(args[1]), out var department))
                {
                    shell.WriteError(Loc.GetString("cmd-voiceofgod-bad-department"));
                    return;
                }

                _voiceChat.StartGodVoice(admin, _voiceChat.GodTargetDepartment(department));
                return;

            case StationMode:
                _voiceChat.StartGodVoice(admin, _voiceChat.GodTargetStation());
                return;
        }

        if (!_player.TryGetSessionByUsername(args[0], out var target))
        {
            shell.WriteError(Loc.GetString("shell-target-player-does-not-exist"));
            return;
        }

        _voiceChat.StartGodVoice(admin, _voiceChat.GodTargetPlayer(target));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            var options = new[] { RadiusMode, DepartmentMode, StationMode }
                .Select(mode => new CompletionOption(mode))
                .Concat(CompletionHelper.SessionNames(players: _player));
            return CompletionResult.FromHintOptions(options, Loc.GetString("cmd-voiceofgod-hint"));
        }

        if (args.Length == 2 && args[0].Equals(DepartmentMode, StringComparison.OrdinalIgnoreCase))
        {
            var departments = _prototype.EnumeratePrototypes<DepartmentPrototype>()
                .Select(department => new CompletionOption(department.ID, Loc.GetString(department.Name)));
            return CompletionResult.FromHintOptions(departments, Loc.GetString("cmd-voiceofgod-department-hint"));
        }

        if (args.Length == 2 && args[0].Equals(RadiusMode, StringComparison.OrdinalIgnoreCase))
            return CompletionResult.FromHint(Loc.GetString("cmd-voiceofgod-radius-hint"));

        return CompletionResult.Empty;
    }
}
