using Content.Goobstation.Shared.Slasher.Components;
using Content.Goobstation.Shared.Slasher.Events;
using Content.Shared.Actions;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Slasher.Systems;

/// <summary>
/// Handles the Slasher Stagger Area action. When used, slows nearby mobs in range for a short duration.
/// </summary>
public sealed class SlasherStaggerAreaSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedInteractionSystem _interact = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherStaggerAreaComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlasherStaggerAreaComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SlasherStaggerAreaComponent, SlasherStaggerAreaEvent>(OnUse);
    }

    private void OnMapInit(Entity<SlasherStaggerAreaComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);
    }

    private void OnShutdown(Entity<SlasherStaggerAreaComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Comp.ActionEnt);
    }

    private void OnUse(Entity<SlasherStaggerAreaComponent> ent, ref SlasherStaggerAreaEvent args)
    {
        if (args.Handled)
            return;

        var (uid, comp) = ent;

        foreach (var (targetUid, _) in _lookup.GetEntitiesInRange<StatusEffectsComponent>(Transform(uid).Coordinates, comp.Range, LookupFlags.Dynamic))
        {
            if (targetUid == uid)
                continue;

            if (!_interact.InRangeUnobstructed(uid, targetUid, comp.Range))
                continue;

            _stun.TrySlowdown(targetUid, TimeSpan.FromSeconds(comp.SlowDuration), true, comp.SlowMultiplier, comp.SlowMultiplier);

            // Show popup to the victim
            if (_net.IsServer)
                _popup.PopupEntity(Loc.GetString("slasher-staggerarea-victim"), targetUid, targetUid, PopupType.MediumCaution);
        }

        _audio.PlayPredicted(comp.StaggerSound, uid, uid);

        // Show popup to the user
        if (_net.IsServer)
            _popup.PopupEntity(Loc.GetString("slasher-staggerarea-popup"), uid, uid, PopupType.MediumCaution);

        args.Handled = true;
    }
}
