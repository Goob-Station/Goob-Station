// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Popups;

namespace Content.Shared._DV.Abilities;
public abstract class SharedCrawlUnderObjectsSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CrawlUnderObjectsComponent, CrawlingUpdatedEvent>(OnCrawlingUpdated);
    }

    private void OnCrawlingUpdated(EntityUid uid,
        CrawlUnderObjectsComponent component,
        CrawlingUpdatedEvent args)
    {
        if (args.Enabled)
            _popup.PopupEntity(Loc.GetString("crawl-under-objects-toggle-on"), uid);
        else
            _popup.PopupEntity(Loc.GetString("crawl-under-objects-toggle-off"), uid);
    }
}