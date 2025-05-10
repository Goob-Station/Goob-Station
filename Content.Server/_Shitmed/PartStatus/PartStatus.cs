using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;

namespace Content.Server._Shitmed.PartStatus;

// collecting a body parts information together
// ik its another bs level of abstraction but i think it helps for now..
public sealed class PartStatus(
    BodyPartType partType,
    BodyPartSymmetry partSymmetry,
    string partName,
    WoundableSeverity partSverity,
    Dictionary<string, WoundSeverity> damageSeverities,
    BoneSeverity boneSeverity,
    bool bleeding)
{
    public BodyPartType PartType = partType;

    public BodyPartSymmetry PartSymmetry = partSymmetry;

    public string PartName = partName;

    public WoundableSeverity PartSeverity = partSverity;

    public Dictionary<string, WoundSeverity> DamageSeverities = damageSeverities;

    public BoneSeverity BoneSeverity = boneSeverity;

    public bool Bleeding = bleeding;
}
