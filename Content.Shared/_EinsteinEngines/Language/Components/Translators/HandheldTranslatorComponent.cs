// SPDX-FileCopyrightText: 2025 CerberusWolfie <wb.johnb.willis@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 ImWeax <59857479+ImWeax@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 John Willis <143434770+CerberusWolfie@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._EinsteinEngines.Language.Components.Translators;

/// <summary>
///   A translator that must be held in a hand or a pocket of an entity in order ot have effect.
/// </summary>
[RegisterComponent]
public sealed partial class HandheldTranslatorComponent : BaseTranslatorComponent
{
    /// <summary>
    ///   Whether interacting with this translator toggles it on and off.
    /// </summary>
    [DataField]
    public bool ToggleOnInteract = true;

    /// <summary>
    ///     If true, when this translator is turned on, the entities' current spoken language will
    ///     be set to the first new language added by this translator.
    /// </summary>
    /// <remarks>
    ///      This should generally be used for translators that translate speech between two languages.
    /// </remarks>
    [DataField]
    public bool SetLanguageOnInteract = true;

    /// <summary>
    ///     Whether to display details about the translator when the object is examined.
    /// </summary>
    /// <remarks>
    ///     Added by Goob Station. This should be used for something like a magical object that grants a language to the user while they are holding/wearing it.
    /// </remarks>
    [DataField]
    public bool ShowInfoOnExamine = true;
}
