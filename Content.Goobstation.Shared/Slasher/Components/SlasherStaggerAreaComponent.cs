using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Grants the Slasher an area stagger action that slows nearby enemies briefly.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlasherStaggerAreaComponent : Component
{
    [ViewVariables]
    public EntityUid? ActionEnt;

    [DataField]
    public EntProtoId ActionId = "ActionSlasherStaggerArea";

    /// <summary>
    /// Radius of the aura effect, and the radius the shockwave ring expands to. Networked so the
    /// overlay can draw other slashers' rings.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Range = 3.5f;

    /// <summary>
    /// Shader the shockwave ring draws with.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string ShockwaveShader = "SlasherStagger";

    [DataField, AutoNetworkedField]
    public Color RingColor = new(0.22f, 0.02f, 0.34f);

    /// <summary>
    /// When the slasher last triggered the stagger, driving the shockwave ring's expansion.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan SpawnTime;

    /// <summary>
    /// Duration of the slowdown.
    /// </summary>
    [DataField]
    public float SlowDuration = 8f;

    /// <summary>
    /// Speed debuff.
    /// </summary>
    [DataField]
    public float SlowMultiplier = 0.5f;

    /// <summary>
    /// Sound to play when the stagger area is activated.
    /// </summary>
    [DataField]
    public SoundSpecifier StaggerSound = new SoundPathSpecifier("/Audio/_Goobstation/Slasher/Effects/SlasherStaggerArea.ogg")
    {
        Params = AudioParams.Default
                       .WithMaxDistance(4f)
    };
}
