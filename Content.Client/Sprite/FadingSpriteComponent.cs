// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Sprite;

namespace Content.Client.Sprite;

/// <summary>
/// The non-networked client-only component to track active <see cref="SpriteFadeComponent"/>
/// </summary>
[RegisterComponent, Access(typeof(SpriteFadeSystem))]
public sealed partial class FadingSpriteComponent : Component
{
    [ViewVariables]
    public float OriginalAlpha;
}