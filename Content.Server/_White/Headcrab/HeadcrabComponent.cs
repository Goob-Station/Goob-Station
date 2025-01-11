using Content.Server.NPC.HTN;
using Content.Shared.Damage;
using Content.Shared.NPC.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._White.Headcrab;

[Access(typeof(HeadcrabSystem))]
[RegisterComponent]
public sealed partial class HeadcrabComponent : Component
{
    /// <summary>
    /// WorldTargetAction
    /// </summary>
    [DataField]
    public EntProtoId JumpAction = "ActionHeadcrabJump";

    public EntityUid? JumpActionEntity;

    [DataField]
    public TimeSpan ParalyzeTime = TimeSpan.FromSeconds(3);

    [DataField]
    public float ChancePounce = 0.33f;

    [DataField(required: true)]
    public DamageSpecifier Damage = default!;

    [DataField]
    public DamageSpecifier HealOnEqupped = default!;

    public EntityUid EquippedOn;

    [ViewVariables]
    public float Accumulator = 0;

    [DataField]
    public float DamageFrequency = 5;

    [DataField]
    public SoundSpecifier? JumpSound = new SoundPathSpecifier("/Audio/_White/Misc/Headcrab/headcrab_jump.ogg");

    /// <summary>
    /// Whether or not is currently attached to an NPC.
    /// </summary>
    [DataField]
    public bool HasNpc;

    /// <summary>
    /// The mind that was booted from the wearer when the headcrab took over.
    /// </summary>
    [DataField]
    public EntityUid? StolenMind;

    public ProtoId<HTNCompoundPrototype> TakeoverTask = "SimpleHostileCompound";

    [DataField]
    public ProtoId<NpcFactionPrototype> HeadcrabFaction = "Zombie";

    [DataField]
    public HashSet<ProtoId<NpcFactionPrototype>> OldFactions = new();

    [DataField]
    public LocId MindLostMessageSelf = "headcrab-mind";
}
