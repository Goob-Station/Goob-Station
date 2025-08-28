using Content.Shared.Body.Part;
using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._White.Xenomorphs.FaceHugger;

[RegisterComponent]
public sealed partial class FaceHuggerComponent : Component
{
    [DataField]
    public (BodyPartType Type, BodyPartSymmetry Symmetry) InfectionBodyPart = (BodyPartType.Chest, BodyPartSymmetry.None);

    [DataField]
    public DamageSpecifier DamageOnImpact = new();

    [DataField]
    public DamageSpecifier DamageOnInfect = new();

    [DataField]
    public EntityWhitelist? Blacklist;

    [DataField]
    public EntProtoId? InfectionPrototype = "XenomorphInfection";

    [DataField]
    public string BlockingSlot = "head";

    [DataField]
    public string InfectionSlotId = "xenomorph_larva";

    [DataField]
    public string Slot = "mask";

    [DataField]
    public SoundSpecifier SoundOnImpact = new SoundCollectionSpecifier("MetalThud");

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan MaxInfectTime = TimeSpan.FromSeconds(20);

    [DataField]
    public TimeSpan MaxRestTime = TimeSpan.FromSeconds(20);

    [DataField]
    public TimeSpan MinInfectTime = TimeSpan.FromSeconds(10);

    [DataField]
    public TimeSpan MinRestTime = TimeSpan.FromSeconds(10);

    [ViewVariables]
    public bool Active = true;

    [ViewVariables]
    public TimeSpan InfectIn = TimeSpan.Zero;

    [ViewVariables]
    public TimeSpan RestIn = TimeSpan.Zero;
}
