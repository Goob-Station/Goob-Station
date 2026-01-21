using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.EntityEffects;
using Content.Shared.Heretic;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Heretic.Effects;

public sealed partial class DeconvertGhoul : EntityEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("reagent-effect-guidebook-deconvert-ghoul");
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent(args.TargetEntity, out GhoulComponent? ghoul) || !ghoul.CanDeconvert ||
            args.EntityManager.HasComponent<GhoulDeconvertComponent>(args.TargetEntity))
            return;

        args.EntityManager.AddComponent<GhoulDeconvertComponent>(args.TargetEntity);
    }
}
