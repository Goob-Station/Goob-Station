// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Thomas <87614336+Aeshus@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Radiation.Events;

/// <summary>
///     Raised on entity when it was irradiated
///     by some radiation source.
/// </summary>
public readonly record struct OnIrradiatedEvent(float FrameTime, float RadsPerSecond)
{
    public readonly float FrameTime = FrameTime;

    public readonly float RadsPerSecond = RadsPerSecond;

    public float TotalRads => RadsPerSecond * FrameTime;
}