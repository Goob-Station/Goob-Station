// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Server.Nyanotrasen.Abilities.Felinid;

[RegisterComponent]
public sealed partial class FelinidComponent : Component
{
    /// <summary>
    /// The hairball prototype to use.
    /// </summary>
    [DataField("hairballPrototype")]
    public EntProtoId HairballPrototype = "Hairball";

    //[DataField("hairballAction")]
    //public string HairballAction = "ActionHairball";

    [DataField("hairballActionId")]
    public EntProtoId? HairballActionId = "ActionHairball";

    [DataField("hairballAction")]
    public EntityUid? HairballAction;

    [DataField("eatActionId")]
    public EntProtoId? EatActionId = "ActionEatMouse";

    [DataField("eatAction")]
    public EntityUid? EatAction;

    [DataField("eatActionTarget")]
    public EntityUid? EatActionTarget = null;
}
