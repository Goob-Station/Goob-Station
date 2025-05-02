// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Dash;

[RegisterComponent]
public sealed partial class DashActionComponent : Component
{
    [DataField]
    public string? ActionProto;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ActionUid;
}

public sealed partial class DashActionEvent : WorldTargetActionEvent
{
    [DataField]
    public float Distance = 4.65f;

    [DataField]
    public float Speed = 9.65f;

    /// <summary>
    /// Whether you need gravity to perform the dash. Keep in mind there's no friction without gravity so if this
    /// is false, the performer gets every chance to be launched straight to Ohio on dashing without gravity.
    /// </summary>
    [DataField]
    public bool NeedsGravity = true;

    /// <summary>
    /// Whether dash distance and speed are affected by performer's speed modifiers. Should be true most of the time.
    /// </summary>
    [DataField]
    public bool AffectedBySpeed = true;

    /// <summary>
    /// Animated emote to play on successful dash.
    /// </summary>
    [DataField]
    public ProtoId<EmotePrototype>? Emote = "Flip";
}
