// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Lavaland.Hierophant.Components;

/// <summary>
/// Signifies that this entity is being blink-teleported to some spot.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class HierophantActiveBlinkComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan BlinkDelay = TimeSpan.FromSeconds(0.9f);

    [ViewVariables, AutoPausedField]
    public TimeSpan? DefaultBlinkTime;

    [ViewVariables]
    public EntityUid? BlinkDummy;
}
