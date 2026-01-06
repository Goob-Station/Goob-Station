using Content.Goobstation.Server.Cult.Components;
using Content.Goobstation.Shared.Cult;
using Content.Server.Antag;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Cult.GameTicking;
public sealed partial class BloodCultRuleSystem : GameRuleSystem<BloodCultRuleComponent>
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly GameTicker _ticker = default!;

    public static readonly EntProtoId CultGamemodeId = "BloodCult";

    public static readonly SoundSpecifier GainSound = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/bloodcult_gain.ogg");
    public static readonly SoundSpecifier EyesGainSound = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/bloodcult_eyes.ogg");
    public static readonly SoundSpecifier HalosGainSound = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/bloodcult_halos.ogg");
    public static readonly SoundSpecifier ScribeSound = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/bloodcult_scribe.ogg");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultRuleComponent, AfterAntagEntitySelectedEvent>(OnAfterAntagEntitySelected);
    }

    private void OnAfterAntagEntitySelected(Entity<BloodCultRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        MakeCultist(args.EntityUid, ent, roundstart: true);
    }

    public void MakeCultist(EntityUid target, Entity<BloodCultRuleComponent> rule, bool roundstart = false)
    {
        EnsureComp<BloodCultistComponent>(target);
        EnsureComp<BloodMagicProviderComponent>(target);

        if (roundstart) _antag.SendBriefing(target, Loc.GetString("cult-gain-bloat"), Color.Crimson, null);
        _antag.SendBriefing(target, Loc.GetString("cult-gain-briefing"), Color.Red, GainSound);

        rule.Comp.Cultists.Add(target);
    }
}
