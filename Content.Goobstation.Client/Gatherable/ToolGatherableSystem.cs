// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Gatherable;

namespace Content.Goobstation.Client.Gatherable;

public sealed class ToolGatherableSystem : SharedToolGatherableSystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    protected override void Gather(Entity<ToolGatherableComponent> ent, EntityUid user)
    {
        base.Gather(ent, user);

        // only predict the entity being destroyed TODO replace with PredictedDel(ent) after engine update
        _transform.DetachEntity(ent.Owner, Transform(ent));
        // server will do the sound drops etc don't try to predict any of that
    }
}
