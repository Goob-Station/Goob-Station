using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.ModSuits;

public sealed class ModModulesUiStateReadyEvent : EntityEventArgs
{
    public Dictionary<NetEntity, BoundUserInterfaceState?> States = new();  // ADT Mech UI Fix
}
