// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Server._EinsteinEngines.Medical.CPR;

[RegisterComponent]
public sealed partial class CanDoCprComponent : Component
{
    /// <summary>
    /// Sound played on CPR.
    /// </summary>
    [DataField]
    public SoundSpecifier CPRSound = new SoundPathSpecifier("/Audio/_EinsteinEngines/Effects/CPR.ogg");

    /// <summary>
    /// How long each compression takes to finish.
    /// </summary>
    [DataField]
    public TimeSpan DoAfterDuration = TimeSpan.FromSeconds(4);

    /// <summary>
    /// The damages healed per compression.
    /// </summary>
    [DataField]
    public DamageSpecifier CPRHealing = new()
    {
        DamageDict =
        {
            ["Asphyxiation"] = -6,
        },
    };

    /// <summary>
    /// Chance per compression to revive the target.
    /// </summary>
    [DataField]
    public float ResuscitationChance = 0.05f;

    /// <summary>
    /// The multiplier for reducing rot per compression.
    /// </summary>
    [DataField]
    public float RotReductionMultiplier;

    public EntityUid? CPRPlayingStream;
}
