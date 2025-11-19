// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Silicons.Components;

/// <summary>
/// Used for law uploading console, when inserted it will update laws randomly,
/// then after some time when this set of laws wasn't changed it gives some research points to an RnD server.
/// </summary>
[RegisterComponent]
public sealed partial class ExperimentalLawProviderComponent : Component
{
    [DataField] public string RandomLawsets = "IonStormLawsets";

    // Numbers are equivalent to 83 points per second, so it's like running a dangerous anomaly for 2 minutes.
    [DataField] public float RewardTime = 120.0f;

    [DataField] public int RewardPoints = 10000;
}
