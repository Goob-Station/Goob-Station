using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.TapeRecorder;

/// <summary>
/// Added to tape records that are updating, winding or rewinding the tape.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ActiveTapeRecorderComponent : Component;
