// SPDX-FileCopyrightText: 2025 Ark <189933909+ark1368@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Abilities.Chitinid;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class CoughingUpChitziteComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextCough;

    [DataField]
    public TimeSpan CoughUpTime = TimeSpan.FromSeconds(2.15);
}
