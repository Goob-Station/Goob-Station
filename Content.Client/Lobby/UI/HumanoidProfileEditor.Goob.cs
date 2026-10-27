using System.Linq;
using System.Numerics;
using Content.Client.Humanoid;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.UIKit.UserInterface.Lobby;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Lobby.UI;

public sealed partial class HumanoidProfileEditor
{
    private static readonly Color[] EyePresets =
    {
        Color.FromHex("#3b2a1f"), Color.FromHex("#6b4a2b"), Color.FromHex("#4a6b3f"), Color.FromHex("#3f5f8a"),
        Color.FromHex("#6f8aa8"), Color.FromHex("#7a7a7a"), Color.FromHex("#a83a3a"), Color.FromHex("#d8b030"),
    };

    private static readonly float[] SkinTonePresets = { 0, 15, 30, 45, 60, 75, 90, 100 };

    private bool _showFacialHair;
    private bool _syncingAge;

    private void InitializeAgeGoob()
    {
        AgeSlider.OnValueChanged += _ =>
        {
            if (_syncingAge)
                return;

            var age = (int) AgeSlider.Value;
            AgeEdit.Text = age.ToString();
            UpdateAgeBracket(age);

            if (!AgeSlider.Grabbed)
                SetAge(age);
        };

        AgeSlider.OnReleased += _ => SetAge((int) AgeSlider.Value);

        AgeEdit.OnTextChanged += args =>
        {
            if (int.TryParse(args.Text, out var age))
                SyncAgeSlider(age);
        };
    }

    private void UpdateAgeSlider()
    {
        if (Profile == null || !_prototypeManager.TryIndex<SpeciesPrototype>(Profile.Species, out var species))
            return;

        _syncingAge = true;
        AgeSlider.MinValue = species.MinAge;
        AgeSlider.MaxValue = species.MaxAge;
        _syncingAge = false;

        SyncAgeSlider(Profile.Age);
    }

    private void SyncAgeSlider(int age)
    {
        _syncingAge = true;
        AgeSlider.Value = age;
        _syncingAge = false;
        UpdateAgeBracket(age);
    }

    private void UpdateAgeBracket(int age)
    {
        if (Profile == null || !_prototypeManager.TryIndex<SpeciesPrototype>(Profile.Species, out var species))
        {
            AgeBracketLabel.Text = string.Empty;
            return;
        }

        var bracket = age < species.YoungAge ? "young" : age >= species.OldAge ? "old" : "adult";
        AgeBracketLabel.Text = Loc.GetString($"humanoid-profile-editor-age-{bracket}");
    }

    private void InitializeHairGoob()
    {
        HideLockedJobsCheck.OnToggled += _ => ApplyJobFilter();

        AppearanceBox.OnResized += () => BodyScroll.MaxHeight = Math.Clamp(AppearanceBox.Height * 0.5f, 120f, 360f);

        HairTabButton.OnPressed += _ => ShowHairTab(false);
        FacialHairTabButton.OnPressed += _ => ShowHairTab(true);

        var match = _cfgManager.GetCVar(GoobCVars.FacialHairMatchesHair);
        HairStylePicker.MatchButton.Pressed = match;
        FacialHairPicker.MatchButton.Pressed = match;
        HairStylePicker.MatchButton.OnToggled += args => SetHairColorMatch(args.Pressed);
        FacialHairPicker.MatchButton.OnToggled += args => SetHairColorMatch(args.Pressed);

        HairStylePicker.OnColorChanged += args => OnHairColorMatch(MarkingCategories.Hair, args.marking);
        FacialHairPicker.OnColorChanged += args => OnHairColorMatch(MarkingCategories.FacialHair, args.marking);

        HairStylePicker.OnMarkingHover += id => PreviewHair(MarkingCategories.Hair, id);
        FacialHairPicker.OnMarkingHover += id => PreviewHair(MarkingCategories.FacialHair, id);
        HairStylePicker.OnMarkingHoverEnd += EndHairPreview;
        FacialHairPicker.OnMarkingHoverEnd += EndHairPreview;

        Markings.OnMarkingPreview += PreviewMarkings;
        Markings.OnMarkingPreviewEnd += EndHairPreview;
    }

    private void PreviewMarkings(MarkingSet set)
    {
        if (Profile == null || !_entManager.EntityExists(PreviewDummy))
            return;

        var appearance = Profile.Appearance.WithMarkings(set.GetForwardEnumerator().ToList());
        _entManager.System<HumanoidAppearanceSystem>().LoadProfile(PreviewDummy, Profile.WithCharacterAppearance(appearance));
    }

