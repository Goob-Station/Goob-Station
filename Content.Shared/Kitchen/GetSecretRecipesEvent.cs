// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Kitchen;

/// <summary>
/// This returns a list of recipes not found in the main list of available recipes.
/// </summary>
[ByRefEvent]
public struct GetSecretRecipesEvent()
{
    public List<FoodRecipePrototype> Recipes = new();
}