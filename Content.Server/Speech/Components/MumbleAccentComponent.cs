// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Pancake <Pangogie@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 dahnte <70238020+dahnte@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 themias <89101928+themias@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Server.Speech.Components;

[RegisterComponent]
public sealed partial class MumbleAccentComponent : Component
{
    /// <summary>
    /// This modifies the audio parameters of emote sounds, screaming, laughing, etc.
    /// By default, it reduces the volume and distance of emote sounds.
    /// </summary>
    [DataField]
    public AudioParams EmoteAudioParams = AudioParams.Default.WithVolume(-8f).WithMaxDistance(5);
}
