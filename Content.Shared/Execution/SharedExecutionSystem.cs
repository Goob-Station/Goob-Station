// SPDX-FileCopyrightText: 2024 Celene <4323352+CuteMoonGod@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Celene <maurice_riepert94@web.de>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Scribbles0 <91828755+Scribbles0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.ActionBlocker;
using Content.Shared.Chat;
using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Content.Shared.Entry; // Goobstation
using Content.Shared.Interaction.Events; // Goobstation
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Projectiles;
using Robust.Shared.Player;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network; // Goobstation
using Robust.Shared.Prototypes; // Goobstation
using Robust.Shared.Timing;
using System.Diagnostics;

namespace Content.Shared.Execution;

/// <summary>
///     Verb for violently murdering cuffed creatures.
/// </summary>
public sealed class SharedExecutionSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedSuicideSystem _suicide = default!;
    [Dependency] private readonly SharedCombatModeSystem _combat = default!;
    [Dependency] private readonly SharedExecutionSystem _execution = default!;
    [Dependency] private readonly SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private readonly SharedGunSystem _gunSystem = default!; // Goobstation
    [Dependency] private readonly INetManager _netManager = default!; // Goobstation
    [Dependency] private readonly IComponentFactory _componentFactory = default!; // Goobstation
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!; // Goobstation
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!; // Goobstation
    [Dependency] private readonly IGameTiming _timing = default!; // Goobstation

    private const float GunExecutionTime = 4.0f; // Goobstation

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecutionComponent, GetVerbsEvent<UtilityVerb>>(OnGetInteractionsVerbs);
        SubscribeLocalEvent<ExecutionComponent, GetMeleeDamageEvent>(OnGetMeleeDamage);
        SubscribeLocalEvent<ExecutionComponent, SuicideByEnvironmentEvent>(OnSuicideByEnvironment);
        SubscribeLocalEvent<ExecutionComponent, ExecutionDoAfterEvent>(OnExecutionDoAfter);
        SubscribeLocalEvent<GunComponent, GetVerbsEvent<UtilityVerb>>(OnGetInteractionVerbsGun); // Goobstation gun executions
        SubscribeLocalEvent<GunComponent, ExecutionDoAfterEvent>(OnDoafterGun); // Goobstation gun executions

    }

    private void OnGetInteractionsVerbs(EntityUid uid, ExecutionComponent comp, GetVerbsEvent<UtilityVerb> args)
    {
        if (args.Hands == null || args.Using == null || !args.CanAccess || !args.CanInteract)
            return;

        var attacker = args.User;
        var weapon = args.Using.Value;
        var victim = args.Target;

        if (!CanBeExecuted(victim, attacker))
            return;

        UtilityVerb verb = new()
        {
            Act = () => TryStartExecutionDoAfter(weapon, victim, attacker, comp),
            Impact = LogImpact.High,
            Text = Loc.GetString("execution-verb-name"),
            Message = Loc.GetString("execution-verb-message"),
        };

        args.Verbs.Add(verb);
    }

    // Goobstation gun executions start
    private void OnGetInteractionVerbsGun(EntityUid uid, GunComponent component, GetVerbsEvent<UtilityVerb> args)
    {
        if (args.Hands == null || args.Using == null || !args.CanAccess || !args.CanInteract)
            return;

        var attacker = args.User;
        var weapon = args.Using!.Value;
        var victim = args.Target;

        if (HasComp<GunExecutionBlacklistComponent>(weapon))
            return;

        if (!CanExecuteWithGun(weapon, victim, attacker))
            return;

        UtilityVerb verb = new()
        {
            Act = () => TryStartGunExecutionDoafter(weapon, victim, attacker),
            Impact = LogImpact.High,
            Text = Loc.GetString("execution-verb-name"),
            Message = Loc.GetString("execution-verb-message"),
        };

        args.Verbs.Add(verb);
    }
    // Goobstation gun executions end

    private void TryStartExecutionDoAfter(EntityUid weapon, EntityUid victim, EntityUid attacker, ExecutionComponent comp)
    {
        if (!CanBeExecuted(victim, attacker))
            return;

        if (attacker == victim)
        {
            ShowExecutionInternalPopup(comp.InternalSelfExecutionMessage, attacker, victim, weapon);
            ShowExecutionExternalPopup(comp.ExternalSelfExecutionMessage, attacker, victim, weapon);
        }
        else
        {
            ShowExecutionInternalPopup(comp.InternalMeleeExecutionMessage, attacker, victim, weapon);
            ShowExecutionExternalPopup(comp.ExternalMeleeExecutionMessage, attacker, victim, weapon);
        }

        var doAfter =
            new DoAfterArgs(EntityManager, attacker, comp.DoAfterDuration, new ExecutionDoAfterEvent(), weapon, target: victim, used: weapon)
            {
                BreakOnMove = true,
                BreakOnDamage = true,
                NeedHand = true,
                MultiplyDelay = false, // Goobstation
            };

        _doAfter.TryStartDoAfter(doAfter);
    }

    // Goobstation gun executions start
    private void TryStartGunExecutionDoafter(EntityUid weapon, EntityUid victim, EntityUid attacker)
    {
        if (!CanExecuteWithGun(weapon, victim, attacker))
            return;

        if (attacker == victim)
        {
            ShowExecutionInternalPopup("suicide-popup-gun-initial-internal", attacker, victim, weapon);
            ShowExecutionExternalPopup("suicide-popup-gun-initial-external", attacker, victim, weapon);
        }
        else
        {
            ShowExecutionInternalPopup("execution-popup-gun-initial-internal", attacker, victim, weapon);
            ShowExecutionExternalPopup("execution-popup-gun-initial-external", attacker, victim, weapon);
        }

        var doAfter =
            new DoAfterArgs(EntityManager, attacker, GunExecutionTime, new ExecutionDoAfterEvent(), weapon, target: victim, used: weapon)
            {
                BreakOnMove = true,
                BreakOnDamage = true,
                NeedHand = true,
                MultiplyDelay = false, // Goobstation
            };

        _doAfter.TryStartDoAfter(doAfter);
    }
    // Goobstation gun executions end
    public bool CanBeExecuted(EntityUid victim, EntityUid attacker)
    {
        // No point executing someone if they can't take damage
        if (!HasComp<DamageableComponent>(victim))
            return false;

        // You can't execute something that cannot die
        if (!TryComp<MobStateComponent>(victim, out var mobState))
            return false;

        // You're not allowed to execute dead people (no fun allowed)
        if (_mobState.IsDead(victim, mobState))
            return false;

        // You must be able to attack people to execute
        if (!_actionBlocker.CanAttack(attacker, victim))
            return false;

        // The victim must be incapacitated to be executed
        if (victim != attacker && _actionBlocker.CanInteract(victim, null))
            return false;

        // All checks passed
        return true;
    }

    // Goobstation gun executions start
    private bool CanExecuteWithGun(EntityUid weapon, EntityUid victim, EntityUid user)
    {
        if (!CanBeExecuted(victim, user))
            return false;

        // We must be able to actually fire the gun
        if (!TryComp<GunComponent>(weapon, out var gun) && _gunSystem.CanShoot(gun!))
            return false;

        return true;
    }
    // Goobstation gun executions end

    private void OnGetMeleeDamage(Entity<ExecutionComponent> entity, ref GetMeleeDamageEvent args)
    {
        if (!TryComp<MeleeWeaponComponent>(entity, out var melee) || !entity.Comp.Executing)
        {
            return;
        }

        var bonus = melee.Damage * entity.Comp.DamageMultiplier - melee.Damage;
        args.Damage += bonus;
        args.ResistanceBypass = true;
    }
    private void OnSuicideByEnvironment(Entity<ExecutionComponent> entity, ref SuicideByEnvironmentEvent args)
    {
        if (!TryComp<MeleeWeaponComponent>(entity, out var melee))
            return;

        string? internalMsg = entity.Comp.CompleteInternalSelfExecutionMessage;
        string? externalMsg = entity.Comp.CompleteExternalSelfExecutionMessage;

        if (!TryComp<DamageableComponent>(args.Victim, out var damageableComponent))
            return;

        ShowExecutionInternalPopup(internalMsg, args.Victim, args.Victim, entity, false);
        ShowExecutionExternalPopup(externalMsg, args.Victim, args.Victim, entity);
        _audio.PlayPredicted(melee.HitSound, args.Victim, args.Victim);
        _suicide.ApplyLethalDamage((args.Victim, damageableComponent), melee.Damage);
        args.Handled = true;
    }

    private void ShowExecutionInternalPopup(string locString, EntityUid attacker, EntityUid victim, EntityUid weapon, bool predict = true)
    {
        if (predict)
        {
            _popup.PopupClient(
               Loc.GetString(locString, ("attacker", Identity.Entity(attacker, EntityManager)), ("victim", Identity.Entity(victim, EntityManager)), ("weapon", weapon)),
               attacker,
               attacker,
               PopupType.MediumCaution
               );
        }
        else
        {
            _popup.PopupEntity(
               Loc.GetString(locString, ("attacker", Identity.Entity(attacker, EntityManager)), ("victim", Identity.Entity(victim, EntityManager)), ("weapon", weapon)),
               attacker,
               attacker,
               PopupType.MediumCaution
               );
        }
    }

    private void ShowExecutionExternalPopup(string locString, EntityUid attacker, EntityUid victim, EntityUid weapon)
    {
        _popup.PopupEntity(
            Loc.GetString(locString, ("attacker", Identity.Entity(attacker, EntityManager)), ("victim", Identity.Entity(victim, EntityManager)), ("weapon", weapon)),
            attacker,
            Filter.PvsExcept(attacker),
            true,
            PopupType.MediumCaution
            );
    }

    private void OnExecutionDoAfter(Entity<ExecutionComponent> entity, ref ExecutionDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Used == null || args.Target == null)
            return;

        if (!TryComp<MeleeWeaponComponent>(entity, out var meleeWeaponComp))
            return;

        var attacker = args.User;
        var victim = args.Target.Value;
        var weapon = args.Used.Value;

        if (!_execution.CanBeExecuted(victim, attacker))
            return;

        // This is needed so the melee system does not stop it.
        var prev = _combat.IsInCombatMode(attacker);
        _combat.SetInCombatMode(attacker, true);
        entity.Comp.Executing = true;

        var internalMsg = entity.Comp.CompleteInternalMeleeExecutionMessage;
        var externalMsg = entity.Comp.CompleteExternalMeleeExecutionMessage;

        if (attacker == victim)
        {
            var suicideEvent = new SuicideEvent(victim);
            RaiseLocalEvent(victim, suicideEvent);

            var suicideGhostEvent = new SuicideGhostEvent(victim);
            RaiseLocalEvent(victim, suicideGhostEvent);
        }
        else
        {
            _melee.AttemptLightAttack(attacker, weapon, meleeWeaponComp, victim);
        }

        _combat.SetInCombatMode(attacker, prev);
        entity.Comp.Executing = false;
        args.Handled = true;

        if (attacker != victim)
        {
            _execution.ShowExecutionInternalPopup(internalMsg, attacker, victim, entity);
            _execution.ShowExecutionExternalPopup(externalMsg, attacker, victim, entity);
        }
    }

    // Goobstation gun executions start
    private void OnDoafterGun(EntityUid uid, GunComponent component, DoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Used == null || args.Target == null)
            return;

        if (!TryComp<GunComponent>(uid, out var guncomp))
            return;

        var attacker = args.User;
        var victim = args.Target.Value;
        var weapon = args.Used.Value;

        if (!_execution.CanExecuteWithGun(weapon, victim, attacker))
            return;

        if (!TryComp<DamageableComponent>(victim, out var damageableComponent))
            return;

        if (!_timing.IsFirstTimePredicted)
            return;

        // Take some ammunition for the shot (one bullet)
        var fromCoordinates = Transform(attacker).Coordinates;
        var ev = new TakeAmmoEvent(1, new List<(EntityUid? Entity, IShootable Shootable)>(), fromCoordinates, attacker);
        RaiseLocalEvent(weapon, ev);

        // Check if there's any ammo left
        if (ev.Ammo.Count <= 0)
        {
            _audio.PlayPredicted(component.SoundEmpty, uid, uid);
            ShowExecutionInternalPopup("execution-popup-gun-empty", attacker, victim, weapon);
            ShowExecutionExternalPopup("execution-popup-gun-empty", attacker, victim, weapon);
            Logger.Debug("GunFired");
            return;
        }

        // Get some information from IShootable
        var ammoUid = ev.Ammo[0].Entity;

        switch (ev.Ammo[0].Shootable)
        {
            case CartridgeAmmoComponent cartridge:
                {
                    var prototype = _prototypeManager.Index<EntityPrototype>(cartridge.Prototype);

                    prototype.TryGetComponent<ProjectileComponent>(out var projectileA, _componentFactory); // sloth forgive me
                    // Expend the cartridge
                    cartridge.Spent = true;
                    _appearanceSystem.SetData(ammoUid!.Value, AmmoVisuals.Spent, true);
                    Dirty(ammoUid.Value, cartridge);

                    break;
                }
            case AmmoComponent newAmmo: // This stops revolvers from hitting the user while executing someone, somehow
                TryComp<ProjectileComponent>(ammoUid, out var projectileB);
                if (ammoUid != null)
                    Del(ammoUid);
                break;
        }

        var prev = _combat.IsInCombatMode(attacker);
        _combat.SetInCombatMode(attacker, true);

        if (attacker == victim)
        {
            ShowExecutionInternalPopup("suicide-popup-gun-complete-internal", attacker, victim, weapon);
            ShowExecutionExternalPopup("suicide-popup-gun-complete-external", attacker, victim, weapon);
            _suicide.ApplyLethalDamage((victim, damageableComponent), "Piercing");
            if (_netManager.IsServer)
                _audio.PlayPvs(component.SoundGunshot, uid);
            if (!HasComp<RevolverAmmoProviderComponent>(weapon))
            {
                var suicideGhostEvent = new SuicideGhostEvent(victim);
                RaiseLocalEvent(victim, suicideGhostEvent);
            }
        }
        else
        {
            ShowExecutionInternalPopup("execution-popup-gun-complete-internal", attacker, victim, weapon);
            ShowExecutionExternalPopup("execution-popup-gun-complete-external", attacker, victim, weapon);
            if (_netManager.IsServer)
                _audio.PlayPvs(component.SoundGunshot, uid);
            _suicide.ApplyLethalDamage((victim, damageableComponent), "Piercing");
        }

        _combat.SetInCombatMode(attacker, prev);
        args.Handled = true;
    }
    // Goobstation gun executions end
}