// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Heretic.Effects;

public sealed partial class RemoveAccessSystem : EntityEffectSystem<AccessReaderComponent, RemoveAccess>
{
    [Dependency] private readonly SharedIdCardSystem _id = default!;
    [Dependency] private readonly SharedAccessSystem _access = default!;

    protected override void Effect(Entity<AccessReaderComponent> entity, ref EntityEffectEvent<RemoveAccess> args)
    {
        if (!_id.TryFindIdCard(entity.Owner, out var idCard))
            return;

        _access.TrySetTags(idCard, new List<ProtoId<AccessLevelPrototype>>());
    }
}

public sealed partial class RemoveAccess : EntityEffectBase<RemoveAccess>
{
    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => "Removes all target access.";
}
