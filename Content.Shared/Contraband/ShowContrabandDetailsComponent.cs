// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.GameStates;

namespace Content.Shared.Contraband;

/// <summary>
/// This component allows you to see Contraband details on examine items
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShowContrabandDetailsComponent : Component;
