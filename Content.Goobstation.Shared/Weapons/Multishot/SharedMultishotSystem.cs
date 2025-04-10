// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Weapons.Multishot;
using Content.Shared.CombatMode;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using System.Linq;

namespace Content.Goobstation.Shared.Weapons.Multishot;

public sealed partial class SharedMultishotSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedGunSystem _gunSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly SharedCombatModeSystem _combatSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MultishotComponent, GotEquippedHandEvent>(OnEquipWeapon);
        SubscribeLocalEvent<MultishotComponent, GotUnequippedHandEvent>(OnUnequipWeapon);
        SubscribeLocalEvent<MultishotComponent, GunRefreshModifiersEvent>(OnRefreshModifiers);
        SubscribeAllEvent<RequestShootEvent>(OnRequestShoot);
    }

    private void OnRequestShoot(RequestShootEvent msg, EntitySessionEventArgs args)
    {
        var user = args.SenderSession.AttachedEntity;

        if (user == null ||
            !_combatSystem.IsInCombatMode(user))
            return;

        var gunsEnumerator = GetMultishotGuns(user.Value);
        var shootCoords = GetCoordinates(msg.Coordinates);
        var target = GetEntity(msg.Target);

        foreach(var gun in gunsEnumerator)
        {
            var (gunEnt, gunComp, multiComp) = gun;

            var potentialTarget = target;
            if (gunComp.Target == null || !gunComp.BurstActivated || !gunComp.LockOnTargetBurst)
                gunComp.Target = potentialTarget;

            _gunSystem.AttemptShoot(user.Value, gunEnt, gunComp, shootCoords);
        }
    }

    private void OnRefreshModifiers(Entity<MultishotComponent> multishotWeapon, ref GunRefreshModifiersEvent args)
    {
        var (uid, comp) = multishotWeapon;

        var parent = _transformSystem.GetParentUid(uid);
        var gunsEnumerator = GetMultishotGuns(parent);

        if (gunsEnumerator.Count() < 2)
            return;

        args.MaxAngle *= comp.SpreadMultiplier;
        args.MinAngle *= comp.SpreadMultiplier;
    }

    private void OnEquipWeapon(Entity<MultishotComponent> multishotWeapon, ref GotEquippedHandEvent args)
    {
        var gunsEnumerator = GetMultishotGuns(args.User);

        foreach (var gun in gunsEnumerator)
        {
            _gunSystem.RefreshModifiers(gun.Item1);
        }
    }

    private void OnUnequipWeapon(Entity<MultishotComponent> multishotWeapon, ref GotUnequippedHandEvent args)
    {
        var gunsEnumerator = GetMultishotGuns(args.User);

        _gunSystem.RefreshModifiers(args.Unequipped);

        foreach (var gun in gunsEnumerator)
        {
            _gunSystem.RefreshModifiers(gun.Item1);
        }
    }

    /// <summary>
    /// Return list of guns in hands
    /// </summary>
    private IEnumerable<(EntityUid, GunComponent, MultishotComponent)> GetMultishotGuns(EntityUid entity)
    {
        var handsItems = _handsSystem.EnumerateHeld(entity);

        if (!handsItems.Any())
            yield break;

        foreach (var item in handsItems)
        {
            if (TryComp<GunComponent>(item, out var gunComp) && TryComp<MultishotComponent>(item, out var multishotComp))
                yield return (item, gunComp, multishotComp);
        }
    }
}
