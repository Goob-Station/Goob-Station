// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Shared.Kitchen;

/// <summary>
/// This returns a list of recipes not found in the main list of available recipes.
/// </summary>
[ByRefEvent]
public struct GetSecretRecipesEvent()
{
    public List<FoodRecipePrototype> Recipes = new();
}
