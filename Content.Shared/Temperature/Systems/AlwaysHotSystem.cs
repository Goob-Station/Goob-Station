// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Temperature.Components;

namespace Content.Shared.Temperature.Systems;

public sealed class AlwaysHotSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AlwaysHotComponent, IsHotEvent>(OnIsHot);
    }

    private void OnIsHot(Entity<AlwaysHotComponent> ent, ref IsHotEvent args)
    {
        args.IsHot = true;
    }
}