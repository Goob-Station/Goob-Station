namespace Content.Goobstation.Common.Emag.Components;

/// <summary>
/// Marker component for vending machines that will lube all of it's item when emagged with EmagType.Jestographic
/// </summary>
[RegisterComponent]
public sealed partial class LubeItemsOnEmagComponent : Component
{
    /// <summary>
    /// How many attempt left
    /// </summary>
    [DataField]
    public int SlipsLeft;

    /// <summary>
    /// How strong the throw would be
    /// </summary>
    [DataField]
    public int SlipStrength;
}
