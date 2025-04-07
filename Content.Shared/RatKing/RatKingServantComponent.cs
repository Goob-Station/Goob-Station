// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.RatKing;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedRatKingSystem))]
[AutoGenerateComponentState]
public sealed partial class RatKingServantComponent : Component
{
    /// <summary>
    /// The king this rat belongs to.
    /// </summary>
    [DataField("king")]
    [AutoNetworkedField]
    public EntityUid? King;
}