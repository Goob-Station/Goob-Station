// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Wizard.Components;
using Content.Shared._Goobstation.Wizard.Projectiles;

namespace Content.Goobstation.Shared.Wizard.Systems;

public sealed class EntityTrailSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EntityTrailComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<EntityTrailComponent> ent, ref ComponentInit args)
    {
        var (uid, comp) = ent;
        if (!TryComp(uid, out TrailComponent? trail))
            return;

        trail.RenderedEntity = uid;
        Dirty(uid, trail);
    }
}