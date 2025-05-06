// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class StarTouchComponent : Component
{
    [DataField]
    public EntityUid? StarTouchAction;

    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(15);

    [DataField]
    public TimeSpan SleepTime = TimeSpan.FromSeconds(4);

    [DataField]
    public LocId Speech = "heretic-speech-star-touch";

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Items/welder.ogg");

    [DataField]
    public SpriteSpecifier BeamSprite = new SpriteSpecifier.Rsi(new("/Textures/_Goobstation/Heretic/Effects/effects.rsi"), "cosmic_beam");

    [DataField]
    public float BeamLifetime = 60f;

    [DataField]
    public float BeamTickInterval = 0.2f;

    [DataField]
    public float BeamMaxDistanceSquared = 100f;
}

[Serializable, NetSerializable]
public sealed partial class StarTouchBeamEvent : BaseContinuousBeamEvent
{
    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict =
        {
            { "Heat", 3f },
        },
    };
}
