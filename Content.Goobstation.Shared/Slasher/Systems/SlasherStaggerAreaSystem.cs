using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.Actions;
using Content.Shared.Interaction;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Slasher.Systems;

/// <summary>
/// Handles the Slasher Stagger Area action. When used, slows nearby mobs in range for a short duration.
/// </summary>
public sealed partial class SlasherStaggerAreaSystem : EntitySystem
{
    public static readonly EntProtoId EffectId = "SlasherSlowdownStatusEffect";

    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedInteractionSystem _interact = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private MovementModStatusSystem _movemod = default!;

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

            _movemod.TryUpdateMovementSpeedModDuration(targetUid, EffectId, TimeSpan.FromSeconds(comp.SlowDuration), comp.SlowMultiplier, comp.SlowMultiplier);

            // Show popup to the victim only
            if (_net.IsServer)
                _popup.PopupEntity(Loc.GetString("slasher-staggerarea-victim"), targetUid, targetUid, PopupType.MediumCaution);
        }

        _audio.PlayPredicted(comp.StaggerSound, uid, uid);

        var overlay = EnsureComp<SlasherStaggerOverlayComponent>(uid);
        overlay.Range = comp.Range;
        overlay.ShockwaveShader = comp.ShockwaveShader;
        overlay.RingColor = comp.RingColor;
        Dirty(uid, overlay);

        _popup.PopupClient(Loc.GetString("slasher-staggerarea-popup"), uid, uid, PopupType.MediumCaution);

        args.Handled = true;
    }
}
