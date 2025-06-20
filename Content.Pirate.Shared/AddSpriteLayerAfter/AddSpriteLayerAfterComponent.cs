using System.ComponentModel.DataAnnotations;

namespace Content.Pirate.Client.AddSpriteLayerAfter;

/// <summary>
/// Adds a layer to a sprite on
/// <see cref="AppearanceChangeEvent"/>
/// </summary>
[RegisterComponent]
public sealed partial class AddSpriteLayerAfterComponent : Component
{
    [DataField] public string? Sprite;
    [DataField] public string? State;
    [DataField] public bool? Visible;
    [DataField] public Enum? LayerIndex;
}

