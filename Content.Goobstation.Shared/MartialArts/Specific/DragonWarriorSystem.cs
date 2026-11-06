using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Specific.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.StatusEffect;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.MartialArts.Specific;

public sealed partial class DragonWarriorSystem : EntitySystem
{
    [Dependency] private MartialArtsSystem _martialArts = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private StatusEffectsSystem _status = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var dragonQuery =
            EntityQueryEnumerator<DragonKungFuTimerComponent, StatusEffectsComponent, MobStateComponent, PhysicsComponent>();
        while (dragonQuery.MoveNext(out var uid, out var timer, out var status, out var mobState, out var physics))
        {
            if (!_mobState.IsAlive(uid))
                continue;

            if (physics.LinearVelocity.LengthSquared() > timer.MinVelocitySquared)
            {
                timer.LastMoveTime = _timing.CurTime;
                continue;
            }

            if (!_actionBlocker.CanInteract(uid, null)
                || _timing.CurTime < timer.LastMoveTime + timer.PauseDuration)
                continue;

            _status.TryAddStatusEffect<DragonPowerBuffComponent>(uid,
                "DragonPower",
                timer.BuffLength,
                true,
                status);

            // So that it doesn't update constantly
            timer.LastMoveTime = _timing.CurTime;
        }
    }

    [SubscribeLocalEvent]
    private void OnAttacked(Entity<DragonPowerBuffComponent> ent, ref AttackedEvent args)
    {
        if (_hands.TryGetActiveItem(ent.Owner, out _) // Only unarmed
            || !_actionBlocker.CanInteract(ent, null)) // Should be able to interact
            return;

        args.ModifiersList.Add(ent.Comp.ModifierSet);

        // Works for both armed and unarmed attacks
        _martialArts.ApplyMultiplier(ent,
            ent.Comp.DamageMultiplier,
            0f,
            ent.Comp.AttackDamageBuffDuration,
            MartialArtModifierType.Damage);
    }
}
