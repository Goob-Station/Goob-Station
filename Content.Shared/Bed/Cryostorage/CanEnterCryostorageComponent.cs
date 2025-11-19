// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.GameStates;

namespace Content.Shared.Bed.Cryostorage;

/// <summary>
/// Serves as a whitelist that allows an entity with this component to enter cryostorage.
/// It will also require MindContainerComponent.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CanEnterCryostorageComponent : Component { }
