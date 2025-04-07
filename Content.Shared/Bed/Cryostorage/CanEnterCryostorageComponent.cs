// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.GameStates;

namespace Content.Shared.Bed.Cryostorage;

/// <summary>
/// Serves as a whitelist that allows an entity with this component to enter cryostorage.
/// It will also require MindContainerComponent.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CanEnterCryostorageComponent : Component { }