// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Construction;
using Content.Client.Construction.UI;
using Content.Goobstation.Shared.Factory;
using Content.Shared.Construction.Prototypes;
using Content.Shared.Whitelist;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Goobstation.Client.Factory.UI;

public sealed class ConstructorBUI : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    private readonly ConstructionSystem _construction;
    private readonly EntityWhitelistSystem _whitelist;
    private readonly SpriteSystem _sprite;

    private ConstructionMenu? _menu;
    private string? _id;
    private List<ConstructionPrototype> _recipes = new();
    private readonly LocId _favoriteCatName = "construction-category-favorites";
    private readonly LocId _forAllCategoryName = "construction-category-all";

    public ConstructorBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _construction = EntMan.System<ConstructionSystem>();
        _whitelist = EntMan.System<EntityWhitelistSystem>();
        _sprite = EntMan.System<SpriteSystem>();

        _id = EntMan.GetComponentOrNull<ConstructorComponent>(owner)?.Construction;
    }

    protected override void Open()
    {
        base.Open();

        // god BLESS whoever made construction ui for having it so decoupled <3
        _menu = this.CreateWindow<ConstructionMenu>();
        PopulateCategories();
        PopulateRecipes(string.Empty, string.Empty);
        _menu.PopulateRecipes += (_, args) => PopulateRecipes(args.Item1, args.Item2);
        _menu.RecipeSelected += (_, item) =>
        {
            _menu.ClearRecipeInfo();
            if (item?.Metadata is ConstructionPrototype proto)
            {
                _id = proto.ID;
                _menu.SetRecipeInfo(proto.Name, proto.Description, _sprite.Frame0(proto.Icon),
                    proto.Type != ConstructionType.Item, true); // TODO: favourites

                GenerateStepList(proto);
            }
            else
            {
                _id = null;
            }
        };
        _menu.BuildButtonToggled += (_, _) =>
        {
            SendPredictedMessage(new ConstructorSetProtoMessage(_id));
            _menu.Close();
        };
    }

    private void PopulateCategories(string? selected = null)
    {
        if (_menu is not {} menu)
            return;

        var categories = new HashSet<string>();

        foreach (var prototype in _proto.EnumeratePrototypes<ConstructionPrototype>())
        {
            var category = prototype.Category;

            if (!string.IsNullOrEmpty(category))
                categories.Add(category);
        }

        var categoriesArray = new string[categories.Count + 1];

        // hard-coded to show all recipes
        var idx = 0;
        categoriesArray[idx++] = _forAllCategoryName;

        foreach (var cat in categories.OrderBy(Loc.GetString))
        {
            categoriesArray[idx++] = cat;
        }

        menu.OptionCategories.Clear();

        for (var i = 0; i < categoriesArray.Length; i++)
        {
            menu.OptionCategories.AddItem(Loc.GetString(categoriesArray[i]), i);

            if (!string.IsNullOrEmpty(selected) && selected == categoriesArray[i])
                menu.OptionCategories.SelectId(i);
        }

        menu.Categories = categoriesArray;
    }

    // copypasted and optimised from ConstructionMenuPresenter
    private void PopulateRecipes(string search, string category)
    {
        if (PlayerManager.LocalEntity is not { } user
            || _menu is not { } menu)
            return;

        search = search.Trim().ToLowerInvariant();
        var searching = !string.IsNullOrEmpty(search);
        var isEmptyCategory = string.IsNullOrEmpty(category) || category == _forAllCategoryName;

        _recipes.Clear();
        foreach (var recipe in _proto.EnumeratePrototypes<ConstructionPrototype>())
        {
            if (recipe.Hide)
                continue;

            if (_whitelist.IsWhitelistFail(recipe.EntityWhitelist, user))
                continue;

            if (searching 
                && !recipe.Name.ToLowerInvariant().Contains(search))
                continue;

            if (!isEmptyCategory)
            {
                // TODO: when favourites get sent from server do this
                // currently its specific to the G menu
                //if (!_favoritedRecipes.Contains(recipe))
                if (category == _favoriteCatName)
                    continue;
                else if (recipe.Category != category)
                    continue;
            }

            _recipes.Add(recipe);
        }

        _recipes.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.InvariantCulture));

        var recipesList = menu.Recipes;
        recipesList.Clear();

        menu.RecipesGridScrollContainer.Visible = false;
        menu.Recipes.Visible = true;

        // no grid because fuck you
        foreach (var recipe in _recipes)
            recipesList.Add(GetItem(recipe, recipesList));
    }

    private ItemList.Item GetItem(ConstructionPrototype recipe, ItemList itemList)
    {
        return new(itemList)
        {
            Metadata = recipe,
            Text = recipe.Name,
            Icon = _sprite.Frame0(recipe.Icon),
            TooltipEnabled = true,
            TooltipText = recipe.Description,
        };
    }

    private void GenerateStepList(ConstructionPrototype proto)
    {
        if (_construction.GetGuide(proto) is not { } guide
            || _menu is not { } menu)
            return;

        var list = menu.RecipeStepList;
        foreach (var entry in guide.Entries)
        {
            var text = entry.Arguments != null
                ? Loc.GetString(entry.Localization, entry.Arguments)
                : Loc.GetString(entry.Localization);

            if (entry.EntryNumber is { } number)
                text = Loc.GetString("construction-presenter-step-wrapper",
                    ("step-number", number), ("text", text));

            // The padding needs to be applied regardless of text length... (See PadLeft documentation)
            text = text.PadLeft(text.Length + entry.Padding);

            var icon = entry.Icon != null ? _sprite.Frame0(entry.Icon) : Texture.Transparent;
            list.AddItem(text, icon, false);
        }
    }
}
