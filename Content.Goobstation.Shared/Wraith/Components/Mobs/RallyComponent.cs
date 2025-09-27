namespace Content.Goobstation.Shared.Wraith.Components.Mobs;
[RegisterComponent]
public sealed partial class RallyComponent : Component
{
    /// <summary>
    /// Range at which rally can affect entities.
    /// </summary>
    [DataField]
    public float RallyRange = 10f;
}
