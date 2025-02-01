using Content.Shared.Nutrition.Components;
using Robust.Shared.Prototypes;

namespace Content.Client.Nutrition.Components;

// needed for client to be able to check for food component
[RegisterComponent]
public sealed partial class FoodComponent : SharedFoodComponent
{
}
