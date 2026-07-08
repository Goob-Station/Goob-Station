// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Access;
using Content.Shared.Access.Systems;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Heretic.Effects;

public sealed partial class RemoveAccess : EntityEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => "Removes all target access.";

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.System<SharedIdCardSystem>().TryFindIdCard(args.TargetEntity, out var id))
            return;

        args.EntityManager.System<SharedAccessSystem>().TrySetTags(id, new List<ProtoId<AccessLevelPrototype>>());
    }
}
