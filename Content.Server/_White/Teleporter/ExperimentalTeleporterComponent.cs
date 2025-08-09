// SPDX-FileCopyrightText: 2024 Spatison <137375981+spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Server._White.Teleporter;

[RegisterComponent]
public sealed partial class ExperimentalTeleporterComponent : Component
{
    [DataField]
    public int MinTeleportRange = 3;

    [DataField]
    public int MaxTeleportRange = 8;

    [DataField]
    public int EmergencyLength = 4;

    [DataField]
    public List<int> RandomRotations = new() {90, -90};

    [DataField]
    public string? TeleportInEffect = "ExperimentalTeleporterInEffect";

    [DataField]
    public string? TeleportOutEffect = "ExperimentalTeleporterOutEffect";

    [DataField]
    public SoundSpecifier TeleportSound = new SoundPathSpecifier("/Audio/_White/Object/Devices/experimentalsyndicateteleport.ogg");
}
