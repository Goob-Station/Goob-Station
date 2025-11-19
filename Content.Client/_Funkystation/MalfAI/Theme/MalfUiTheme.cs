// SPDX-FileCopyrightText: 2025 Dreykor <160512778+Dreykor@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 funkystationbot <funky@funkystation.org>
//
// SPDX-License-Identifier: MIT

using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Content.Client.Resources;

namespace Content.Client._Funkystation.MalfAI.Theme;

/// <summary>
/// Centralized Malf AI UI theme utilities (accent color, font, and style factories).
/// Use these helpers to apply the Malf theme consistently across different UIs.
/// </summary>
public static class MalfUiTheme
{
    /// <summary>
    /// Malf accent color (green).
    /// </summary>
    public static readonly Color Accent = new(0f, 1f, 0f);

    /// <summary>
    /// Path to the KodeMono font used in Malf-themed UIs.
    /// </summary>
    public const string FontPath = "/Fonts/_DV/KodeMono/KodeMono-Regular.ttf";

    // Cached stylesheets for common font sizes to avoid repeated allocations
    private static Stylesheet? _cachedStylesheet12;
    private static Stylesheet? _cachedStylesheet14;
    private static Stylesheet? _cachedStylesheet16;
    private static readonly object _cacheLock = new();

    /// <summary>
    /// Loads the Malf font from resources.
    /// </summary>
    public static Font GetFont(IResourceCache cache, int size = 12)
        => cache.GetFont(FontPath, size);

    /// <summary>
    /// Creates the main black panel style with a green border.
    /// </summary>
    public static StyleBoxFlat CreateMainPanelStyle(Color? accent = null)
    {
        var a = accent ?? Accent;
        var style = new StyleBoxFlat
        {
            BackgroundColor = Color.Black,
            BorderColor = a,
            BorderThickness = new Thickness(2f)
        };
        style.ContentMarginLeftOverride = 6;
        style.ContentMarginTopOverride = 4;
        style.ContentMarginRightOverride = 6;
        style.ContentMarginBottomOverride = 4;
        return style;
    }

    /// <summary>
    /// Creates the category panel style (very dark background with green border).
    /// </summary>
    public static StyleBoxFlat CreateCategoryPanelStyle(Color? accent = null)
    {
        var a = accent ?? Accent;
        var style = new StyleBoxFlat
        {
            BackgroundColor = new Color(0.02f, 0.02f, 0.03f, 1f),
            BorderColor = a,
            BorderThickness = new Thickness(2f)
        };
        style.ContentMarginLeftOverride = 4;
        style.ContentMarginTopOverride = 4;
        style.ContentMarginRightOverride = 4;
        style.ContentMarginBottomOverride = 4;
        return style;
    }

    /// <summary>
    /// Creates a green-bordered black button style.
    /// </summary>
    public static StyleBoxFlat CreateButtonStyle(Color? accent = null)
    {
        var a = accent ?? Accent;
        var style = new StyleBoxFlat
        {
            BackgroundColor = Color.Black,
            BorderColor = a,
            BorderThickness = new Thickness(2f)
        };
        style.ContentMarginLeftOverride = 4;
        style.ContentMarginTopOverride = 2;
        style.ContentMarginRightOverride = 4;
        style.ContentMarginBottomOverride = 2;
        return style;
    }

    /// <summary>
    /// Creates a solid black backdrop (no border) for window backgrounds.
    /// </summary>
    public static StyleBoxFlat CreateBackdropStyle()
    {
        return new StyleBoxFlat
        {
            BackgroundColor = Color.Black
        };
    }

    /// <summary>
    /// Creates a black list entry panel with a green outline for Malf-themed lists.
    /// </summary>
    public static StyleBoxFlat CreateEntryPanelStyle(Color? accent = null)
    {
        var a = accent ?? Accent;
        var style = new StyleBoxFlat
        {
            BackgroundColor = Color.Black,
            BorderColor = a,
            BorderThickness = new Thickness(2f)
        };
        style.ContentMarginLeftOverride = 6;
        style.ContentMarginTopOverride = 6;
        style.ContentMarginRightOverride = 6;
        style.ContentMarginBottomOverride = 6;
        return style;
    }

    /// <summary>
    /// Creates a hollow square checkbox style for the inactive state (transparent background with green border).
    /// </summary>
    public static StyleBoxFlat CreateCheckboxInactiveStyle(Color? accent = null)
    {
        var a = accent ?? Accent;
        return new StyleBoxFlat
        {
            BackgroundColor = Color.Transparent,
            BorderColor = a,
            BorderThickness = new Thickness(2f)
        };
    }

    /// <summary>
    /// Creates a filled square checkbox style for the active state (green background with green border).
    /// </summary>
    public static StyleBoxFlat CreateCheckboxActiveStyle(Color? accent = null)
    {
        var a = accent ?? Accent;
        return new StyleBoxFlat
        {
            BackgroundColor = a,
            BorderColor = a,
            BorderThickness = new Thickness(2f)
        };
    }

    /// <summary>
    /// Gets a cached stylesheet for common font sizes, or creates one for uncommon sizes.
    /// This avoids repeated allocations for frequently used stylesheets.
    /// </summary>
    public static Stylesheet GetCachedStylesheet(IResourceCache cache, int fontSize = 12)
    {
        // Use cached stylesheets for common sizes
        lock (_cacheLock)
        {
            return fontSize switch
            {
                12 => _cachedStylesheet12 ??= CreateStylesheet(cache, 12),
                14 => _cachedStylesheet14 ??= CreateStylesheet(cache, 14),
                16 => _cachedStylesheet16 ??= CreateStylesheet(cache, 16),
                _ => CreateStylesheet(cache, fontSize) // Create new for uncommon sizes
            };
        }
    }

    /// centralized stylesheet for all Malf AI themed windows and controls
    public static Stylesheet CreateStylesheet(IResourceCache cache, int fontSize = 12)
    {
        var font = GetFont(cache, fontSize);
        var accent = Accent;
        var buttonStyle = CreateButtonStyle(accent);
        var transparentStyle = new StyleBoxFlat
        {
            BackgroundColor = Color.Transparent,
            BorderColor = Color.Transparent,
            BorderThickness = new Thickness(0f)
        };
        transparentStyle.ContentMarginLeftOverride = 0;
        transparentStyle.ContentMarginTopOverride = 0;
        transparentStyle.ContentMarginRightOverride = 0;
        transparentStyle.ContentMarginBottomOverride = 0;

        return new Stylesheet(new[]
        {
            // Font styling for all text controls
            new StyleRule(new SelectorElement(typeof(Label), null, null, null),
                new[] { new StyleProperty("font", font), new StyleProperty("font-color", accent) }),
            new StyleRule(new SelectorElement(typeof(RichTextLabel), null, null, null),
                new[] { new StyleProperty("font", font), new StyleProperty("font-color", accent) }),
            new StyleRule(new SelectorElement(typeof(LineEdit), null, null, null),
                new[] { new StyleProperty("font", font), new StyleProperty("font-color", accent), new StyleProperty("stylebox", buttonStyle) }),
            new StyleRule(new SelectorElement(typeof(LineEdit), null, "placeholder", null),
                new[] { new StyleProperty("font-color", Color.Transparent) }),

            // Button styling
            new StyleRule(new SelectorElement(typeof(Button), null, null, null),
                new[] { new StyleProperty("stylebox", transparentStyle) }),

            // CheckBox styling
            new StyleRule(new SelectorElement(typeof(CheckBox), null, null, null),
                new[] { new StyleProperty("font", font), new StyleProperty("font-color", accent) })
        });
    }

}
