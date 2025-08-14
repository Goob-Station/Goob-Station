// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Lavaland.Megafauna.Components;

/// <summary>
/// Signifies that this entity is being blink-teleported to some spot.
/// TODO: cool shader for this fella
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class MegafaunaBlinkComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan DefaultDelay = TimeSpan.FromSeconds(0.9f);

    [DataField, AutoNetworkedField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/Magic/blink.ogg");

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan? BlinkTime;

    [ViewVariables, AutoNetworkedField]
    public EntityCoordinates Coordinates;
}
