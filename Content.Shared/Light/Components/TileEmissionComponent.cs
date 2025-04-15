// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 DoutorWhite <thedoctorwhite@gmail.com>
// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Light.Components;

/// <summary>
/// Will draw lighting in a range around the tile.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TileEmissionComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Range = 0.25f;

    [DataField(required: true), AutoNetworkedField]
    public Color Color = Color.Transparent;
}