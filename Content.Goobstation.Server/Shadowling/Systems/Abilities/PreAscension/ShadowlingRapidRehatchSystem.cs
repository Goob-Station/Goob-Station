// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.PreAscension;
using Content.Server.Administration.Systems;
using Content.Shared.Actions;
using Content.Shared.Administration.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.Shadowling.Systems.Abilities.PreAscension;

/// <summary>
/// This handles Rapid Re-Hatch logic. An ability that heals all wounds and status effects.
/// </summary>
public sealed partial class ShadowlingRapidRehatchSystem : EntitySystem
{
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private RejuvenateSystem _rejuvenate = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingRapidRehatchComponent, RapidRehatchEvent>(OnRapidRehatch);
        SubscribeLocalEvent<ShadowlingRapidRehatchComponent, RapidRehatchDoAfterEvent>(OnRapidRehatchDoAfter);
        SubscribeLocalEvent<ShadowlingRapidRehatchComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<ShadowlingRapidRehatchComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingRapidRehatchComponent> ent, ref MapInitEvent args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowlingRapidRehatchComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnRapidRehatch(EntityUid uid, ShadowlingRapidRehatchComponent comp, RapidRehatchEvent args)
    {
        if (args.Handled)
            return;

        var user = args.Performer;

        if (_mobState.IsCritical(user) || _mobState.IsDead(user))
            return;

        comp.ActionRapidRehatchEntity = args.Action;

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            uid,
            TimeSpan.FromSeconds(comp.DoAfterTime),
            new RapidRehatchDoAfterEvent(),
            user)
        {
            CancelDuplicate = true
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        args.Handled = true;
    }

    private void OnRapidRehatchDoAfter(EntityUid uid, ShadowlingRapidRehatchComponent comp, RapidRehatchDoAfterEvent args)
    {
        if (args.Cancelled
            || args.Handled)
            return;

        _popup.PopupEntity(Loc.GetString("shadowling-rapid-rehatch-complete"), uid, uid, PopupType.Medium);
        _rejuvenate.PerformRejuvenate(uid);

        var effectEnt = Spawn(comp.RapidRehatchEffect, _transform.GetMapCoordinates(uid));
        _transform.SetParent(effectEnt, uid);

        _audio.PlayPvs(comp.RapidRehatchSound, uid, AudioParams.Default.WithVolume(-2f));

        _actions.StartUseDelay(comp.ActionRapidRehatchEntity);
        args.Handled = true;
    }
}
