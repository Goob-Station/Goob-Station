// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling.Components.Abilities.PreAscension;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Shadowling.Systems.Abilities.PreAscension;

/// <summary>
/// This handles the Glare ability
/// </summary>
public sealed class ShadowlingGlareSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedShadowlingSystem _shadowling = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StatusEffectsSystem _effects = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingGlareComponent, GlareEvent>(OnGlare);
        SubscribeLocalEvent<ShadowlingGlareComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<ShadowlingGlareComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingGlareComponent> ent, ref MapInitEvent args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowlingGlareComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<ShadowlingGlareComponent>();
        while (query.MoveNext(out _, out var glare))
        {
            if (glare.ActivateGlareTimer)
            {
                // Time before the ability activates
                glare.GlareTimeBeforeEffect -= frameTime;

                if (glare.GlareTimeBeforeEffect <= 0)
                    ActivateStun(glare.GlareTarget, glare);
            }
        }
    }

    private void ActivateStun(EntityUid target, ShadowlingGlareComponent comp)
    {
        _stun.TryStun(target, TimeSpan.FromSeconds(comp.GlareStunTime), false);
        comp.ActivateGlareTimer = false;
    }

    private void OnGlare(EntityUid uid, ShadowlingGlareComponent comp, GlareEvent args)
    {
        if (args.Handled)
            return;

        var target = args.Target;
        var user = args.Performer;

        if (!_shadowling.CanGlare(target))
            return;

        var targetCoords = _transform.GetWorldPosition(target);
        var distance = (_transform.GetWorldPosition(user) - targetCoords).Length();
        comp.GlareTarget = target;

        // Glare mutes and slows down the target no matter what.
        if (TryComp<StatusEffectsComponent>(target, out var statComp))
        {
            _effects.TryAddStatusEffect(target, "Muted", TimeSpan.FromSeconds(comp.MuteTime), false, statComp);
            _stun.TrySlowdown(target, TimeSpan.FromSeconds(comp.SlowTime), false, 0.5f, 0.5f, statComp);
        }

        if (distance <= comp.MinGlareDistance)
        {
            comp.GlareStunTime = comp.MaxGlareStunTime;
            _stun.TryStun(target, TimeSpan.FromSeconds(comp.GlareStunTime), true);
        }
        else
        {
            // Do I know what is going on here? No. But it works so... Thanks for listening!
            comp.GlareStunTime = comp.MaxGlareStunTime * (1 - Math.Clamp(distance / comp.MaxGlareDistance, 0, 1));
            comp.GlareTimeBeforeEffect = comp.MinGlareDelay + (comp.MaxGlareDelay - comp.MinGlareDelay) * Math.Clamp(distance / comp.MaxGlareDistance, 0, 1);

            comp.ActivateGlareTimer = true;
        }

        var effectEnt = PredictedSpawnAtPosition(comp.EffectGlare, Transform(uid).Coordinates);
        _transform.SetParent(effectEnt, uid);

        _popup.PopupEntity(Loc.GetString("shadowling-glare-target"), target, target, PopupType.MediumCaution);
        args.Handled = true;
    }
}
