namespace Content.Shared.Objectives.Components;

[RegisterComponent]
public sealed partial class BlankObjectiveComponent : Component
{
    [DataField]
    public bool SelfDefined = false;
}
