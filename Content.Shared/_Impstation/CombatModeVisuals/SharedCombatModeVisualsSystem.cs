// SPDX-FileCopyrightText: 2026 Impstation contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._Impstation.CombatModeVisuals;

public abstract class SharedCombatModeVisualsSystem : EntitySystem
{
    [Serializable, NetSerializable]
    public enum CombatModeVisualsVisuals : byte
    {
        Combat
    }
}
