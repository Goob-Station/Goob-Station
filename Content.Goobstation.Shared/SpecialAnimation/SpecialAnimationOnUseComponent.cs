// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.SpecialAnimation;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SpecialAnimationOnUseComponent : Component
{
    /// <summary>
    /// Animation to play when this entity is triggered.
    /// If not specified, will use default variation.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<SpecialAnimationPrototype>? AnimationDataId;

    /// <summary>
    /// If specified, will override existing text inside SpecialAnimationPrototype.
    /// Use this to not shitspam with prototypes on each name.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? OverrideText;

    [DataField, AutoNetworkedField]
    public SpecialAnimationBroadcastType BroadcastType = SpecialAnimationBroadcastType.Pvs;
}

public enum SpecialAnimationBroadcastType
{
    Local,
    Pvs,
    Grid,
    Map,
    Global,
}
