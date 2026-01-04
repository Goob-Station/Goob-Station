using Content.Server.Antag;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Cult.GameTicking;
public sealed partial class BloodCultRuleSystem : GameRuleSystem<BloodCultRuleComponent>
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly GameTicker _ticker = default!;

    public static readonly EntProtoId CultGamemodeId = "BloodCult";

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

    }
}
