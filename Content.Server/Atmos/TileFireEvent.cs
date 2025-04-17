// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Atmos
{
    /// <summary>
    ///     Event raised directed to an entity when it is standing on a tile that's on fire.
    /// </summary>
    [ByRefEvent]
    public readonly struct TileFireEvent
    {
        public readonly float Temperature;
        public readonly float Volume;

        public TileFireEvent(float temperature, float volume)
        {
            Temperature = temperature;
            Volume = volume;
        }
    }
}