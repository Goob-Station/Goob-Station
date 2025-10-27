// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Effects;

/// <summary>
/// Stores the original sprite color for flashing entity to be able to restore it later.
/// </summary>
[RegisterComponent]
public sealed partial class ColorFlashEffectComponent : Component
{
    [ViewVariables]
    public Color Color = Color.White;
}