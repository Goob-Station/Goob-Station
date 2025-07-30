using Content.Shared.NPC.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;


namespace Content.Shared.Shadowkin;

[RegisterComponent, NetworkedComponent]
public sealed partial class EtherealComponent : Component
{
    /// <summary>
    ///     Does the Ent, Dark lights around it?
    /// </summary>
    [DataField]
    public bool Darken = false;

    /// <summary>
    ///     Range of the Darken Effect.
    /// </summary>
    [DataField]
    public float DarkenRange = 5;

    /// <summary>
    ///     Darken Effect Rate.
    /// </summary>
    [DataField]
    public float DarkenRate = 0.084f;

    /// Can this be stunned by ethereal stun objects?
    [DataField]
    public bool CanBeStunned = true;

    /// <summary>
    ///     How much stamina damage does the user take each second they are in the dark realm?
    /// </summary>
    [DataField]
    public float StaminaPerSecond = 1;

    [DataField]
    public float StaminaDamageOnFlash = 200f;

    public List<EntityUid> DarkenedLights = new();

    public float DarkenAccumulator;

    public int OldMobMask;

    public int OldMobLayer;

    public List<ProtoId<NpcFactionPrototype>> SuppressedFactions = new();
    public bool HasDoorBumpTag;
}
