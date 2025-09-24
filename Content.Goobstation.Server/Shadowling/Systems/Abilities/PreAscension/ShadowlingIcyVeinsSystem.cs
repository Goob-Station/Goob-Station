// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.PreAscension;
using Content.Server.Temperature.Components;
using Content.Shared.Actions;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Shadowling.Systems.Abilities.PreAscension;

/// <summary>
/// This handles Icy Veins logic. An AOE ability that lowers the temperature
/// of targets nearby and paralyzes them for a very short amount.
/// </summary>
public sealed class ShadowlingIcyVeinsSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingIcyVeinsComponent, IcyVeinsEvent>(OnIcyVeins);
        SubscribeLocalEvent<ShadowlingIcyVeinsComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<ShadowlingIcyVeinsComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingIcyVeinsComponent> ent, ref MapInitEvent args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowlingIcyVeinsComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        // Target's temperature adjusts to normal in a minute or so after the effect.
        // That means, they won't take lots of damage from this ability but they will be slowed down.
        var query = EntityQueryEnumerator<IcyVeinsTargetComponent>();
        while (query.MoveNext(out var uid, out var target))
        {
            if (TryComp<TemperatureComponent>(uid, out var temp))
            {
                target.Timer -= frameTime;

                if (target.Timer <= 0)
                {
                    temp.CurrentTemperature -= target.DecreaseTempByValue;
                    if (temp.CurrentTemperature <= target.MinTargetTemperature)
                        RemComp<IcyVeinsTargetComponent>(uid);

                    target.Timer = target.TimeUntilNextDecrease;
                }
            }
        }
    }

    private void OnIcyVeins(EntityUid uid, ShadowlingIcyVeinsComponent component, IcyVeinsEvent args)
    {
        if (args.Handled)
            return;

        foreach (var target in _lookup.GetEntitiesInRange(_transform.GetMapCoordinates(args.Performer), component.Range))
        {
            TryIcyVeins(target, component);
        }

        var effectEnt = Spawn(component.IcyVeinsEffect, _transform.GetMapCoordinates(uid));
        _transform.SetParent(effectEnt, uid);
        _audio.PlayPvs(component.IcyVeinsSound, uid, AudioParams.Default.WithVolume(-1f));
        args.Handled = true;
    }

    private void TryIcyVeins(EntityUid target, ShadowlingIcyVeinsComponent component)
    {
        if (!HasComp<MobStateComponent>(target)
            || !HasComp<TemperatureComponent>(target)
            || HasComp<ShadowlingComponent>(target)
            || HasComp<ThrallComponent>(target))
            return;

        EnsureComp<IcyVeinsTargetComponent>(target);
        _popup.PopupEntity(Loc.GetString("shadowling-icy-veins-activated"), target, target, PopupType.MediumCaution);

        if (!TryComp<StatusEffectsComponent>(target, out var statusEffectsComponent))
            return;

        _stun.TryParalyze(target, TimeSpan.FromSeconds(component.ParalyzeTime), false, statusEffectsComponent);
    }
}
