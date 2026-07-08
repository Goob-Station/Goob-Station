// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared.Kitchen.Components;

[RegisterComponent]
public sealed partial class FoodRecipeProviderComponent : Component
{
    /// <summary>
    /// These are additional recipes that the entity is capable of cooking.
    /// </summary>
    [DataField, ViewVariables]
    public List<ProtoId<FoodRecipePrototype>> ProvidedRecipes = new();
}