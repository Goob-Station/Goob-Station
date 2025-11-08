using Content.Shared.StatusEffectNew;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Stealth;
public sealed partial class ForcedStealthSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;

    public static readonly EntProtoId ForcedStealth = "ForcedStealthStatusEffect";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ForcedStealthStatusEffectComponent, StatusEffectAppliedEvent>(OnStatusApplied);
        SubscribeLocalEvent<ForcedStealthStatusEffectComponent, StatusEffectRemovedEvent>(OnStatusRemoved);
    }

    public bool TryApplyForceStealth(EntityUid uid, out EntityUid? statusEffect, float durationInSeconds)
        => _status.TryAddStatusEffect(uid, ForcedStealth, out statusEffect, TimeSpan.FromSeconds(durationInSeconds));

    private void OnStatusApplied(Entity<ForcedStealthStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        var stealth = EnsureComp<StealthComponent>(args.Target);
        _stealth.SetVisibility(args.Target, ent.Comp.Visibility, stealth);
    }

    private void OnStatusRemoved(Entity<ForcedStealthStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (HasComp<StealthComponent>(args.Target))
            RemComp<StealthComponent>(args.Target);
    }
}
