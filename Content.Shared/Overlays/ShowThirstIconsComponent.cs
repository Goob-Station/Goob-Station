// SPDX-FileCopyrightText: 2024 Krunklehorn <42424291+Krunklehorn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 PrPleGoo <PrPleGoo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT
using Robust.Shared.GameStates;

namespace Content.Shared.Overlays;

/// <summary>
///     This component allows you to see the thirstiness of mobs.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShowThirstIconsComponent : Component { }