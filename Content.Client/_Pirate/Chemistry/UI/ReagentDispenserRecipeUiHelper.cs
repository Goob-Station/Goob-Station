using System.Linq;
using System.Numerics;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Administration;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Reagent;
using Robust.Client.Audio;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.Chemistry.UI;

public static class ReagentDispenserRecipeUiHelper
{
    public const float RecipeActionButtonSize = 28f;
    public const float RecipeChipHeight = 28f;
    public const float RecipeChipActionButtonSize = 20f;
    public const float RecipeChipColorLineWidth = 7f;
    public const int RecipeChipColumns = 3;
    public const float RecipeChipWidthCompensation = 1f;

    private static readonly SoundSpecifier InterfaceClickSound = new SoundPathSpecifier("/Audio/UserInterface/click.ogg");
    private const string DefaultDeleteIconPath = "/Textures/_Pirate/Interface/VerbIcons/ChemRecipes/recipe-delete.svg.192dpi.png";
    private const string RecordIconPath = "/Textures/_Pirate/Interface/VerbIcons/ChemRecipes/recipe-record.svg.192dpi.png";
    private const string CancelIconPath = "/Textures/_Pirate/Interface/VerbIcons/ChemRecipes/recipe-cancel.svg.192dpi.png";
    private const string SaveToDiskIconPath = "/Textures/_Pirate/Interface/VerbIcons/ChemRecipes/recipe-download-from-disk.svg.192dpi.png";
    private const string CopyFromDiskIconPath = "/Textures/_Pirate/Interface/VerbIcons/ChemRecipes/recipe-save-to-disk.svg.192dpi.png";

    public readonly record struct RecipeUiState(
        bool IsRecordingRecipe,
        IReadOnlyCollection<ReagentQuantity> RecordingRecipeReagents,
        IReadOnlyCollection<ReagentDispenserRecipeItem> SavedRecipes,
        bool HasRecipeDisk,
        IReadOnlyCollection<ReagentDispenserRecipeItem> DiskRecipes);

    public readonly record struct RecipeUiCallbacks(
        Action<string>? OnDispenseRecipePressed,
        Action<string>? OnSaveRecipeToDiskPressed,
        Action<string>? OnDeleteRecipePressed,
        Action<string>? OnDispenseDiskRecipePressed,
        Action<string>? OnCopyDiskRecipePressed,
        Action<string>? OnDeleteDiskRecipePressed);

    public static void ConfigureRecipeActionButton(Button button, string texturePath)
    {
        button.Text = string.Empty;
        button.HorizontalExpand = false;
        button.MinWidth = RecipeActionButtonSize;
        button.SetWidth = RecipeActionButtonSize;
        button.MinHeight = RecipeActionButtonSize;
        button.SetHeight = RecipeActionButtonSize;
        button.RemoveAllChildren();
        button.AddChild(new TextureRect
        {
            TexturePath = texturePath,
            MinSize = new Vector2(16f, 16f),
            MaxSize = new Vector2(16f, 16f),
            Stretch = TextureRect.StretchMode.KeepAspectCentered,
            HorizontalAlignment = Control.HAlignment.Center,
            VerticalAlignment = Control.VAlignment.Center
        });
    }

    public static void ConfigureRecordingButton(Button recordRecipeButton, bool isRecordingRecipe, ref bool? recordButtonIsCancel)
    {
        if (recordButtonIsCancel == isRecordingRecipe)
            return;

        recordButtonIsCancel = isRecordingRecipe;
        ConfigureRecipeActionButton(recordRecipeButton, isRecordingRecipe ? CancelIconPath : RecordIconPath);
        recordRecipeButton.ToolTip = Loc.GetString(
            isRecordingRecipe
                ? "reagent-dispenser-window-recipes-cancel-tooltip"
                : "reagent-dispenser-window-recipes-record-tooltip");
    }

    public static FixedPoint2 GetRecordingTotalVolume(IReadOnlyCollection<ReagentQuantity> reagents)
    {
        return reagents.Aggregate(FixedPoint2.Zero, (current, reagent) => current + reagent.Quantity);
    }

