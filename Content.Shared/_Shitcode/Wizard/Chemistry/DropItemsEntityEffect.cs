// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
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
