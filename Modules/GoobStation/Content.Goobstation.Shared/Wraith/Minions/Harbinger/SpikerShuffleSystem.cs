using System.Linq;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Goobstation.Shared.Wraith.Minions.Harbinger;

public sealed class SpikerShuffleSystem : EntitySystem
{
    [Dependency] private readonly Content.Shared.StatusEffect.StatusEffectsSystem _statusOld = default!;
    [Dependency] private readonly StatusEffectsSystem _statusNew = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpikerShuffleComponent, SpikerShuffleEvent>(OnSpikerShuffle);

        SubscribeLocalEvent<SpikerShuffleEffectComponent, StatusEffectAppliedEvent>(OnApplied);
        SubscribeLocalEvent<SpikerShuffleEffectComponent, StatusEffectRemovedEvent>(OnRemoved);
    }

    private void OnSpikerShuffle(Entity<SpikerShuffleComponent> ent, ref SpikerShuffleEvent args)
    {
        // first remove all status effects
        foreach (var statusEffect in ent.Comp.StatusEffectsToRemove)
            _statusOld.TryRemoveStatusEffect(ent.Owner, statusEffect);

        _statusNew.TryAddStatusEffect(ent.Owner, ent.Comp.StatusEffect, out _, ent.Comp.Duration);
        _statusNew.TryAddStatusEffect(ent.Owner, ent.Comp.StatusAbilityDisable, out _, ent.Comp.Duration); // disable using actions

        args.Handled = true;
    }

    private void OnApplied(Entity<SpikerShuffleEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        _popup.PopupClient(Loc.GetString("wraith-spiker-shuffle"), args.Target, args.Target, PopupType.Medium);
        _appearance.SetData(args.Target, ShuffleVisuals.Shuffling, true);

        if (TryComp<FixturesComponent>(args.Target, out var fixtures) && fixtures.FixtureCount >= 1)
        {
            var fixture = fixtures.Fixtures.First();

            _physics.SetCollisionMask(args.Target, fixture.Key, fixture.Value, (int) CollisionGroup.SmallMobMask, fixtures);
            _physics.SetCollisionLayer(args.Target, fixture.Key, fixture.Value, (int) CollisionGroup.SmallMobLayer, fixtures);
        }
    }

    private void OnRemoved(Entity<SpikerShuffleEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        _popup.PopupClient(Loc.GetString("wraith-spiker-shuffle-removed"), args.Target, args.Target, PopupType.Medium);
        _appearance.SetData(args.Target, ShuffleVisuals.Shuffling, false);

        if (TryComp<FixturesComponent>(args.Target, out var fixtures) && fixtures.FixtureCount >= 1)
        {
            var fixture = fixtures.Fixtures.First();

            _physics.SetCollisionMask(args.Target, fixture.Key, fixture.Value, (int) CollisionGroup.MobMask, fixtures);
            _physics.SetCollisionLayer(args.Target, fixture.Key, fixture.Value, (int) CollisionGroup.MobLayer, fixtures);
        }
    }
}

