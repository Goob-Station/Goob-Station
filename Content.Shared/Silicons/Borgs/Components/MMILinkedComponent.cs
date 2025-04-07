// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Silicons.Borgs.Components;

/// <summary>
/// This is used for an entity that is linked to an MMI.
/// Mostly for receiving events.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedBorgSystem))]
[AutoGenerateComponentState]
public sealed partial class MMILinkedComponent : Component
{
    /// <summary>
    /// The MMI this entity is linked to.
    /// </summary>
    [DataField("linkedMMI"), AutoNetworkedField]
    public EntityUid? LinkedMMI;
}