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
        if (ent.Comp.PainToleranceModifier <= 0f)
        {
            Log.Warning($"Invalid PainToleranceModifier {ent.Comp.PainToleranceModifier} for {ent.Owner}. Using default value 1.0f");
            ent.Comp.PainToleranceModifier = 1.0f;
        }
        _mobThresholdSystem.SetMobStateThreshold(ent.Owner,
        _mobThresholdSystem.GetThresholdForState(ent.Owner, MobState.Critical) * ent.Comp.PainToleranceModifier,
        MobState.Critical,
        thresholds);
    }

}
