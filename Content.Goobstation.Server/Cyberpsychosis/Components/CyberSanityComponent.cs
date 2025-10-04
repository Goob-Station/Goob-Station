using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Cyberpsychosis;

[RegisterComponent]
public sealed partial class CyberSanityComponent : Component
{
    [DataField]
    public int MaxSanity = 1000;

    [ViewVariables(VVAccess.ReadWrite)]
    public int Sanity = 1000;

    /// <summary>
    /// Original sanity gain, before modifiers from implants
    /// </summary>
    [DataField("gain")]
    public int OriginalGain = 10;

    /// <summary>
    /// Modified sanity gain, updated by implants
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public int CurrentGain = 10;

    /// <summary>
    /// Effect thresholds applied when reaching certain sanity levels
    /// </summary>
    [DataField]
    public Dictionary<int, List<EntityEffect>> Effects = new();

    /// <summary>
    /// Component thresholds applied when reaching certain sanity levels
    /// </summary>
    [DataField]
    public Dictionary<int, ComponentRegistry> ComponentThresholds = new();

    /// <summary>
    /// Event thresholds applied when sanity becomes less than threshold
    /// </summary>
    [DataField]
    public Dictionary<int, List<object>> EventThresholdsLess = new();

    /// <summary>
    /// Event thresholds applied when sanity becomes more than threshold
    /// </summary>
    [DataField]
    public Dictionary<int, List<object>> EventThresholdsMore = new();

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextGain = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextEffect = TimeSpan.Zero;
}
