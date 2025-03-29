using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Wizard.ForceWall;

[RegisterComponent]
public sealed partial class SpawnAnimationComponent : Component
{
    [DataField(required: true)]
    public float AnimationLength;

    [DataField]
    public bool Spawned;
}

[Serializable, NetSerializable]
public enum SpawnAnimationVisuals : byte
{
    Spawned,
}
