using Content.Shared.Nutrition.Components;

namespace Content.Goobstation.Client.Nutrition;

// needed for client to be able to check for food component
[RegisterComponent]
public sealed partial class FoodComponent : SharedFoodComponent
{
}
