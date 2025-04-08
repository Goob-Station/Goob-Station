// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.GameTicking
{
    /// <summary>
    ///     Describes an entry in the crew manifest.
    /// </summary>
    public sealed class ManifestEntry
    {
        public ManifestEntry(string characterName, string jobId)
        {
            CharacterName = characterName;
            JobId = jobId;
        }

        /// <summary>
        ///     The name of the character on the manifest.
        /// </summary>
        [ViewVariables]
        public string CharacterName { get; }

        /// <summary>
        ///     The ID of the job they picked.
        /// </summary>
        [ViewVariables]
        public string JobId { get; }
    }
}