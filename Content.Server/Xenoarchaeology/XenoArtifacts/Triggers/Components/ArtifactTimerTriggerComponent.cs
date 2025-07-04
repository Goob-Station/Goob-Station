// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Components;

/// <summary>
///     Will try to activate artifact periodically.
///     Doesn't used for random artifacts, can be spawned by admins.
/// </summary>
[RegisterComponent]
public sealed partial class ArtifactTimerTriggerComponent : Component
{
    /// <summary>
    ///     Time between artifact activation attempts.
    /// </summary>
    [DataField("rate")]
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan ActivationRate = TimeSpan.FromSeconds(5.0f);

    /// <summary>
    ///     Last time when artifact was activated.
    /// </summary>
    public TimeSpan LastActivation;
}