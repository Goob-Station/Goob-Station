// SPDX-FileCopyrightText: 2025 CerberusWolfie <wb.johnb.willis@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._EinsteinEngines.Language.Components;

// <summary>
//     Signifies that this entity can speak and understand any language.
//     Applies to such entities as ghosts.
// </summary>
[RegisterComponent]
public sealed partial class UniversalLanguageSpeakerComponent : Component
{
    [DataField]
    public bool Enabled = true;
}
