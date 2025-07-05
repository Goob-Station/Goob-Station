// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Shuttles.Events;

namespace Content.Server._DV.Shipyard;

public sealed class MapDeleterShuttleSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MapDeleterShuttleComponent, FTLStartedEvent>(OnFTLStarted);
    }

    private void OnFTLStarted(Entity<MapDeleterShuttleComponent> ent, ref FTLStartedEvent args)
    {
        if (ent.Comp.Enabled)
            Del(args.FromMapUid);
        RemComp<MapDeleterShuttleComponent>(ent); // prevent the shuttle becoming a WMD
    }

    public void Enable(EntityUid shuttle)
    {
        EnsureComp<MapDeleterShuttleComponent>(shuttle).Enabled = true;
    }
}
