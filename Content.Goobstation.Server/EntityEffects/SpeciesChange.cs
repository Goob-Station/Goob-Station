// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Humanoid;
using Content.Shared.Humanoid;
using Content.Shared.EntityEffects;
using Content.Shared.Humanoid.Prototypes;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityEffects;

[UsedImplicitly]
public sealed partial class SpeciesChange : EntityEffect
{
    /// <summary>
    ///     What sex is the consumer changed to? If not set then swap between male/female.
    /// </summary>
    // bro.
    [DataField("sex")]
    public ProtoId<SpeciesPrototype>? NewSpecies;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent<HumanoidAppearanceComponent>(args.TargetEntity, out var appearance))
            return;

        var uid = args.TargetEntity;
        var newSpecies = NewSpecies;
        var goobHumanoidAppearanceSystem = args.EntityManager.System<SharedGoobHumanoidAppearanceSystem>();
        var humanoidAppearanceSystem = args.EntityManager.System<SharedHumanoidAppearanceSystem>();


        // Eventually this should also add the slime sub-species.
        if (newSpecies.HasValue)
            humanoidAppearanceSystem.SetSpecies(uid, newSpecies.Value);
    }
}
