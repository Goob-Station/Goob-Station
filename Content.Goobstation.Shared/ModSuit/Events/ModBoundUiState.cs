using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.ModSuits;

[Serializable, NetSerializable]
public sealed class ModBoundUiState : BoundUserInterfaceState
{
    public Dictionary<NetEntity, bool> EquipmentStates = new();
}

[Serializable, NetSerializable]
public sealed class RadialModBoundUiState : BoundUserInterfaceState;
