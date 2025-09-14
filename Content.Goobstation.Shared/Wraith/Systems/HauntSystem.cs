using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Drugs;
using Content.Shared.Flash.Components;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.StatusEffect;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using System.Linq;
using System.Threading;

namespace Content.Goobstation.Shared.Wraith.Systems;
//Partially ported from Impstation
public sealed partial class HauntSystem : EntitySystem
{

    [Dependency] private readonly SharedInteractionSystem _interact = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HauntComponent, HauntEvent>(OnHaunt);
    }

    public void OnHaunt(Entity<HauntComponent> ent, ref HauntEvent args)
    {
        var uid = ent.Owner;
        var comp = ent.Comp;

        if (args.Handled)
            return;

        var witnessFilter = Filter.Pvs(uid).RemoveWhere(player =>
        {
            if (player.AttachedEntity == null)
                return true;

            var targetEnt = player.AttachedEntity.Value;

            // Skip non-humanoids, dead mobs, or wraith itself
            if (!HasComp<MobStateComponent>(targetEnt) || !HasComp<HumanoidAppearanceComponent>(targetEnt) || targetEnt == uid)
                return true;

            // Skip if out of range or obstructed
            return !_interact.InRangeUnobstructed((uid, Transform(uid)), (targetEnt, Transform(targetEnt)), range: 0, collisionMask: CollisionGroup.Impassable);
        });

        var witnesses = new HashSet<NetEntity>(
            witnessFilter.RemovePlayerByAttachedEntity(uid).Recipients
                .Select(ply => GetNetEntity(ply.AttachedEntity!.Value))
        );

        _statusEffects.TryAddStatusEffect<HauntComponent>(uid, comp.CorporealEffect, comp.HauntCorporealDuration, false);
        // Play global haunt sound
        _audioSystem.PlayGlobal(comp.HauntSound, witnessFilter, true);

        var newHaunts = 0;

        foreach (var witness in witnesses)
        {
            var witnessEnt = GetEntity(witness);

            // Apply flash effect
            _statusEffects.TryAddStatusEffect<FlashedComponent>(witnessEnt,
                comp.FlashedId,
                comp.HauntFlashDuration,
                false
            );

            // Apply haunted effect
            if (!EnsureComp<HauntedComponent>(witnessEnt, out var haunted))
                newHaunts += 1;
        }

        //TO DO: Increase WP regeneration for a limited period of time and gain WP based on how many people were witnesses.

        args.Handled = true;
    }
}
