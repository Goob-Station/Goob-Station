using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Goobstation.Shared.Slasher.Events;
using Content.Shared.Actions;
using Content.Shared.Chemistry.Components;
using Content.Shared.Fluids;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Slasher.Systems;

/// <summary>
/// Handles the Slasher blood trail toggle action and periodically spills blood while active.
/// </summary>
public sealed class SlasherBloodTrailSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPuddleSystem _puddles = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    // Next time at which we should drop a blood puddle for an entity.
    private readonly Dictionary<EntityUid, TimeSpan> _nextDropAt = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherBloodTrailComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlasherBloodTrailComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SlasherBloodTrailComponent, ToggleBloodTrailEvent>(OnToggle);

        SubscribeLocalEvent<SlasherBloodTrailComponent, SlasherIncorporealizeEvent>(OnIncorporealize);
    }

    private void OnMapInit(Entity<SlasherBloodTrailComponent> ent, ref MapInitEvent args)
    {
        if (!_net.IsServer)
            return;

        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEntity, ent.Comp.ActionId);
    }

    private void OnShutdown(Entity<SlasherBloodTrailComponent> ent, ref ComponentShutdown args)
    {
        if (_net.IsServer)
            _actions.RemoveAction(ent.Owner, ent.Comp.ActionEntity);

        _nextDropAt.Remove(ent.Owner);
        StopFunkyslasher(ent.Owner, ent.Comp);
    }

    private void OnToggle(Entity<SlasherBloodTrailComponent> ent, ref ToggleBloodTrailEvent args)
    {
        if (args.Handled)
            return;

        if (!_net.IsServer)
        {
            args.Handled = true;
            return;
        }

        ent.Comp.IsActive = !ent.Comp.IsActive;
        Dirty(ent, ent.Comp);

        // Reset the drop timer so the first drop happens immediately when turning on.
        if (ent.Comp.IsActive)
        {
            _nextDropAt[ent.Owner] = _timing.CurTime; // drop now
            StartFunkyslasher(ent.Owner, ent.Comp);
        }
        else
        {
            _nextDropAt.Remove(ent.Owner);
            StopFunkyslasher(ent.Owner, ent.Comp); // Aura loss
        }

        args.Handled = true;
    }

    private void OnIncorporealize(Entity<SlasherBloodTrailComponent> ent, ref SlasherIncorporealizeEvent args)
    {
        if (!_net.IsServer)
            return;

        // Always disable the trail when attempting to become incorporeal.
        if (!ent.Comp.IsActive)
            return;

        ent.Comp.IsActive = false;
        Dirty(ent, ent.Comp);
        _nextDropAt.Remove(ent.Owner);
        StopFunkyslasher(ent.Owner, ent.Comp); // AURRAAA LOSSS
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Only the server should actually spawn puddles.
        if (_net.IsClient)
            return;

        var enumerator = EntityQueryEnumerator<SlasherBloodTrailComponent>();
        while (enumerator.MoveNext(out var uid, out var comp))
        {
            if (!comp.IsActive)
                continue;

            if (!_nextDropAt.TryGetValue(uid, out var next) || _timing.CurTime < next)
                continue;

            _nextDropAt[uid] = _timing.CurTime + comp.DropInterval;

            // Spill a small amount of generic blood at the entity's feet.
            var solution = new Solution();
            var amount = FixedPoint2.Max(FixedPoint2.Zero, comp.VolumePerDrop);
            solution.AddReagent("Blood", amount);

            _puddles.TrySpillAt(uid, solution, out _, sound: false);
        }
    }

    private void StartFunkyslasher(EntityUid uid, SlasherBloodTrailComponent comp)
    {
        if (_net.IsClient)
            return;

        if (comp.FunkyslasherStream != null)
            comp.FunkyslasherStream = _audio.Stop(comp.FunkyslasherStream);

        comp.FunkyslasherStream = _audio.PlayPvs(comp.Funkyslasher, uid)?.Entity;
    }

    private void StopFunkyslasher(EntityUid uid, SlasherBloodTrailComponent comp)
    {
        if (_net.IsClient)
            return;

        comp.FunkyslasherStream = _audio.Stop(comp.FunkyslasherStream);
    }
}