    private void UpdateHairTabs()
    {
        var hasHair = HairStylePicker.OptionCount > 0;
        var hasFacial = FacialHairPicker.OptionCount > 0;

        HairSection.Visible = hasHair || hasFacial;
        HairTabButton.Visible = hasHair;
        FacialHairTabButton.Visible = hasFacial;
        HairStylePicker.MatchButton.Visible = hasHair && hasFacial;
        FacialHairPicker.MatchButton.Visible = hasHair && hasFacial;

        HairTabButton.Text = Loc.GetString("humanoid-profile-editor-hair-tab", ("count", HairStylePicker.OptionCount));
        FacialHairTabButton.Text = Loc.GetString("humanoid-profile-editor-facial-hair-tab", ("count", FacialHairPicker.OptionCount));

        if (_showFacialHair && !hasFacial)
            _showFacialHair = false;
        else if (!_showFacialHair && !hasHair && hasFacial)
            _showFacialHair = true;

        ShowHairTab(_showFacialHair);
    }

    private void ShowHairTab(bool facial)
    {
        _showFacialHair = facial;
        HairStylePicker.Visible = !facial && HairStylePicker.OptionCount > 0;
        FacialHairPicker.Visible = facial && FacialHairPicker.OptionCount > 0;
        HairTabButton.Pressed = !facial;
        FacialHairTabButton.Pressed = facial;
    }

    private void UpdateMarkingTileColors()
    {
        if (Profile == null)
            return;

        UpdateColorSwatches();

        HairStylePicker.SetPreviewProfile(Profile);
        FacialHairPicker.SetPreviewProfile(Profile);
        Markings.SetPreviewProfile(Profile);

        var appearance = Profile.Appearance;
        Markings.SetAppearanceColors(appearance.SkinColor, appearance.EyeColor);
        HairStylePicker.SetAppearanceColors(appearance.SkinColor, appearance.EyeColor, appearance.HairColor);
        FacialHairPicker.SetAppearanceColors(appearance.SkinColor, appearance.EyeColor, appearance.FacialHairColor);
    }

    private void SetHairColorMatch(bool match)
    {
        HairStylePicker.MatchButton.Pressed = match;
        FacialHairPicker.MatchButton.Pressed = match;
        _cfgManager.SetCVar(GoobCVars.FacialHairMatchesHair, match);
        _cfgManager.SaveToFile();
    }

    private void OnHairColorMatch(MarkingCategories changed, Marking marking)
    {
        if (Profile is null || !HairStylePicker.MatchButton.Pressed || !HairStylePicker.MatchButton.Visible)
            return;

        var color = marking.MarkingColors[0];
        if (changed == MarkingCategories.Hair)
        {
            Profile = Profile.WithCharacterAppearance(Profile.Appearance.WithFacialHairColor(color));
            UpdateCMarkingsFacialHair();

            var facialHair = Profile.Appearance.FacialHairStyleId == HairStyles.DefaultFacialHairStyle
                ? new List<Marking>()
                : new List<Marking> { new(Profile.Appearance.FacialHairStyleId, new List<Color> { color }) };
            FacialHairPicker.UpdateData(facialHair, Profile.Species, 1);
        }
        else
        {
            Profile = Profile.WithCharacterAppearance(Profile.Appearance.WithHairColor(color));
            UpdateCMarkingsHair();

            var hair = Profile.Appearance.HairStyleId == HairStyles.DefaultHairStyle
                ? new List<Marking>()
                : new List<Marking> { new(Profile.Appearance.HairStyleId, new List<Color> { color }) };
            HairStylePicker.UpdateData(hair, Profile.Species, 1);
        }

        ReloadProfilePreview();
    }

    private void PreviewHair(MarkingCategories category, string? id)
    {
        if (Profile == null || !_entManager.EntityExists(PreviewDummy))
            return;

        var appearance = category == MarkingCategories.FacialHair
            ? Profile.Appearance.WithFacialHairStyleName(id ?? HairStyles.DefaultFacialHairStyle)
            : Profile.Appearance.WithHairStyleName(id ?? HairStyles.DefaultHairStyle);

        _entManager.System<HumanoidAppearanceSystem>().LoadProfile(PreviewDummy, Profile.WithCharacterAppearance(appearance));
    }

    private void EndHairPreview()
    {
        if (Profile == null || !_entManager.EntityExists(PreviewDummy))
            return;

        _entManager.System<HumanoidAppearanceSystem>().LoadProfile(PreviewDummy, Profile);
    }

