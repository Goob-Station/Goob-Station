// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.SurveillanceCamera;

/// <summary>
///     This allows surveillance cameras to speak, if the camera in question
///     has a microphone that listens to speech.
/// </summary>
[RegisterComponent]
public sealed partial class SurveillanceCameraSpeakerComponent : Component
{
    // mostly copied from Speech
    [DataField("speechEnabled")] public bool SpeechEnabled = true;

    [ViewVariables] public float SpeechSoundCooldown = 0.5f;

    public TimeSpan LastSoundPlayed = TimeSpan.Zero;
}