using Content.Server.Body.Systems;
using Content.Server.Body.Components;
using Content.Shared.Damage;

namespace Content.Server.Traits.Assorted;

public sealed class HemophiliaSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HemophiliaComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<HemophiliaComponent, DamageModifyEvent>(OnDamageModify);
    }

    private void OnStartup(EntityUid uid, HemophiliaComponent component, ComponentStartup args)
    {
        if (!TryComp<BloodstreamComponent>(uid, out var bloodstream))
            return;

        bloodstream.BleedReductionAmount *= component.BleedReductionModifier;
    }

    private void OnDamageModify(EntityUid uid, HemophiliaComponent component, DamageModifyEvent args)
    {
        args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, component.DamageModifiers);
    }
}
