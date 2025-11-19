// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.GameStates;

namespace Content.Shared.Forensics.Components;

/// <summary>
/// This component stops the entity from leaving fingerprints,
/// usually so fibres can be left instead.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FingerprintMaskComponent : Component;

