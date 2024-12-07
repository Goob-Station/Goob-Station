using Content.Shared._Goobstation.Clothing.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Clothing.Components;

/// <summary>
///     Defines the clothing entity that can be sealed by <see cref="SealableClothingControlComponent"/>
/// </summary>
[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SealableClothingSystem))]
public sealed partial class SealableClothingComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool IsSealed = false;

    [DataField, AutoNetworkedField]
    public TimeSpan SealingTime = TimeSpan.FromSeconds(2);
}
