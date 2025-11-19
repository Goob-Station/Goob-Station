// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Hagvan <22118902+Hagvan@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace  Content.Goobstation.Common.MartialArts;

public enum GrabStage
{
    No = 0,
    Soft = 1,
    Hard = 2,
    Suffocate = 3,
}

public enum GrabStageDirection
{
    Increase,
    Decrease,
}

public enum GrabResistResult
{
    TooSoon,
    Failed,
    Succeeded
}
