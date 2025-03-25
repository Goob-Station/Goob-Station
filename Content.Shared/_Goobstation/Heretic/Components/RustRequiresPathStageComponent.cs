namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent]
public sealed partial class RustRequiresPathStageComponent : Component
{
    /// <summary>
    /// If rust heretic path stage is less than this - they won't be able to rust this surface
    /// </summary>
    [DataField]
    public int PathStage = 2;
}
