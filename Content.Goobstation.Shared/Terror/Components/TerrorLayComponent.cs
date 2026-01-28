using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class TerrorLayComponent : Component
{
    [DataField]
    public List<EntProtoId> EggsTier1 = new()
{
    "TerrorEggRed",
    "TerrorEggGray",
    "TerrorEggGreen"
};

    [DataField]
    public List<EntProtoId> EggsTier2 = new()
{
    "TerrorEggBlack",
    "TerrorEggWhite",
    "TerrorEggPurple",
    "TerrorEggBrown"
};

    [DataField]
    public List<EntProtoId> EggsTier3 = new()
{
    "TerrorEggPrince",
    "TerrorEggPrincess",
    "TerrorEggMother"
};

}
