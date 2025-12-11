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

namespace Content.Goobstation.Server.EntityEffects;

[UsedImplicitly]
public sealed partial class SpeciesChange : EntityEffect
{
    [DataField(required: true)] public ProtoId<SpeciesPrototype> NewSpecies;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-change-species", ("species", NewSpecies));

    public override void Effect(EntityEffectBaseArgs args)
    {
        // this only changes the sprite of the urist.
        // a proper species change needs to be using polymorph system
        // but we can't afford that because polymorph is dogshit.

        if (!args.EntityManager.TryGetComponent<HumanoidAppearanceComponent>(args.TargetEntity, out var appearance))
            return;

        var humanoidAppearanceSystem = args.EntityManager.System<SharedHumanoidAppearanceSystem>();

        humanoidAppearanceSystem.SetSpecies(args.TargetEntity, NewSpecies);

        // TODO add slime species specific content here
    }
}
