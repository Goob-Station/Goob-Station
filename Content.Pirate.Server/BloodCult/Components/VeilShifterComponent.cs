// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.BloodCult.Components;

[RegisterComponent]
public sealed partial class VeilShifterComponent : Component
{
    [DataField]
    public int Charges = 4;

    [DataField]
    public int TeleportDistanceMax = 10;

    [DataField]
    public int TeleportDistanceMin = 5;

    [DataField]
    public int Attempts = 10;

    [DataField]
    public SoundSpecifier TeleportInSound = new SoundPathSpecifier("/Audio/_Pirate/BloodCult/veilin.ogg");

    [DataField]
    public SoundSpecifier TeleportOutSound = new SoundPathSpecifier("/Audio/_Pirate/BloodCult/veilout.ogg");

    [DataField]
    public EntProtoId? TeleportInEffect = "BloodCultTeleportInEffect";

    [DataField]
    public EntProtoId? TeleportOutEffect = "BloodCultTeleportOutEffect";
}
