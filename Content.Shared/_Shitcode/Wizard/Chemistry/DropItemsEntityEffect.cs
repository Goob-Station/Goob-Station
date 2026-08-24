// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.EntityEffects;
using Content.Shared.Hands.Components;
using Content.Shared.Standing;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Wizard.Chemistry;

public sealed partial class DropItemsEntityEffectSystem : EntityEffectSystem<HandsComponent, DropItemsEntityEffect>
{
    protected override void Effect(Entity<HandsComponent> entity, ref EntityEffectEvent<DropItemsEntityEffect> args)
    {
        var ev = new DropHandItemsEvent();
        RaiseLocalEvent(entity.Owner, ref ev);
    }
}

[UsedImplicitly]
public sealed partial class DropItemsEntityEffect : EntityEffectBase<DropItemsEntityEffect>
{
    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("reagent-effect-guidebook-drop-items", ("chance", Probability));
    }
}
