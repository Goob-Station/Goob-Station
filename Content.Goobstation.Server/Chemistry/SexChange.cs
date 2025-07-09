// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Humanoid;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Chemistry;

[UsedImplicitly]
public sealed partial class SexChange : EntityEffect
{
    /// <summary>
    ///     What sex is the consumer changed to? If not set then swap between male/female.
    /// </summary>
    [DataField("sex")]
    public Sex? NewSex;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-sex-change", ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent<HumanoidAppearanceComponent>(args.TargetEntity, out var appearance))
            return;

        var uid = args.TargetEntity;
        var newSex = NewSex;
        var humanoidAppearanceSystem = args.EntityManager.System<SharedHumanoidAppearanceSystem>();

        if (newSex.HasValue)
        {
            humanoidAppearanceSystem.SetSex(uid, newSex.Value);
            return;
        }

        if (appearance.Sex != Sex.Unsexed)
            humanoidAppearanceSystem.SwapSex(uid);
    }
}
