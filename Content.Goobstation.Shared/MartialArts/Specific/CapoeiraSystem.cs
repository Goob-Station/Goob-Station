using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Goobstation.Shared.MartialArts.Specific.Components;
using Content.Goobstation.Shared.Sprinting;
using Content.Shared.Movement.Systems;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.MartialArts.Specific;

public sealed partial class CapoeiraSystem : EntitySystem
{
    [Dependency] private MovementSpeedModifierSystem _modifier = default!;
    [Dependency] private SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private MartialArtsSystem _martialArts = default!;
    [Dependency] private SprintingSystem _sprinting = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    [SubscribeLocalEvent]
    private void OnCapoeiraMeleeHit(Entity<CapoeiraUserComponent> ent, ref MeleeHitEvent ev)
    {
        if (ev.HitEntities.Count > 0 || ev.Weapon != ent.Owner)
            return;

        // Damage up on miss
        _martialArts.ApplyMultiplier(ent,
            1f,
            2f,
            TimeSpan.FromSeconds(3),
            MartialArtModifierType.Damage | MartialArtModifierType.Unarmed);

        // Miss recovery
        if (!TryComp(ent, out MeleeWeaponComponent? melee))
            return;

        melee.NextAttack -= TimeSpan.FromSeconds(0.75f / _melee.GetAttackRate(ent, ent, melee));
        Dirty(ent, melee);
    }

    private void OnCapoeiraAttackPerformed(Entity<MartialArtsKnowledgeComponent> ent,
        ref AfterComboAttackPerformedEvent args)
    {
        if (args.Performer.Owner == args.Target)
            return;

        if (args.Type == ComboAttackType.Grab)
        {
            _martialArts.ApplyMultiplier(ent, 1.2f, 0f, TimeSpan.FromSeconds(4), MartialArtModifierType.MoveSpeed);
            _sprinting.SetSprinting(args.Target, false, gracefulStop: false, cooldownOverride: TimeSpan.FromSeconds(2));
            return;
        }

        if (args.Weapon != args.Performer.Owner
            || args.Type is not (ComboAttackType.Disarm or ComboAttackType.Harm))
            return;

        var velocity = GetVelocity(ent.Owner);
        var multiplier = Math.Clamp(MathF.Pow(velocity, 0.2f), 1f, 1.5f);
        _martialArts.ApplyMultiplier(ent, multiplier, 0f, TimeSpan.FromSeconds(3));
    }

    public bool CanPerformCapoeiraMove(
        Entity<PhysicsComponent?> ent,
        float minPower,
        float maxPower,
        float minVelocity,
        float velocityPowerMultiplier,
        out float power)
    {
        power = 0;

        var velocity = GetVelocity(ent);

        if (velocity < minVelocity)
            return false;

        power = Math.Clamp(velocity * velocityPowerMultiplier, minPower, maxPower);
        return true;
    }

    private float GetVelocity(Entity<PhysicsComponent?> ent)
    {
        return Resolve(ent, ref ent.Comp, false) ? ent.Comp.LinearVelocity.Length() : 0f;
    }
}
