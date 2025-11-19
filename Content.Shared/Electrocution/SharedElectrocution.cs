// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

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
