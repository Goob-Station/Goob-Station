
using Content.Shared.Popups;

namespace Content.Shared.DeltaV.Abilities;
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
