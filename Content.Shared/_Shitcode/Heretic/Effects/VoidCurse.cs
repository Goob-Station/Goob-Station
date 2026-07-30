// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitcode.Heretic.Systems;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Effects;

public sealed partial class VoidCurseSystemEffect : EntityEffectSystem<TransformComponent, VoidCurse>
{
    [Dependency] private readonly SharedVoidCurseSystem _voidCurse = default!;

    protected override void Effect(Entity<TransformComponent> entity, ref EntityEffectEvent<VoidCurse> args)
    {
        _voidCurse.DoCurse(entity.Owner, args.Effect.Stacks);
    }
}

public sealed partial class VoidCurse : EntityEffectBase<VoidCurse>
{
    [DataField]
    public int Stacks = 1;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => "Inflicts void curse.";
}
