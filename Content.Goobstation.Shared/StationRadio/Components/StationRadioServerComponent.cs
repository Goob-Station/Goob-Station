using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.StationRadio.Components;

/// <summary>
/// Relays all packets from the vinyl player and rig to all entities with <see cref="StationRadioReceiverComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StationRadioServerComponent : Component;
