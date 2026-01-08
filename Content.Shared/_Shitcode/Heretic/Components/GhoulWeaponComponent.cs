using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class GhoulWeaponComponent : Component
{
    [DataField]
    public LocId ExamineMessage = "ghoul-weapon-comp-examine";
}
