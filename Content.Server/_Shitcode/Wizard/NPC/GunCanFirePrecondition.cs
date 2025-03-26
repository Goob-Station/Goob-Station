using System.Linq;
using Content.Server.NPC;
using Content.Server.NPC.HTN.Preconditions;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Components;

namespace Content.Server._Goobstation.Wizard.NPC;

/// <summary>
/// Whether gun either has no ammo (other precondition handles that case) and it's not bolted/has no spent cartridges
/// </summary>
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

        return CanFire(gunSystem, gunUid) ^ Invert;
    }

    private bool CanFire(GunSystem gunSystem, EntityUid gunUid)
    {
        if (_entManager.TryGetComponent(gunUid, out RevolverAmmoProviderComponent? revolver))
        {
            // No ammo, let other precondition handle that OR there is at least 1 unspent casing
            return revolver.Chambers.All(x => x is null) || revolver.Chambers.Any(x => x is true);
        }

        if (_entManager.TryGetComponent(gunUid, out BallisticAmmoProviderComponent? ballistic))
            return CanBallisticShoot(ballistic);

        if (_entManager.HasComponent<MagazineAmmoProviderComponent>(gunUid))
            return CanMagazineShoot(gunUid);

        // ReSharper disable once InvertIf
        if (_entManager.TryGetComponent(gunUid, out ChamberMagazineAmmoProviderComponent? chamberMagazine))
        {
            if (chamberMagazine.BoltClosed is false)
                return false;

            if (gunSystem.GetChamberEntity(gunUid) is { } ammo)
                return IsAmmoValid(ammo);

            return CanMagazineShoot(gunUid);
        }

        return true;

        bool CanMagazineShoot(EntityUid gunEnt)
        {
            if (gunSystem.GetMagazineEntity(gunEnt) is not { } mag)
                return true;

            return !_entManager.TryGetComponent(mag, out BallisticAmmoProviderComponent? ballisticMag) ||
                   CanBallisticShoot(ballisticMag);
        }

        bool CanBallisticShoot(BallisticAmmoProviderComponent ballisticProvider)
        {
            if (ballisticProvider.Entities.Count == 0)
                return true; // Other precondition should handle that

            var ammo = ballisticProvider.Entities[^1];
            return IsAmmoValid(ammo);
        }

        bool IsAmmoValid(EntityUid ammo)
        {
            return !_entManager.TryGetComponent(ammo, out CartridgeAmmoComponent? cartridge) || !cartridge.Spent;
        }
    }
}
