// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Veritius <veritiusgaming@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Administration.Systems;

namespace Content.Shared.Administration.Components;

[RegisterComponent]
[Access(typeof(SharedBufferingSystem))]
public sealed partial class BufferingComponent : Component
{
    [DataField("minBufferTime")]
    public float MinimumBufferTime = 0.5f;

    [DataField("maxBufferTime")]
    public float MaximumBufferTime = 1.5f;

    [DataField("minTimeTilNextBuffer")]
    public float MinimumTimeTilNextBuffer = 10.0f;

    [DataField("maxTimeTilNextBuffer")]
    public float MaximumTimeTilNextBuffer = 120.0f;

    [DataField]
    public float TimeTilNextBuffer = 15.0f;

    [DataField]
    public EntityUid? BufferingIcon = null;

    [DataField]
    public float BufferingTimer = 0.0f;
}
