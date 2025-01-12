using Content.Server.Body.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Server.Body.Systems;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;

namespace Content.Server.EntityEffects.Effects;

public sealed partial class AddReagentToBlood : EntityEffect
{
    private readonly SharedSolutionContainerSystem _solutionContainers;

    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<ReagentPrototype>))]
    public string? Reagent = null;

    [DataField]
    public FixedPoint2 Amount = default!;

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args.EntityManager.TryGetComponent<BloodstreamComponent>(args.TargetEntity, out var blood))
        {
            var sys = args.EntityManager.System<BloodstreamSystem>();
            if (args is EntityEffectReagentArgs reagentArgs)
            {
                if (Reagent is null) return;
                var amt = Amount;
                var solution = new Solution();
                solution.AddReagent(Reagent, amt);
                sys.TryAddToChemicals(args.TargetEntity, solution, blood);
            }
            return;
        }
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        if (Reagent is not null && prototype.TryIndex(Reagent, out ReagentPrototype? reagentProto))
        {
            return Loc.GetString("reagent-effect-guidebook-add-to-chemicals",
                ("chance", Probability),
                ("deltasign", MathF.Sign(Amount.Float())),
                ("reagent", reagentProto.LocalizedName),
                ("amount", MathF.Abs(Amount.Float())));
        }

        throw new NotImplementedException();
    }
}