    private void InitializeColorsGoob()
    {
        foreach (var color in EyePresets)
        {
            var swatch = new ColorSwatch(color);
            swatch.OnPressed += _ => SetEyeColorGoob(color);
            EyeSwatches.AddChild(swatch);
        }

        EyeCustomButton.OnToggled += args => EyeColorPicker.Visible = args.Pressed;
    }

    private void SetEyeColorGoob(Color color)
    {
        if (Profile is null)
            return;

        EyeColorPicker.SetData(color);
        Profile = Profile.WithCharacterAppearance(Profile.Appearance.WithEyeColor(color));
        Markings.CurrentEyeColor = color;
        ReloadProfilePreview();
    }

    private void UpdateSkinTrack()
    {
        SkinSwatches.RemoveAllChildren();

        if (Profile == null
            || !_prototypeManager.TryIndex<SpeciesPrototype>(Profile.Species, out var species)
            || !_prototypeManager.TryIndex(species.SkinColoration, out var coloration))
        {
            return;
        }

        var strategy = coloration.Strategy;
        if (strategy.InputType == SkinColorationStrategyInput.Unary)
        {
            Skin.ColorAt = value => strategy.FromUnary(value);
            foreach (var tone in SkinTonePresets)
            {
                var swatch = new ColorSwatch(strategy.FromUnary(tone));
                swatch.OnPressed += _ => Skin.Value = tone;
                SkinSwatches.AddChild(swatch);
            }
        }
        else
        {
            Skin.ColorAt = null;
            var seen = new HashSet<string>();
            AddSkinSwatch(strategy.EnsureVerified(species.DefaultSkinTone));
            for (var i = 0; i < 10; i++)
            {
                AddSkinSwatch(strategy.ClosestSkinColor(Color.FromHsv(new Vector4(i / 10f, 0.55f, 0.75f, 1f))));
            }

            void AddSkinSwatch(Color color)
            {
                if (!seen.Add(color.ToHex()))
                    return;

                var swatch = new ColorSwatch(color);
                swatch.OnPressed += _ =>
                {
                    _rgbSkinColorSelector.Color = color;
                    OnSkinColorOnValueChanged();
                };
                SkinSwatches.AddChild(swatch);
            }
        }

        UpdateColorSwatches();
    }

    private void UpdateColorSwatches()
    {
        if (Profile == null)
            return;

        foreach (var child in SkinSwatches.Children)
        {
            if (child is ColorSwatch swatch)
                swatch.Pressed = swatch.Matches(Profile.Appearance.SkinColor);
        }

        foreach (var child in EyeSwatches.Children)
        {
            if (child is ColorSwatch swatch)
                swatch.Pressed = swatch.Matches(Profile.Appearance.EyeColor);
        }
    }

    private void ApplyJobFilter()
    {
        var hide = HideLockedJobsCheck.Pressed;

        foreach (var (_, selector) in _jobPriorities)
        {
            if (selector.Parent is { } row)
                row.Visible = !hide || !selector.Locked;
        }

        foreach (var category in _jobCategories.Values)
        {
            var anyVisible = false;
            foreach (var child in category.Children)
            {
                if (child is BoxContainer row && row.Visible && row.ChildCount > 0 && row.GetChild(0) is Roles.RequirementsSelector)
                    anyVisible = true;
            }

            category.Visible = !hide || anyVisible;
        }
    }

    private Control CreateTraitBudget(int used, int max)
    {
        var row = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 8,
            Margin = new Thickness(0, 2, 0, 6),
        };

        var meter = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SetSize = new Vector2(140, 8),
            VerticalAlignment = VAlignment.Center,
        };

        var filled = Math.Clamp(used, 0, max);
        if (filled > 0)
        {
            meter.AddChild(new PanelContainer
            {
                HorizontalExpand = true,
                SizeFlagsStretchRatio = filled,
                PanelOverride = new StyleBoxFlat { BackgroundColor = HazardStripe.StripeColor },
            });
        }

        if (max - filled > 0)
        {
            meter.AddChild(new PanelContainer
            {
                HorizontalExpand = true,
                SizeFlagsStretchRatio = max - filled,
                PanelOverride = new StyleBoxFlat { BackgroundColor = Color.FromHex("#1b1e21"), BorderColor = HullPanel.EdgeColor, BorderThickness = new Thickness(1) },
            });
        }

        row.AddChild(meter);
        row.AddChild(new Label
        {
            Text = Loc.GetString("humanoid-profile-editor-trait-count-hint", ("current", used), ("max", max)),
            StyleClasses = { HullStyleClasses.FieldLabel },
        });

        return row;
    }
}
