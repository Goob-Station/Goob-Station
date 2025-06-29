// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Nightmare.Components;
using Content.Goobstation.Shared.PhaseShift;
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.Ascension;
using Content.Server.Actions;
using Content.Server.Stunnable;
using Content.Shared.Actions;
using Content.Shared.Weapons.Reflect;

namespace Content.Goobstation.Server.Nightmare;


/// <summary>
/// This handles nightmare logic
/// </summary>
public sealed class NightmareSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actionsSystem = default!;
    [Dependency] private readonly StunSystem _stunSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NightmareComponent, ComponentStartup>(OnComponentStartup);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Nightmares reflect shots while in the dark
        var nightmare = EntityQueryEnumerator<NightmareComponent>();
        while (nightmare.MoveNext(out var uid, out var nightmareComponent))
        {
            if (!TryComp<LightDetectionComponent>(uid, out var lightDet))
                continue;

            if (lightDet.IsOnLight && HasComp<PhaseShiftedComponent>(uid))
            {
                RemComp<PhaseShiftedComponent>(uid);
                _stunSystem.TryKnockdown(uid, TimeSpan.FromSeconds(3), false);
                _actionsSystem.SetCooldown(nightmareComponent.ActionPlaneShiftEntity, TimeSpan.FromSeconds(3));
            }

            if (!TryComp<ReflectComponent>(uid, out var reflect))
                continue;

            if (lightDet.IsOnLight)
            {
                reflect.ReflectProb = 0f;
            }
            else
            {
                reflect.ReflectProb = 1f;
            }
        }
    }

    private void OnComponentStartup(EntityUid uid, NightmareComponent component, ComponentStartup args)
    {
        if (!TryComp<ActionsComponent>(uid, out var actions))
            return;

        if (TryComp<LesserShadowlingComponent>(uid, out var lesser))
        {
            _actionsSystem.RemoveAction(uid, lesser.ShadowWalkAction);
        }

        if (TryComp<ThrallComponent>(uid, out var thrall) && !HasComp<LesserShadowlingComponent>(uid))
        {
            _actionsSystem.RemoveAction(uid, thrall.ActionGuiseEntity);
            _actionsSystem.RemoveAction(uid, thrall.ActionThrallDarksightEntity);
        }
        // Shadowling Checks - END

        EnsureComp<LightDetectionComponent>(uid);
        EnsureComp<LightDetectionDamageModifierComponent>(uid);

        EnsureComp<LightEaterUserComponent>(uid);
        EnsureComp<ShadowlingPlaneShiftComponent>(uid);
        EnsureComp<ReflectComponent>(uid);

        _actionsSystem.AddAction(uid, ref component.ActionLightEntity, component.ActionLightEater, component: actions);
        _actionsSystem.AddAction(uid, ref component.ActionPlaneShiftEntity, component.ActionPlaneShift, component: actions);
    }
}
