// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Server.Medical.CPR;

[RegisterComponent]
public sealed partial class CPRTrainingComponent : Component
{
    [DataField]
    public SoundSpecifier CPRSound = new SoundPathSpecifier("/Audio/_EinsteinEngines/Effects/CPR.ogg");

    [DataField]
    public TimeSpan DoAfterDuration = TimeSpan.FromSeconds(4);

    [DataField] public DamageSpecifier CPRHealing = new()
    {
        DamageDict =
        {
            ["Asphyxiation"] = -6
        }
    };

    [DataField] public float ResuscitationChance = 0.05f;

    [DataField] public float RotReductionMultiplier;

    public EntityUid? CPRPlayingStream;
}