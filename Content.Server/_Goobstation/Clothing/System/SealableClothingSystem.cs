using Content.Server.PowerCell;
using Content.Shared._Goobstation.Clothing.Components;
using Content.Shared._Goobstation.Clothing.Systems;

namespace Content.Server._Goobstation.Clothing.System;

public sealed partial class SealableClothingSystem : SharedSealableClothingSystem
{
    [Dependency] private readonly PowerCellSystem _powerCellSystem = default!;

    protected override void OnRequiresPowerSealAttempt(Entity<SealableClothingRequiresPowerComponent> entity, ref ClothingSealAttemptEvent args)
    {
        base.OnRequiresPowerSealAttempt(entity, ref args);

        _powerCellSystem.TryUseActivatableCharge(entity);
    }
}
