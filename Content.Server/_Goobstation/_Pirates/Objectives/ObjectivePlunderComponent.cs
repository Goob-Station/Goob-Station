namespace Content.Server._Goobstation._Pirates.Objectives;

[RegisterComponent]
public sealed partial class ObjectivePlunderComponent : Component
{
    [DataField] public float Plundered = 0f;
}
