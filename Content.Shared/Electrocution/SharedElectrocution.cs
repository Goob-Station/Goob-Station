// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.Electrocution;

[Serializable, NetSerializable]
public enum ElectrifiedLayers : byte
{
    Sparks,
    HUD,
}

[Serializable, NetSerializable]
public enum ElectrifiedVisuals : byte
{
    ShowSparks, // only shown when zapping someone, deactivated after a short time
    IsElectrified, // if the entity is electrified or not, used for the AI HUD
}