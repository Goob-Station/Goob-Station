using Content.Shared.Dataset;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Anomaly;

/// <summary>
/// This is used for the bald anomaly
/// </summary>
[RegisterComponent]
public sealed partial class BaldAnomalyComponent : Component
{
    /// <summary>
    /// Anomaly's area of effect.
    /// </summary>
    [DataField]
    public float BaseRange = 10f;

    /// <summary>
    /// Sound emitted when someone is made bald
    /// </summary>
    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/Items/scissors.ogg");

    [DataField]
    public float SpeakChance = 0.5f;


    [DataField]
    public ProtoId<LocalizedDatasetPrototype> BaldIsAwesomeStringDataset = "BaldIsAwesomeStringDataset";

    /// <summary>
    ///  if this entity will spawn a copy on deleted
    /// </summary>
    [DataField]
    public bool CanCopy = true;

    /// <summary>
    /// copy to spawn
    /// </summary>
    [DataField]
    public EntProtoId CopyProto = "AnomalyBaldCopy";
}
