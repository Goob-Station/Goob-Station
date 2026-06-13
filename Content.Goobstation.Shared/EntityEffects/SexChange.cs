// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Humanoid;
using Content.Shared.Humanoid;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class SexChangeSystem : EntityEffectSystem<HumanoidAppearanceComponent, SexChange>
{
    [Dependency] private readonly SharedGoobHumanoidAppearanceSystem _goobHumanoid = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoid = default!;

    protected override void Effect(Entity<HumanoidAppearanceComponent> entity, ref EntityEffectEvent<SexChange> args)
    {
        var uid = entity.Owner;
        var newSex = args.Effect.NewSex;

        if (newSex.HasValue)
        {
            _humanoid.SetSex(uid, newSex.Value);
            return;
        }

        if (entity.Comp.Sex != Sex.Unsexed)
            _goobHumanoid.SwapSex(uid);
    }
}

[UsedImplicitly]
public sealed partial class SexChange : EntityEffectBase<SexChange>
{
    /// <summary>
    ///     What sex is the consumer changed to? If not set then swap between male/female.
    /// </summary>
    [DataField] public Sex? NewSex;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-sex-change", ("chance", Probability));
}
