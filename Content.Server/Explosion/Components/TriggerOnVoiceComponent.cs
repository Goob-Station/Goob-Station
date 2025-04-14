// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Explosion.Components
{
    /// <summary>
    /// Sends a trigger when the keyphrase is heard
    /// </summary>
    [RegisterComponent]
    public sealed partial class TriggerOnVoiceComponent : Component
    {
        public bool IsListening => IsRecording || !string.IsNullOrWhiteSpace(KeyPhrase);

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("keyPhrase")]
        public string? KeyPhrase;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("listenRange")]
        public int ListenRange { get; private set; } = 4;

        [DataField("isRecording")]
        public bool IsRecording = false;

        [DataField("minLength")]
        public int MinLength = 3;

        [DataField("maxLength")]
        public int MaxLength = 50;
    }
}