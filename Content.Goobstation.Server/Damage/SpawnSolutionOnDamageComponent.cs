// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Fishbait <Fishbait@git.ml>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Damage;

[RegisterComponent]

public sealed partial class SpawnSolutionOnDamageComponent : Component
{
    [DataField]
    public EntProtoId Solution = "unknown";
    [DataField]
    public float MinimumAmount = 0;
    [DataField]
    public float MaximumAmount = 30;
    [DataField]
    public float Threshold = 5;
    [DataField]
    public float Probability = 0.5f;
}