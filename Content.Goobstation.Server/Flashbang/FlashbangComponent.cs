// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Flashbang;

[RegisterComponent]
public sealed partial class FlashbangComponent : Component
{
    [DataField]
    public float StunTime = 2f;

    [DataField]
    public float KnockdownTime = 10f;

    /// <summary>
    /// Minimum protection range on entity for stun and knocked down effects to be applied
    /// </summary>
    [DataField]
    public float MinProtectionRange;
}
