// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Eagle <lincoln.mcqueen@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 vanx <61917534+Vaaankas@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Weapons.Melee;
using Content.Goobstation.Shared.Blob;
using Content.Goobstation.Shared.Blob.Events;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Blob;

public sealed class BlobCoreActionSystem : SharedBlobCoreActionSystem
{
    [Dependency] private readonly MeleeWeaponSystem _meleeWeaponSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<BlobAttackEvent>(OnBlobAttack);
    }

    [ValidatePrototypeId<EntityPrototype>]
    private const string Animation = "WeaponArcPunch";

    private void OnBlobAttack(BlobAttackEvent ev)
    {
        if(!TryGetEntity(ev.BlobEntity, out var user))
            return;

        _meleeWeaponSystem.DoLunge(user.Value, user.Value, Angle.Zero, ev.Position, Animation, Angle.Zero, false);
    }
}