    public static void UpdateVirtualRecordingContents(
        IPrototypeManager prototypeManager,
        BoxContainer containerInfo,
        IReadOnlyCollection<ReagentQuantity> reagents)
    {
        if (reagents.Count == 0)
        {
            containerInfo.Children.Add(new Label
            {
                Text = Loc.GetString("reagent-dispenser-window-recipes-virtual-container-empty"),
                StyleClasses = { StyleClass.LabelWeak },
            });
            return;
        }

        foreach (var reagent in reagents.OrderBy(r => r.Reagent.Prototype))
        {
            var localizedName = prototypeManager.TryIndex(reagent.Reagent.Prototype, out ReagentPrototype? p)
                ? p.LocalizedName
                : Loc.GetString("reagent-dispenser-window-reagent-name-not-found-text");

            var nameLabel = new Label { Text = $"{localizedName}: " };
            var quantityLabel = new Label
            {
                Text = Loc.GetString("reagent-dispenser-window-quantity-label-text", ("quantity", reagent.Quantity)),
                StyleClasses = { StyleClass.LabelWeak },
            };

            containerInfo.Children.Add(new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                Children = { nameLabel, quantityLabel },
            });
        }
    }

    public static void UpdateRecipeSection(
        RecipeUiState state,
        RecipeUiCallbacks callbacks,
        Button recordRecipeButton,
        Button saveRecipeButton,
        Button clearRecipesButton,
        Button ejectRecipeDiskButton,
        Control recipeDiskSection,
        GridContainer recipeList,
        GridContainer recipeDiskList,
        ref bool? recordButtonIsCancel)
    {
        ConfigureRecordingButton(recordRecipeButton, state.IsRecordingRecipe, ref recordButtonIsCancel);
        saveRecipeButton.Disabled = !state.IsRecordingRecipe || state.RecordingRecipeReagents.Count == 0;
        clearRecipesButton.Disabled = state.SavedRecipes.Count == 0;
        recipeDiskSection.Visible = state.HasRecipeDisk;
        ejectRecipeDiskButton.Disabled = !state.HasRecipeDisk;

        recipeList.Children.Clear();
        foreach (var recipe in state.SavedRecipes)
        {
            var chip = CreateRecipeChip(
                recipe,
                !state.HasRecipeDisk,
                callbacks.OnDispenseRecipePressed,
                callbacks.OnSaveRecipeToDiskPressed,
                callbacks.OnDeleteRecipePressed,
                Loc.GetString("reagent-dispenser-window-recipes-save-to-disk-tooltip"),
                Loc.GetString("reagent-dispenser-window-recipes-delete-tooltip"),
                SaveToDiskIconPath);
            recipeList.AddChild(chip);
        }
        ApplyRecipeChipWidths(recipeList);

        if (state.SavedRecipes.Count == 0)
        {
            recipeList.AddChild(new Label
            {
                Text = Loc.GetString("reagent-dispenser-window-recipes-none"),
                StyleClasses = { StyleClass.LabelWeak },
            });
        }

        recipeDiskList.Children.Clear();
        if (!state.HasRecipeDisk)
            return;

        foreach (var recipe in state.DiskRecipes)
        {
            var chip = CreateRecipeChip(
                recipe,
                false,
                callbacks.OnDispenseDiskRecipePressed,
                callbacks.OnCopyDiskRecipePressed,
                callbacks.OnDeleteDiskRecipePressed,
                Loc.GetString("reagent-dispenser-window-recipes-copy-from-disk-tooltip"),
                Loc.GetString("reagent-dispenser-window-recipes-delete-disk-tooltip"),
                CopyFromDiskIconPath,
                Loc.GetString("reagent-dispenser-window-recipes-copy-from-disk-tooltip"));
            recipeDiskList.AddChild(chip);
        }
        ApplyRecipeChipWidths(recipeDiskList);

        if (state.DiskRecipes.Count == 0)
        {
            recipeDiskList.AddChild(new Label
            {
                Text = Loc.GetString("reagent-dispenser-window-recipes-disk-none"),
                StyleClasses = { StyleClass.LabelWeak },
            });
        }
    }

    public static void PlayRecipeUiClickSound(AudioSystem audioSystem)
    {
        audioSystem.PlayGlobal(InterfaceClickSound, Filter.Local(), false, AudioParams.Default.WithVolume(-2f));
    }

    public static void OpenSaveRecipeDialog(DialogWindow? saveRecipeDialog, int recipeNameMaxLength, Action<string>? onSaveRecipePressed, Action<DialogWindow?> setSaveRecipeDialog)
    {
        if (saveRecipeDialog != null)
        {
            saveRecipeDialog.MoveToFront();
            return;
        }

        const string field = "name";
        var entry = new QuickDialogEntry(
            field,
            QuickDialogEntryType.ShortText,
            Loc.GetString("reagent-dispenser-window-recipes-save-dialog-prompt"),
            Loc.GetString("reagent-dispenser-window-recipes-save-dialog-placeholder"));

        var dialog = new DialogWindow(
            Loc.GetString("reagent-dispenser-window-recipes-save-dialog-title"),
            new List<QuickDialogEntry> { entry });
        dialog.MinHeight = 0f;
        dialog.SetHeight = float.NaN;

        dialog.OnConfirmed += responses =>
        {
            var name = responses[field].Trim();
            if (name.Length > recipeNameMaxLength)
                name = name[..recipeNameMaxLength];

            if (!string.IsNullOrEmpty(name))
                onSaveRecipePressed?.Invoke(name);
        };

        setSaveRecipeDialog(dialog);
        dialog.OnClose += () => setSaveRecipeDialog(null);
    }

    public static BoxContainer CreateRecipeChip(
        ReagentDispenserRecipeItem recipe,
        bool secondaryDisabled,
        Action<string>? onPrimaryPressed,
        Action<string>? onSecondaryPressed,
        Action<string>? onDeletePressed,
        string secondaryTooltip,
        string deleteTooltip,
        string secondaryIconPath,
        string? primaryTooltip = null,
        string? deleteIconPath = null)
    {
        var chip = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            HorizontalExpand = false,
        };

        chip.AddChild(CreateRecipeColorSwatch(recipe.Color));

        var nameButton = new Button
        {
            Text = recipe.Name,
            StyleClasses = { StyleClass.ButtonSquare },
            HorizontalExpand = true,
            MinHeight = RecipeChipHeight,
            SetHeight = RecipeChipHeight,
            ClipText = true,
            Margin = new Thickness(-1f, 0f, 0f, 0f),
        };

        if (primaryTooltip != null)
            nameButton.ToolTip = primaryTooltip;

        nameButton.OnPressed += _ => onPrimaryPressed?.Invoke(recipe.Name);

        var secondaryButton = new Button
        {
            HorizontalExpand = false,
            StyleClasses = { StyleClass.ButtonSquare },
            MinWidth = RecipeChipActionButtonSize,
            SetWidth = RecipeChipActionButtonSize,
            Disabled = secondaryDisabled,
            ToolTip = secondaryTooltip,
            Margin = new Thickness(-1f, 0f, 0f, 0f),
        };
        ConfigureCompactIconButton(secondaryButton, secondaryIconPath);
        secondaryButton.OnPressed += _ => onSecondaryPressed?.Invoke(recipe.Name);

        var deleteButton = new Button
        {
            HorizontalExpand = false,
            StyleClasses = { StyleClass.ButtonSquare },
            MinWidth = RecipeChipActionButtonSize,
            SetWidth = RecipeChipActionButtonSize,
            ToolTip = deleteTooltip,
            Margin = new Thickness(-1f, 0f, 0f, 0f),
        };
        ConfigureCompactIconButton(deleteButton, deleteIconPath ?? DefaultDeleteIconPath);
        deleteButton.OnPressed += _ => onDeletePressed?.Invoke(recipe.Name);

        chip.AddChild(nameButton);
        chip.AddChild(secondaryButton);
        chip.AddChild(deleteButton);
        return chip;
    }

    public static void ApplyRecipeChipWidths(GridContainer container)
    {
        if (container.PixelWidth <= 0)
            return;

        var hSeparation = container.HSeparationOverride ?? 4;
        var totalSeparation = hSeparation * (RecipeChipColumns - 1);
        var chipWidth = MathF.Max(0f, (container.PixelWidth - totalSeparation) / RecipeChipColumns) + RecipeChipWidthCompensation;
        foreach (var child in container.Children)
        {
            if (child is not BoxContainer chip)
                continue;

            chip.MinWidth = chipWidth;
            chip.MaxWidth = chipWidth;
            chip.SetWidth = chipWidth;
        }
    }

    private static void ConfigureCompactIconButton(Button button, string texturePath)
    {
        button.Text = string.Empty;
        button.MinHeight = RecipeChipHeight;
        button.SetHeight = RecipeChipHeight;
        button.RemoveAllChildren();
        button.AddChild(new TextureRect
        {
            TexturePath = texturePath,
            MinSize = new Vector2(16f, 16f),
            MaxSize = new Vector2(16f, 16f),
            Stretch = TextureRect.StretchMode.KeepAspectCentered,
            HorizontalAlignment = Control.HAlignment.Center,
            VerticalAlignment = Control.VAlignment.Center
        });
    }

    private static PanelContainer CreateRecipeColorSwatch(Color color)
    {
        return new PanelContainer
        {
            VerticalExpand = true,
            SetWidth = RecipeChipColorLineWidth,
            Margin = new Thickness(0f, 1f, 0f, 0f),
            PanelOverride = new StyleBoxFlat { BackgroundColor = color },
        };
    }
}
