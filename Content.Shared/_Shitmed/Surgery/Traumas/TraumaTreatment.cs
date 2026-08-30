using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery.Traumas;

/// <summary>
/// Declares how a trauma can be treated / removed.
/// </summary>
[ImplicitDataDefinitionForInheritors]
public abstract partial class TraumaTreatment;

/// <summary>
/// The trauma is removed by surgery. Presence of this treatment makes the trauma eligible for the generic "extract foreign body" surgery.
/// Define a new surgery instead when you want something else.
/// </summary>
public sealed partial class SurgicalTreatment : TraumaTreatment;
