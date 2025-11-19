// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Content.Shared.Storage;
using Robust.Shared.GameStates;

namespace Content.Shared.Implants.Components;

/// <summary>
/// Handles emptying the implant's <see cref="StorageComponent"/> when the implant is removed.
/// Without this the contents would be deleted.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StorageImplantComponent : Component;
