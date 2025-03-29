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

        _meleeWeaponSystem.DoLunge(user.Value, user.Value, Angle.Zero, ev.Position, Animation, false);
    }
}
