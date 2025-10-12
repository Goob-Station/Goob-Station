using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Goobstation.Shared.Wraith.WraithPoints;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using System.Linq;
using Content.Shared.Flash.Components;
using Content.Shared.Revenant.Components;
using Content.Shared.StatusEffect;

namespace Content.Goobstation.Shared.Wraith.Systems;
//Partially ported from Impstation
public sealed partial class HauntSystem : EntitySystem
{
    [Dependency] private readonly SharedInteractionSystem _interact = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly Content.Shared.StatusEffectNew.StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffectsOld = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly WraithPointsSystem _wraithPointsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HauntComponent, HauntEvent>(OnHaunt);
    }
    //TO DO: Add action to stop corporeal form.
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HauntComponent>();
        while (query.MoveNext(out var uid, out var haunt))
        {
            if (!haunt.Active)
                continue;

            if (_timing.CurTime < haunt.NextHauntWpRegenUpdate)
                continue;

            // reset generation rate to previous state
            _wraithPointsSystem.SetWpRate(haunt.PreviousWpRate, uid);
            haunt.Active = false;
            Dirty(uid, haunt);
        }
    }

    private void OnHaunt(Entity<HauntComponent> ent, ref HauntEvent args)
    {
        ent.Comp.PreviousWpRate = _wraithPointsSystem.GetCurrentWpRate(ent.Owner);
        Dirty(ent);

        // stop writing unreadable linq im gonna kms
        var witnessFilter = Filter.Pvs(ent.Owner).RemoveWhere(player =>
        {
            if (player.AttachedEntity == null)
                return true;

            var targetEnt = player.AttachedEntity.Value;

            // Skip non-humanoids, dead mobs, or wraith itself
            if (!HasComp<MobStateComponent>(targetEnt)
                || !HasComp<HumanoidAppearanceComponent>(targetEnt)
                || targetEnt == ent.Owner
                || HasComp<HauntedComponent>(targetEnt))
                return true;

            // Skip if out of range or obstructed
            return !_interact.InRangeUnobstructed((ent.Owner, Transform(ent.Owner)), (targetEnt, Transform(targetEnt)), range: 0, collisionMask: CollisionGroup.Impassable);
        });


        var witnesses = new HashSet<NetEntity>(
            witnessFilter.RemovePlayerByAttachedEntity(ent.Owner).Recipients
                .Select(ply => GetNetEntity(ply.AttachedEntity!.Value))
        );

        _statusEffectsOld.TryAddStatusEffect<CorporealComponent>(ent.Owner, ent.Comp.CorporealEffect, ent.Comp.HauntCorporealDuration, false);

        foreach (var witness in witnesses)
        {
            var witnessEnt = GetEntity(witness);

            _statusEffectsOld.TryAddStatusEffect<FlashedComponent>(witnessEnt, ent.Comp.FlashedId, ent.Comp.HauntFlashDuration, false);
            // Apply haunted effect
            EnsureComp<HauntedComponent>(witnessEnt);
        }

        if (_net.IsServer)
            _audioSystem.PlayGlobal(ent.Comp.HauntSound, witnessFilter, true);

        //TO DO: Increase WP regeneration for a limited period of time and gain WP based on how many people were witnesses.
        // Comment: In SS13, the skill constantly checks for witnesses rather than having one witness event. So WP regenaration should be in update.

        ent.Comp.Active = true;
        ent.Comp.NextHauntWpRegenUpdate = _timing.CurTime + ent.Comp.HauntWpRegenDuration;
        Dirty(ent);

        _wraithPointsSystem.AdjustWraithPoints(ent.Comp.HauntStolenWpPerWitness * witnesses.Count, ent.Owner);
        _wraithPointsSystem.AdjustWpGenerationRate(ent.Comp.HauntWpRegenPerWitness * witnesses.Count, ent.Owner);

        args.Handled = true;
    }
}
