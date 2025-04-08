// SPDX-FileCopyrightText: 2022 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2023 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Client.PDA;

/// <summary>
/// Used for specifying the pda windows border colors
/// </summary>
[RegisterComponent]
public sealed partial class PdaBorderColorComponent : Component
{
    [DataField("borderColor", required: true)]
    public string? BorderColor;


    [DataField("accentHColor")]
    public string? AccentHColor;


    [DataField("accentVColor")]
    public string? AccentVColor;
}