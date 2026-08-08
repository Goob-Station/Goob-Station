// SPDX-FileCopyrightText: 2026 Impstation contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.CombatMode;
using Content.Shared.Damage.Systems;
using Content.Shared.IdentityManagement;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Robust.Shared.Player;

namespace Content.Shared._Impstation.CombatModeSprint;

public abstract class SharedCombatModeSprintSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly SharedCombatModeSystem _combatMode = default!;
    [Dependency] private readonly DamageOnHighSpeedImpactSystem _impact = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CombatModeSprintComponent, ToggleCombatActionEvent>(OnToggleCombat);
        SubscribeLocalEvent<CombatModeSprintComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovespeed);
    }

    private void OnToggleCombat(Entity<CombatModeSprintComponent> ent, ref ToggleCombatActionEvent args)
    {
        _movementSpeed.RefreshMovementSpeedModifiers(ent);
        if (ent.Comp.BeginCombatMessage != null && _combatMode.IsInCombatMode(ent))
            _popup.PopupEntity(Loc.GetString(ent.Comp.BeginCombatMessage, ("name", Identity.Entity(ent, EntityManager))), ent, Filter.PvsExcept(ent), true);
        if (ent.Comp.EndCombatMessage != null && !_combatMode.IsInCombatMode(ent))
            _popup.PopupEntity(Loc.GetString(ent.Comp.EndCombatMessage, ("name", Identity.Entity(ent, EntityManager))), ent, Filter.PvsExcept(ent), true);
    }

    private void OnRefreshMovespeed(Entity<CombatModeSprintComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (_combatMode.IsInCombatMode(ent))
        {
            args.ModifySpeed(ent.Comp.SprintCoefficient);
            if (ent.Comp.DoImpactDamage)
                _impact.ChangeCollide(ent, ent.Comp.MinimumSpeed, ent.Comp.StunSeconds, ent.Comp.DamageCooldown, ent.Comp.SpeedDamage);
        }
        else
        {
            args.ModifySpeed(1f);
            if (ent.Comp.DoImpactDamage)
                _impact.ChangeCollide(ent, ent.Comp.DefaultMinimumSpeed, ent.Comp.DefaultStunSeconds, ent.Comp.DefaultDamageCooldown, ent.Comp.DefaultSpeedDamage);
        }
    }
}

