using System.Text;
using Content.Goobstation.Common.Traitor;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Popups;
using Microsoft.Extensions.Configuration;

namespace Content.Server.GameTicking.Rules;

public sealed partial class TraitorRuleSystem
{
    [Dependency] private readonly GoobCommonUplinkSystem _goobUplink = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    // RequestUplink method but with preference and pen spin code output
    private bool RequestUplink(EntityUid traitor,
        EntityUid mindId,
        FixedPoint2 startingBalance,
        out string? briefing,
        out string? briefingShort)
    {
        briefing = null;
        briefingShort = null;

        var uplinkPreference = _goobUplink.GetUplinkPreference(mindId);

        if (!_uplink.AddUplink(traitor,
                startingBalance,
                uplinkPreference,
                out _,
                out var setupEvent,
                giveDiscounts: true))
            return false;

        if (setupEvent != null)
        {
            briefing = setupEvent.Value.BriefingEntry;
            briefingShort = setupEvent.Value.BriefingEntryShort;
        }
        else // Fallback ooplink
        {
            briefing = Loc.GetString("traitor-role-uplink-implant");
            briefingShort = Loc.GetString("traitor-role-uplink-implant-short");
        }

        return true;
    }

    private string GenerateBriefingCharacter(string[]? codewords, string? uplinkBriefingShort, string objectiveIssuer)
    {
        var sb = new StringBuilder();
        sb.AppendLine("\n" + Loc.GetString($"traitor-{objectiveIssuer.Replace(" ", "").ToLower()}-intro"));

        if (uplinkBriefingShort != null)
            sb.AppendLine(uplinkBriefingShort);
        else sb.AppendLine("\n" + Loc.GetString($"traitor-role-nouplink"));

        if (codewords != null)
            sb.AppendLine(Loc.GetString($"traitor-role-codewords-short", ("codewords", string.Join(", ", codewords))));

        sb.AppendLine("\n" + Loc.GetString($"traitor-role-allegiances"));
        sb.AppendLine(Loc.GetString($"traitor-{objectiveIssuer.Replace(" ", "").ToLower()}-allies"));

        sb.AppendLine("\n" + Loc.GetString($"traitor-role-notes"));
        sb.AppendLine(Loc.GetString($"traitor-{objectiveIssuer.Replace(" ", "").ToLower()}-goal"));

        return sb.ToString();
    }
}
