using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Antag.EldritchHorror;
using Content.Shared._Goobstation.EldritchHorror.EldritchHorrorEvents;

[RegisterComponent]
public sealed partial class EldritchHorrorComponent : Component
{

    [DataField]
    public EntityUid? SpawnProphetsActionEntity;

    [DataField]
    public EntProtoId SpawnProphetsAction = "ActionRiseProphets";

    [DataField]
    public EntProtoId ProphetProtoId = "MobGhoulProphet";

    [DataField]
    public int ProphetAmount = 2;
}
