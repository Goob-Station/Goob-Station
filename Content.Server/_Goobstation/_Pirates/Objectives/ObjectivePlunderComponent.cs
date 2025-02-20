using Content.Server._Goobstation._Pirates.Pirates.Siphon;

namespace Content.Server._Goobstation._Pirates.Objectives;

[RegisterComponent]
public sealed partial class ObjectivePlunderComponent : Component
{
    //[ViewVariables(VVAccess.ReadOnly)][NonSerialized] public Entity<ResourceSiphonComponent>? BoundSiphon;
    [DataField] public float Plundered = 0f;
}
