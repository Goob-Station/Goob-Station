using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.MalfunctionAi;

/// <summary>
/// Sent by the voice-mask window's reset button: clears the fake name, speech verb and
/// job icon so the speaker talks with their own voice again.
/// </summary>
[Serializable, NetSerializable]
public sealed class VoiceMaskResetNameMessage : BoundUserInterfaceMessage;
