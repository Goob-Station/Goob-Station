using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Silicon.MalfAI;

[RegisterComponent]
public sealed partial class MalfStationAIComponent : Component
{
    /// <summary>
    /// The currency the AI uses to get abilities.
    /// </summary>
    [DataField]
    public int ProcessingPowerPoints = 50;

    /// <summary>
    /// The reward for hacking an APC.
    /// </summary>
    [DataField]
    public int HackAPCReward = 10;
}