// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Humanoid;
using Content.Shared.EntityEffects;
using Content.Shared.Humanoid.Prototypes;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Content.Server.Polymorph.Systems;
using Content.Shared.Polymorph;
using Content.Server.Polymorph.Components;

namespace Content.Goobstation.Server.EntityEffects;

[UsedImplicitly]
public sealed partial class SpeciesChange : EntityEffect
{
    [DataField(required: true)] public ProtoId<SpeciesPrototype> NewSpecies;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-change-species", ("species", NewSpecies));

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent<HumanoidAppearanceComponent>(args.TargetEntity, out var appearance))
            return;

        var polymorphSystem = args.EntityManager.System<PolymorphSystem>();
        var protMan = IoCManager.Resolve<IPrototypeManager>();

        if (protMan.TryIndex(NewSpecies, out var species))
        {
            var config = new PolymorphConfiguration
            {
                Entity = species.Prototype,
                TransferDamage = true,
                Forced = true,
                Inventory = PolymorphInventoryChange.Transfer,
                RevertOnCrit = false,
                RevertOnDeath = false,
                TransferName = true,
            };

            var @new = polymorphSystem.PolymorphEntity(args.TargetEntity, config);
            if (@new.HasValue)
            {
                args.EntityManager.RemoveComponentDeferred<PolymorphedEntityComponent>(@new.Value);
            }
        }

        // TODO add slime subspecies specific content here
    }
}
