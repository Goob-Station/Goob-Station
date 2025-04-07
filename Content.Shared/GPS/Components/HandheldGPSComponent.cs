// SPDX-FileCopyrightText: 2024 ArchRBX <5040911+ArchRBX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 archrbx <punk.gear5260@fastmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.GPS.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HandheldGPSComponent : Component
{
    [DataField]
    public float UpdateRate = 1.5f;
}