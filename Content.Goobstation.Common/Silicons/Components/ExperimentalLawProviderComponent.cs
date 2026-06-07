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

    // buffed point amounts 3x so people will actually use this; 30k in two minutes seems ok to me (triples points per second from ~80 to ~240)- strong but takes a lot of setup by RD (and stealing the AI upload console)
    [DataField] public float RewardTime = 120.0f;

    [DataField] public int RewardPoints = 30000;
}
