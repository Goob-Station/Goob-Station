using Content.Server.NPC;
using Content.Server.NPC.HTN.Preconditions;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Components;

namespace Content.Server._Goobstation.Wizard.NPC;

public sealed partial class GunCanFirePrecondition : HTNPrecondition
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    [DataField]
    public bool Invert;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var gunSystem = _entManager.System<GunSystem>();

        if (!gunSystem.TryGetGun(owner, out var gunUid, out _))
            return false;

        return CanFire(_entManager, gunSystem, gunUid) ^ Invert;
    }

    public static bool CanFire(IEntityManager entManager, GunSystem gunSystem, EntityUid gunUid)
    {
        if (entManager.TryGetComponent(gunUid, out RevolverAmmoProviderComponent? revolver))
        {
            var ammo = revolver.AmmoSlots[revolver.CurrentIndex];

            return ammo == null || IsAmmoValid(ammo.Value);
        }

        if (entManager.TryGetComponent(gunUid, out BallisticAmmoProviderComponent? ballistic))
            return CanBallisticShoot(ballistic);

        if (entManager.HasComponent<MagazineAmmoProviderComponent>(gunUid))
        {
            if (gunSystem.GetMagazineEntity(gunUid) is not { } mag)
                return false;

            return entManager.TryGetComponent(mag, out BallisticAmmoProviderComponent? ballisticMag) &&
                   CanBallisticShoot(ballisticMag);
        }

        if (entManager.TryGetComponent(gunUid, out ChamberMagazineAmmoProviderComponent? chamberMagazine))
        {
            if (chamberMagazine.BoltClosed is false)
                return false;

            return gunSystem.GetChamberEntity(gunUid) is { } ammo && IsAmmoValid(ammo);
        }

        return true;

        bool CanBallisticShoot(BallisticAmmoProviderComponent ballisticProvider)
        {
            if (ballisticProvider.Entities.Count == 0)
                return true; // Other precondition should handle that

            var ammo = ballisticProvider.Entities[^1];
            return IsAmmoValid(ammo);
        }

        bool IsAmmoValid(EntityUid ammo)
        {
            return !entManager.TryGetComponent(ammo, out CartridgeAmmoComponent? cartridge) || !cartridge.Spent;
        }
    }
}
