using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Bingle;

[RegisterComponent]
public sealed partial class BinglePrimePlacerComponent : Component
{
    [DataField]
    public EntProtoId BinglePrimePrototype = "MobBinglePrime";

    [DataField]
    public EntProtoId PlaceActionPrototype = "ActionPlaceBinglePrime";
}