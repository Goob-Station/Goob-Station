using Content.Server._Harmony.GameTicking.Rules.Components;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Roles;
using Content.Server.Antag;
using Content.Shared.GameTicking.Components;
using Content.Shared._Harmony.Morph;


namespace Content.Server._Harmony.Gameticking.Rules;

/// <summary>
/// Sets round end breif for morphs.
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
        foreach (var (mind, data, name) in sessionData)
        {
            _role.MindHasRole<MorphComponent>(mind, out var role);
            var count = MorphComponent.Children;

            if (MorphComponent.Children != 1)
            args.AddLine(Loc.GetString("morph-name-user", ("name", name), ("username", data.UserName), ("count", count)));
            else
            args.AddLine(Loc.GetString("morph-name-user-lone", ("name", name), ("username", data.UserName), ("count", count)));
        }
    }


}
