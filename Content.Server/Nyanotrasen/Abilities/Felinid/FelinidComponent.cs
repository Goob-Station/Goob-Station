// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Prototypes;

namespace Content.Server.Abilities.Felinid;

[RegisterComponent]
public sealed partial class FelinidComponent : Component
{
    /// <summary>
    /// The hairball prototype to use.
    /// </summary>
    [DataField("hairballPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string HairballPrototype = "Hairball";

    //[DataField("hairballAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    //public string HairballAction = "ActionHairball";

    [DataField("hairballActionId",
        customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? HairballActionId = "ActionHairball";

    [DataField("hairballAction")]
    public EntityUid? HairballAction;

    [DataField("eatActionId",
        customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? EatActionId = "ActionEatMouse";

    [DataField("eatAction")]
    public EntityUid? EatAction;

    [DataField("eatActionTarget")]
    public EntityUid? EatActionTarget = null;
}