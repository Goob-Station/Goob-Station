using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Component for Cosmic Cult's entropic colossus.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CosmicColossusComponent : Component
{
    [DataField]
    public EntityUid PolyVictim;
}

[Serializable, NetSerializable]
public enum Colossusisuals : byte
{
    Status,
}

[Serializable, NetSerializable]
public enum ColossusStatus : byte
{
    Alive,
    Dead,
}
