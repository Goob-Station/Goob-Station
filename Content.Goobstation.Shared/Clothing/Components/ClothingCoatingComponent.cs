using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Clothing.Components;
/// <summary>
///     Add this to an item that can be used to coat clothing with somehting
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ClothingCoatingComponent : Component
{
    [DataField(required: true)] public string CoatingName;

    [DataField(required: true)]
    [AlwaysPushInheritance]
    public ComponentRegistry Components { get; private set; } = new();
}
