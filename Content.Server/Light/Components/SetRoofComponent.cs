// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 DoutorWhite <thedoctorwhite@gmail.com>
// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Light.Components;

/// <summary>
/// Applies the roof flag to this tile and deletes the entity.
/// </summary>
[RegisterComponent]
public sealed partial class SetRoofComponent : Component
{
    [DataField(required: true)]
    public bool Value;
}