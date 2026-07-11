// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Server.BloodCult.Components;

[RegisterComponent]
public sealed partial class VoidTorchComponent : Component
{
    [DataField]
    public int Charges = 5;

    [DataField]
    public SoundSpecifier TeleportSound = new SoundPathSpecifier("/Audio/_Pirate/BloodCult/veilin.ogg");
}
