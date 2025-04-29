// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpriteRandomOffsetComponent : Component
{
    [DataField]
    public float MinX = -0.25f;

    [DataField]
    public float MaxX = 0.25f;

    [DataField]
    public float MinY = -0.25f;

    [DataField]
    public float MaxY = 0.25f;
}

[Serializable, NetSerializable]
public enum OffsetVisuals : byte
{
    Offset
}