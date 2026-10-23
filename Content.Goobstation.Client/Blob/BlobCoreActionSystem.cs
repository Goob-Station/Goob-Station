// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Weapons.Melee;
using Content.Goobstation.Shared.Blob;
using Content.Goobstation.Shared.Blob.Events;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Blob;

public sealed partial class BlobCoreActionSystem : SharedBlobCoreActionSystem
{
    [Dependency] private MeleeWeaponSystem _meleeWeaponSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<BlobAttackEvent>(OnBlobAttack);
    }

    private static readonly EntProtoId Animation = "WeaponArcPunch";

    private void OnBlobAttack(BlobAttackEvent ev)
    {
        if(!TryGetEntity(ev.BlobEntity, out var user))
            return;

        _meleeWeaponSystem.DoLunge(user.Value, user.Value, Angle.Zero, ev.Position, Animation, Angle.Zero, false);
    }
}