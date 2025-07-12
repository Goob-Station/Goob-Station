using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;

namespace Content.Pirate.Server.Traits.PainTolerant;

public sealed class PainTolerantSystem : EntitySystem
{
    [Dependency] private readonly MobThresholdSystem _mobThresholdSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PainTolerantComponent, MapInitEvent>(OnInit);
    }
    private void OnInit(Entity<PainTolerantComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<MobThresholdsComponent>(ent.Owner, out var thresholds)) return;
        _mobThresholdSystem.SetMobStateThreshold(ent.Owner,
        _mobThresholdSystem.GetThresholdForState(ent.Owner, MobState.Critical) * ent.Comp.PainToleranceModifier,
        MobState.Critical,
        thresholds);
    }

}
