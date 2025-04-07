// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Inventory.VirtualItem;

/// <inheritdoc cref="SharedVirtualItemSystem"/>
[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState(true)]
public sealed partial class VirtualItemComponent : Component
{
    /// <summary>
    /// The entity blocking this slot.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid BlockingEntity;
}