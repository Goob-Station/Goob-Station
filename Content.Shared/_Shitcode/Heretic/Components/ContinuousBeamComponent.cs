// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ContinuousBeamComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public Dictionary<NetEntity, ContinuousBeamData> Data = new(); // Target -> Data (no more than 1 beam per target)
}

[Serializable, NetSerializable, DataDefinition]
public sealed partial class ContinuousBeamData(
    SpriteSpecifier sprite,
    float lifetime,
    float tickInterval,
    float maxDistanceSquared,
    Color color,
    BaseContinuousBeamEvent? ev)
{
    public ContinuousBeamData() : this(SpriteSpecifier.Invalid, 60f, 0.2f, 100f, Color.White, null) { }

    [DataField]
    public SpriteSpecifier Sprite = sprite;

    [DataField]
    public float MaxDistanceSquared = maxDistanceSquared;

    [DataField]
    public BaseContinuousBeamEvent? Event = ev;

    [DataField]
    public float Lifetime = lifetime;

    [DataField]
    public float TickInterval = tickInterval;

    [DataField]
    public float Timer = tickInterval;

    [DataField]
    public Color Color = color;
}

[Serializable, NetSerializable, ImplicitDataDefinitionForInheritors]
public abstract partial class BaseContinuousBeamEvent : HandledEntityEventArgs
{
    public NetEntity User;

    public NetEntity Target;
}
