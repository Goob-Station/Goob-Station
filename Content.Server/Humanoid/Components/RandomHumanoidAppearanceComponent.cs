// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Humanoid; // Pirate Change: Added using for Sex

namespace Content.Server.CharacterAppearance.Components;

[RegisterComponent]
public sealed partial class RandomHumanoidAppearanceComponent : Component
{
    [DataField("randomizeName")] public bool RandomizeName = true;
    /// <summary>
    /// After randomizing, sets the hair style to this, if possible
    /// </summary>
    [DataField] public string? Hair = null;

    //Pirate Changes Start Here
    [DataField] public string? FacialHair = null;

    [DataField] public float? Height; // Висота

    [DataField] public float? Width; // Ширина

    [DataField] public Color? HairColor; // Колір волосся

    [DataField] public Color? SkinColor; // Колір шкіри

    [DataField] public Sex? Sex; // Стать
    //Pirate Changes End Here
}
