using Content.Goobstation.Shared.CriminalRecords.Systems;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.CriminalRecords.Components;

/// <summary>
/// Lets the user hack a criminal records console, once.
/// Everyone is set to wanted with a randomly picked reason.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedCriminalRecordsHackerSystem))]
public sealed partial class GoobCriminalRecordsHackerComponent : Component;
