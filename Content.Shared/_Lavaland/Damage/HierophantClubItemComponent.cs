using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Damage;

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
    public EntProtoId ToggleTileMovementActionId = "ActionHierophantTileMovement";
    [DataField]
    public EntityUid? CreateCrossActionEntity;

    [DataField]
    public EntityUid? PlaceMarkerActionEntity;

    [DataField]
    public EntityUid? TeleportToMarkerActionEntity;

    [DataField]
    public EntityUid? ToggleTileMovementActionEntity;

    [DataField]
    public EntityUid? TeleportMarker;

    [DataField]
    public EntProtoId TeleportMarkerPrototype = "LavalandHierophantClubTeleportMarker";

    [DataField]
    public float CrossRange = 5f;

    [DataField]
    public SoundSpecifier DamageSound = new SoundPathSpecifier("/Audio/_Lavaland/Mobs/Bosses/hiero_blast.ogg");

    [DataField]
    public SoundSpecifier TeleportSound = new SoundPathSpecifier("/Audio/Magic/blink.ogg");
}
