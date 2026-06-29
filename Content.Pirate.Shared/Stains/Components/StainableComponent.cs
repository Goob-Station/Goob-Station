using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.DoAfter;
using Content.Shared.Inventory;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Pirate.Shared.Stains.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class StainableComponent : Component
{
    [DataField]
    public string SolutionName = "stain";

    [DataField]
    public FixedPoint2 MaxStainVolume = FixedPoint2.New(5);

    [DataField]
    public FixedPoint2 SpillTransferAmount = 0.5f;

    [DataField]
    public float WringDoAfterDuration = 10f;

    [DataField]
    public SoundSpecifier WringSound = new SoundPathSpecifier("/Audio/_Pirate/Machines/wring.ogg");

    [DataField]
    public Dictionary<string, List<PrototypeLayerData>> ClothingVisuals = new();

    [DataField]
    public Dictionary<string, List<PrototypeLayerData>> ItemVisuals = new();

    [DataField]
    public List<PrototypeLayerData> IconVisuals = new();

    [ViewVariables]
    public HashSet<int> RevealedLayers = new();

    [ViewVariables]
    public HashSet<string> RevealedLayerKeys = new();

    [ViewVariables]
    public SlotFlags BodyStainSlots = SlotFlags.NONE;

    /// <summary>
    /// Whether stains own the temporary DNA scan tag.
    /// </summary>
    [ViewVariables]
    public bool AddedDnaSolutionScannable;
}

[Serializable, NetSerializable]
public sealed partial class WringStainDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class CleanStainsDoAfterEvent : SimpleDoAfterEvent;

[RegisterComponent]
public sealed partial class StainCleanerComponent : Component
{
    [DataField]
    public float CleanDelay = 12f;
}
