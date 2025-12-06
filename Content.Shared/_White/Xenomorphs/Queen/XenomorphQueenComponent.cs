using Content.Shared._White.Xenomorphs.Caste;
using Robust.Shared.Prototypes;
using Content.Shared.NPC.Prototypes;
using Robust.Shared.Audio; //goob

namespace Content.Shared._White.Xenomorphs.Queen;

[RegisterComponent]
public sealed partial class XenomorphQueenComponent : Component
{
    [DataField]
    public EntProtoId PromotionActionId = "ActionXenomorphPromotion";

    [DataField]
    public EntProtoId PromoteTo = "MobXenomorphPraetorian";

    [DataField]
    public List<ProtoId<XenomorphCastePrototype>> CasteWhitelist = new() { "Drone", "Hunter", "Sentinel" };

    [DataField]
    public TimeSpan EvolutionDelay = TimeSpan.FromSeconds(3);

    [ViewVariables]
    public EntityUid? PromotionAction;

    //goobstart
    [ViewVariables(VVAccess.ReadWrite), DataField("soundRoar")]
    public SoundSpecifier? SoundRoar =
        new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_echoroar_1.ogg") //FIX
        {
            Params = AudioParams.Default.WithVolume(3f),
        };

    [DataField]
    public EntityUid? RoarActionEntity;

    [DataField]
    public EntProtoId RoarAction = "ActionQueenRoar";

    [DataField]
    public float RoarRange = 4f; //goob, 2 larger than dragon

    [DataField]
    public float RoarStunTime = 2f;
    //goobend
}
