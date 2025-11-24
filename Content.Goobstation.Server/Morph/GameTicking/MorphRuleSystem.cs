using Content.Server.Antag;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Roles;
using Content.Goobstation.Shared.Morph;
using Content.Shared.GameTicking.Components;

namespace Content.Goobstation.Server.Morph.GameTicking;

/// <summary>
/// Sets round end brief for morphs.
/// </summary>
public sealed class MorphRuleSystem : GameRuleSystem<MorphRuleComponent>
{
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;

    /// <summary>
    /// Checks for "prime morphs" and looks how many times they replicated, would do all morphs but that would clog the end of round brief a bit.
    /// </summary>
    protected override void AppendRoundEndText(EntityUid uid, MorphRuleComponent component, GameRuleComponent gameRule, ref RoundEndTextAppendEvent args)
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);

        var sessionData = _antag.GetAntagIdentifiers(uid);
        foreach (var (_, data, name) in sessionData)
        {
            var count = MorphComponent.TotalChildren;

            if (count != 1)
                args.AddLine(Loc.GetString("morph-name-user", ("name", name), ("username", data.UserName), ("count", count)));
            else
                args.AddLine(Loc.GetString("morph-name-user-lone", ("name", name), ("username", data.UserName), ("count", count)));
        }
    }
}
