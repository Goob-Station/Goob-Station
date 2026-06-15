using Content.Goobstation.Shared.Disease;
using Content.Goobstation.Shared.Disease.Components;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects.Disease;

/// <summary>
/// Reduces the progress of diseases of chosen type on the entity.
/// </summary>
public sealed partial class DiseaseProgressChange : EventEntityEffect<DiseaseProgressChange>
{
    /// <summary>
    /// Diseases of which type to affect.
    /// </summary>
    [DataField]
    public ProtoId<DiseaseTypePrototype> AffectedType;

    /// <summary>
    /// How much to add to the disease progress.
    /// </summary>
    [DataField]
    public float ProgressModifier = -0.02f;

    [DataField]
    public bool Scaled = true;

    [DataField]
    public float Scale = 1f;
    [DataField]
    public float Quantity = 1f;

    public DiseaseProgressChange(ProtoId<DiseaseTypePrototype> affectedType, float progressModifier, bool scaled, float scale, float quantity)
    {
        AffectedType = affectedType;
        ProgressModifier = progressModifier;
        Scaled = scaled;
        Scale = scale;
        Quantity = quantity;
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("reagent-effect-guidebook-disease-progress-change",
            ("chance", Probability),
            ("type", prototype.Index<DiseaseTypePrototype>(AffectedType).LocalizedName),
            ("amount", ProgressModifier));
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent<DiseaseCarrierComponent>(args.TargetEntity, out _))
            return;

        var ev = new DiseaseProgressChange(AffectedType, ProgressModifier, Scaled, Scale, Quantity);

        if (args is EntityEffectReagentArgs reagentArgs)
        {
            ev.Scale = reagentArgs.Scale.Float();
            ev.Quantity = reagentArgs.Quantity.Float();
        }

        args.EntityManager.EventBus.RaiseLocalEvent(args.TargetEntity, ev);
    }
}
