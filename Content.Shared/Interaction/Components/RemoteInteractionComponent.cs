// SPDX-FileCopyrightText: 2025 GoobStation
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Interaction.Components;

/// <summary>
/// Allows entities to interact with objects remotely using vision-based checks for whitelisted entities.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class RemoteInteractionComponent : Component
{
}
