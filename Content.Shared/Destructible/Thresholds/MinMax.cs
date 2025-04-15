// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Random;

namespace Content.Shared.Destructible.Thresholds;

[DataDefinition, Serializable]
public partial struct MinMax
{
    [DataField]
    public int Min;

    [DataField]
    public int Max;

    public MinMax(int min, int max)
    {
        Min = min;
        Max = max;
    }

    public readonly int Next(IRobustRandom random)
    {
        return random.Next(Min, Max + 1);
    }

    public readonly int Next(System.Random random)
    {
        return random.Next(Min, Max + 1);
    }
}