// SPDX-FileCopyrightText: 2022 Francesco <frafonia@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT
using Robust.Shared.GameStates;

namespace Content.Shared.Medical.Cryogenics;

/// <summary>
/// Tracking component for an enabled cryo pod (which periodically tries to inject chemicals in the occupant, if one exists)
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ActiveCryoPodComponent : Component;
