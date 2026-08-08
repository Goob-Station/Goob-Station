namespace Content.Goobstation.Shared.Devil.Components;

[RegisterComponent]
public sealed partial class NoLimbForYouComponent : Component
{
    [DataField]
    public HashSet<string> ForbiddenSlots = new();
}
