// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Goobstation.Common.Silicons.Components;

/// <summary>
/// Actively provides a random lawset to some entities
/// If the timer ticks down gives it's reward to a research server
/// </summary>
[RegisterComponent]
public sealed partial class ActiveExperimentalLawProviderComponent : Component
{
    [DataField]
    public string OldSiliconLawsetId = string.Empty;

    [DataField]
    public float Timer = 120.0f;

    [DataField]
    public int RewardPoints = 5000;

    [DataField]
    public SoundSpecifier? LawRewardSound = new SoundPathSpecifier("/Audio/Misc/cryo_warning.ogg");
}
