// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._RMC14.Weapons.Ranged.Prediction;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedGunPredictionSystem))]
public sealed partial class IgnorePredictionHideComponent : Component;
