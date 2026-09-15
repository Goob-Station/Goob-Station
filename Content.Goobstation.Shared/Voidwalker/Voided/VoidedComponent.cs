using Content.Goobstation.Shared.Voidwalker.Components;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Voidwalker.Voided;

[RegisterComponent]
public sealed partial class VoidedComponent : Component
{
    /// <summary>
    /// The creature that kidnapped this entity.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Kidnapper;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextSpacedCheck;

    [DataField(customTypeSerializer:typeof(TimeOffsetSerializer))]
    public TimeSpan SpacedCheckInterval = TimeSpan.FromSeconds(2);

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextVomitTime;

    [DataField]
    public TimeSpan MinVomitInterval = TimeSpan.FromSeconds(60);

    [DataField]
    public TimeSpan MaxVomitInterval = TimeSpan.FromSeconds(90);

    [DataField]
    public float HungerLost = -10f;

    [DataField]
    public float ThirstLost = -10f;

    [DataField]
    public string NebulaVomitProto = "NebulaVomit";

    [ViewVariables]
    public HashSet<string> AddedComponents = [];
}
