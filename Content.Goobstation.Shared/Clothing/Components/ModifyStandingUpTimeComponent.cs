using Content.Shared.Inventory;

namespace Content.Goobstation.Shared.Clothing.Components;

[RegisterComponent]
public sealed partial class ModifyStandingUpTimeComponent : Component
{
    [DataField]
    public float Multiplier = 1f;
}
