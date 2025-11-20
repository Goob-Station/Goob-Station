// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
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
