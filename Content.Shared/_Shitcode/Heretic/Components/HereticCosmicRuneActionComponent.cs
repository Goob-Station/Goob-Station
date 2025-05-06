// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HereticCosmicRuneActionComponent : Component
{
    [DataField]
    public EntityUid? FirstRune;

    [DataField]
    public EntityUid? SecondRune;
}
