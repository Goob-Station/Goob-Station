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
using Content.Shared.Entry;
using Content.Shared.Interaction.Events;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Projectiles;
using Content.Shared.Execution;
using Content.Shared.Camera;
using Robust.Shared.Player;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;

namespace Content.Goobstation.Shared.Execution;

/// <summary>
///     verb for executing with guns
/// </summary>
public sealed class SharedGunExecutionSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedSuicideSystem _suicide = default!;
    [Dependency] private readonly SharedCombatModeSystem _combat = default!;
    [Dependency] private readonly SharedExecutionSystem _execution = default!;
    [Dependency] private readonly SharedGunSystem _gunSystem = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _recoil = default!;
    [Dependency] private readonly INetManager _net = default!;

    private const float GunExecutionTime = 4.0f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunComponent, GetVerbsEvent<UtilityVerb>>(OnGetInteractionVerbsGun);
        SubscribeLocalEvent<GunComponent, ExecutionDoAfterEvent>(OnDoafterGun);

    }

    private void OnGetInteractionVerbsGun(EntityUid uid, GunComponent component, GetVerbsEvent<UtilityVerb> args)
    {
        if (args.Hands == null || args.Using == null || !args.CanAccess || !args.CanInteract)
            return;

        var attacker = args.User;
        var weapon = args.Using!.Value;
        var victim = args.Target;
        var gunexecutiontime = component.GunExecutionTime;

        if (HasComp<GunExecutionBlacklistComponent>(weapon)
            || HasComp<PacifismAllowedGunComponent>(weapon)
            || !CanExecuteWithGun(weapon, victim, attacker))
            return;

        UtilityVerb verb = new()
        {
            Act = () => TryStartGunExecutionDoafter(weapon, victim, attacker, gunexecutiontime),
            Impact = LogImpact.High,
            Text = Loc.GetString("execution-verb-name"),
            Message = Loc.GetString("execution-verb-message"),
        };

        args.Verbs.Add(verb);
    }

    private bool CanExecuteWithGun(EntityUid weapon, EntityUid victim, EntityUid user)
    {
        if (!_execution.CanBeExecuted(victim, user)
            || TryComp<GunComponent>(weapon, out var gun)
            && !_gunSystem.CanShoot(gun))
            return false;

        return true;
    }

    private void TryStartGunExecutionDoafter(EntityUid weapon, EntityUid victim, EntityUid attacker, float gunexecutiontime)
    {
        if (!CanExecuteWithGun(weapon, victim, attacker))
            return;

        if (attacker == victim)
        {
            _execution.ShowExecutionInternalPopup("suicide-popup-gun-initial-internal", attacker, victim, weapon);
            _execution.ShowExecutionExternalPopup("suicide-popup-gun-initial-external", attacker, victim, weapon);
        }
        else
        {
            _execution.ShowExecutionInternalPopup("execution-popup-gun-initial-internal", attacker, victim, weapon);
            _execution.ShowExecutionExternalPopup("execution-popup-gun-initial-external", attacker, victim, weapon);
        }

        var doAfter =
            new DoAfterArgs(EntityManager, attacker, gunexecutiontime, new ExecutionDoAfterEvent(), weapon, target: victim, used: weapon)
            {
                BreakOnMove = true,
                BreakOnDamage = true,
                NeedHand = true,
                MultiplyDelay = false,
            };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private string GetDamage(DamageSpecifier damage, string? mainDamageType)
    {
        // Default fallback if nothing valid found
        mainDamageType ??= "Blunt";

        if (damage == null || damage.DamageDict.Count == 0)
            return mainDamageType;

        var filtered = damage.DamageDict
            .Where(kv => !string.Equals(kv.Key, "Structural", StringComparison.OrdinalIgnoreCase));

        if (filtered.Any())
        {
            mainDamageType = filtered.Aggregate((a, b) => a.Value > b.Value ? a : b).Key;
        }

        return mainDamageType ?? "Blunt";
    }

    private void OnDoafterGun(EntityUid uid, GunComponent component, DoAfterEvent args)
    {
        if (args.Handled
            || args.Cancelled
            || args.Used == null
            || args.Target == null
            || !_timing.IsFirstTimePredicted
            || !TryComp<GunComponent>(uid, out var guncomp))
            return;

        var attacker = args.User;
        var victim = args.Target.Value;
        var weapon = args.Used.Value;

        // Get the direction for the recoil
        Vector2 direction = Vector2.Zero;
        var attackerXform = Transform(attacker);
        var victimXform = Transform(victim);
        var diff = victimXform.WorldPosition - attackerXform.WorldPosition;
        if (diff != Vector2.Zero)
            direction = -diff.Normalized(); // recoil opposite of shot


        if (!CanExecuteWithGun(weapon, victim, attacker)
            || !TryComp<DamageableComponent>(victim, out var damageableComponent))
            return;

        // Take some ammunition for the shot (one bullet)
        var fromCoordinates = Transform(attacker).Coordinates;
        var ev = new TakeAmmoEvent(1, new List<(EntityUid? Entity, IShootable Shootable)>(), fromCoordinates, attacker);
        RaiseLocalEvent(weapon, ev);

        // Check if there's any ammo left
        if (ev.Ammo.Count <= 0)
        {
            _audio.PlayPredicted(component.SoundEmpty, uid, attacker);
            _execution.ShowExecutionInternalPopup("execution-popup-gun-empty", attacker, victim, weapon);
            _execution.ShowExecutionExternalPopup("execution-popup-gun-empty", attacker, victim, weapon);
            return;
        }

        DamageSpecifier damage = new DamageSpecifier();
        string? mainDamageType = null;
        // Get some information from IShootable
        var ammoUid = ev.Ammo[0].Entity;

        switch (ev.Ammo[0].Shootable)
        {
            case CartridgeAmmoComponent cartridge:
                {
                    var prototype = _prototypeManager.Index<EntityPrototype>(cartridge.Prototype);

                    prototype.TryGetComponent<ProjectileComponent>(out var projectileA, _componentFactory); // sloth forgive me

                    if (projectileA != null)
                    {
                        damage = projectileA.Damage;
                        mainDamageType = GetDamage(damage, mainDamageType);
                    }

                    cartridge.Spent = true; // Expend the cartridge
                    _appearanceSystem.SetData(ammoUid!.Value, AmmoVisuals.Spent, true);
                    Dirty(ammoUid.Value, cartridge);

                    break;
                }
            case AmmoComponent newAmmo: // This stops revolvers from hitting the user while executing someone, somehow
                TryComp<ProjectileComponent>(ammoUid, out var projectileB);

                if (projectileB != null)
                {
                    damage = projectileB.Damage;
                    mainDamageType = GetDamage(damage, mainDamageType);
                }

                if (ammoUid != null)
                    Del(ammoUid);
                break;
        }

        if (HasComp<HitscanBatteryAmmoProviderComponent>(weapon)) // Almost all hitscans are heat so this should work fine 
            mainDamageType = "Heat";

        var prev = _combat.IsInCombatMode(attacker);
        _combat.SetInCombatMode(attacker, true);

        if (attacker == victim)
        {
            _execution.ShowExecutionInternalPopup("suicide-popup-gun-complete-internal", attacker, victim, weapon);
            _execution.ShowExecutionExternalPopup("suicide-popup-gun-complete-external", attacker, victim, weapon);
            _audio.PlayPredicted(component.SoundGunshot, uid, attacker);
            _suicide.ApplyLethalDamage((victim, damageableComponent), mainDamageType);
        }
        else
        {
            if (_net.IsClient && direction != Vector2.Zero && _timing.IsFirstTimePredicted) // Just apply recoil for the client
                _recoil.KickCamera(attacker, direction);
            _execution.ShowExecutionInternalPopup("execution-popup-gun-complete-internal", attacker, victim, weapon);
            _execution.ShowExecutionExternalPopup("execution-popup-gun-complete-external", attacker, victim, weapon);
            _audio.PlayPredicted(component.SoundGunshot, uid, attacker);
            _suicide.ApplyLethalDamage((victim, damageableComponent), mainDamageType);
        }

        _combat.SetInCombatMode(attacker, prev);
        args.Handled = true;
    }
}
