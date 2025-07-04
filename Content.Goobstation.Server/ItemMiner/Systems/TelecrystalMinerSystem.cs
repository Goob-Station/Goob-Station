// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.ItemMiner;
using Content.Server.Chat.Systems;
using Content.Server.Station.Components;

namespace Content.Goobstation.Server.ItemMiner;

public sealed class TelecrystalMinerSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TelecrystalMinerComponent, ItemMinerCheckEvent>(OnCheck);
        SubscribeLocalEvent<TelecrystalMinerComponent, ItemMinedEvent>(OnMined);
    }

    private void OnCheck(Entity<TelecrystalMinerComponent> ent, ref ItemMinerCheckEvent args)
    {
        // no worky on non-main stations
        if (!HasComp<BecomesStationComponent>(Transform(ent).ParentUid))
            args.Cancelled = true;
    }

    private void OnMined(Entity<TelecrystalMinerComponent> ent, ref ItemMinedEvent args)
    {
        ent.Comp.Accumulated += args.Count;
        if (ent.Comp.Notified)
            return;

        if (ent.Comp.Accumulated >= ent.Comp.Required)
        {
            ent.Comp.Notified = true;

            _chat.DispatchStationAnnouncement(
                Transform(ent).ParentUid,
                Loc.GetString(ent.Comp.Announcement),
                playDefaultSound: true,
                colorOverride: Color.Yellow
            );
        }
    }
}
