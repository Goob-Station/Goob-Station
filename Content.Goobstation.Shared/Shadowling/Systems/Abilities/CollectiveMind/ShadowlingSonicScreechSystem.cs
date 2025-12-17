// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.CollectiveMind;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Shadowling.Systems.Abilities.CollectiveMind;

/// <summary>
/// This handles the Sonic Screech ability logic.
/// Sonic Screech "confuses" and "deafens" (flash effect + tinnitus sound) nearby people, damages windows, and stuns silicons/borgs. All in one pack!
/// </summary>
public sealed class ShadowlingSonicScreechSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingSonicScreechComponent, SonicScreechEvent>(OnSonicScreech);
        SubscribeLocalEvent<ShadowlingSonicScreechComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<ShadowlingSonicScreechComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingSonicScreechComponent> ent, ref MapInitEvent args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowlingSonicScreechComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnSonicScreech(EntityUid uid, ShadowlingSonicScreechComponent component, SonicScreechEvent args)
    {
        if (args.Handled)
            return;

        _popups.PopupPredicted(Loc.GetString("shadowling-sonic-screech-complete"), uid, uid, PopupType.Medium);
        _audio.PlayPredicted(component.ScreechSound, uid, uid);

        var effectEnt = PredictedSpawnAtPosition(component.SonicScreechEffect, Transform(uid).Coordinates);
        _transform.SetParent(effectEnt, uid);

        foreach (var entity in _lookup.GetEntitiesInRange(uid, component.Range))
        {
            if (_tag.HasTag(entity, component.WindowTag)
                && TryComp<DamageableComponent>(entity, out var damageableComponent)
                && _net.IsServer)
            {
                _damageable.TryChangeDamage(entity, component.WindowDamage, true, damageable: damageableComponent);
                continue;
            }

            if (!HasComp<MobStateComponent>(entity))
                continue;

            if (HasComp<ThrallComponent>(entity) ||
                HasComp<ShadowlingComponent>(entity))
                continue;

            if (HasComp<SiliconComponent>(entity))
            {
                _stun.TryParalyze(entity, component.SiliconStunTime, false);
                continue;
            }

            if (HasComp<HumanoidAppearanceComponent>(entity))
                PredictedSpawnAtPosition(component.ProtoFlash, Transform(entity).Coordinates);
        }

        args.Handled = true;
    }
}
