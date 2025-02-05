using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Mobs.Hierophant.Components;

[RegisterComponent]
public sealed partial class HierophantClubItemComponent : Component
{
    [DataField]
    public EntProtoId CreateCrossActionId = "ActionHierophantSpawnCross";

    [DataField]
    public EntProtoId PlaceMarkerActionId = "ActionHierophantPlaceMarker";

    [DataField]
    public EntProtoId TeleportToMarkerActionId = "ActionHierophantTeleport";

    [DataField]
    public EntityUid? CreateCrossActionEntity;

    [DataField]
    public EntityUid? PlaceMarkerActionEntity;

    [DataField]
    public EntityUid? TeleportToMarkerActionEntity;

    [DataField]
    public EntityUid? TeleportMarker;
}
