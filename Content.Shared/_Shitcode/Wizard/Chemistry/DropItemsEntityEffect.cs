// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.Standing;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard.Chemistry;

[UsedImplicitly]
public sealed partial class DropItemsEntityEffect : EntityEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("reagent-effect-guidebook-drop-items", ("chance", Probability));
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        var ev = new DropHandItemsEvent();
        args.EntityManager.EventBus.RaiseLocalEvent(args.TargetEntity, ref ev);
    }
}
