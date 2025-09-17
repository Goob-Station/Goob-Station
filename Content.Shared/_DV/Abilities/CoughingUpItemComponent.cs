// SPDX-FileCopyrightText: 2025 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Abilities;

/// <summary>
/// Spawns the item from <see cref="ItemCougherComponent"/> after the coughing sound is finished.
/// </summary>
/// <remarks>
/// Client doesn't care about spawning so the field isn't networked.
/// </remarks>
[RegisterComponent, NetworkedComponent, Access(typeof(ItemCougherSystem))]
[AutoGenerateComponentPause]
public sealed partial class CoughingUpItemComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextCough;
}