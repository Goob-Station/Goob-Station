using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Voidwalker.Abilities.WallConversion;

[RegisterComponent]
public sealed partial class VoidedStructureComponent : Component
{
    [DataField]
    public string TrackedComponentsIdentifier = "VoidedStructure";

    [DataField(customTypeSerializer:typeof(TimeOffsetSerializer))]
    public TimeSpan RemoveAt;

    [DataField]
    public TimeSpan RemoveDelay = TimeSpan.FromSeconds(30);
}
