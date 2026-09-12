namespace Content.Shared._Lavaland.Megafauna.Components;

[RegisterComponent]
public sealed partial class MegafaunaAnchorComponent : Component
{
    [DataField, ViewVariables]
    public bool Anchored;
}
