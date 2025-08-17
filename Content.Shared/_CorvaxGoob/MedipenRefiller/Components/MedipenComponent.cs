namespace Content.Shared._CorvaxNext.MedipenRefiller;

[RegisterComponent]
public sealed partial class MedipenComponent : Component
{
    /// <summary>
    /// Adds "Trash" tag when fully use
    /// </summary>
    [DataField]
    public bool TrashOnUse = true;

    /// <summary>
    /// Sets <seealso cref="MedipenRefiller.MedipenVisualLayer.Fill"/> to true on use 
    /// </summary>
    [DataField]
    public bool SetTrashSpriteOnUse = true;

    [DataField]
    public bool Used = false;

    [DataField]
    public Color Color = Color.White;

    [DataField("label")]
    public string DefaultLabel = "component-medipen-default-label";
}
