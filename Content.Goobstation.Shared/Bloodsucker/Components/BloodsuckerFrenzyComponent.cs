using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Bloodsuckers.Components;

/// <summary>
/// Added to a bloodsucker entity while it is in a blood frenzy.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BloodsuckerFrenzyComponent : Component
{
    /// <summary>Multiplier applied to blood drained per tick by the Feed action while in frenzy.</summary>
    [DataField]
    public float FeedMultiplier = 4f;

    /// <summary>When <c>true</c>, any grab attempt by this entity is immediately upgraded to a hard grab.</summary>
    [DataField]
    public bool InstantHardGrab = true;

    /// <summary>
    /// Burn damage per second applied while in frenzy.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float BurnDamagePerSecond = 3f;

    /// <summary>Accumulated time since last burn tick (seconds).</summary>
    [DataField]
    public float BurnAccumulator;

    /// <summary>How often (seconds) a burn damage tick is applied.</summary>
    [DataField]
    public float BurnTickRate = 1f;

    /// <summary>Whether deafness has been applied by the frenzy system.</summary>
    [DataField, AutoNetworkedField]
    public bool DeafnessApplied;

    /// <summary>Whether muteness has been applied by the frenzy system.</summary>
    [DataField, AutoNetworkedField]
    public bool MutenessApplied;

    /// <summary>Color of the full-screen overlay applied during frenzy.</summary>
    [DataField]
    public Color OverlayColor = new(0.6f, 0f, 0f, 0.35f);
}
