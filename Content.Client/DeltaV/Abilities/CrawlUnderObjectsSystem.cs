using Content.Shared.DeltaV.Abilities;
using Content.Shared.Popups;
using Robust.Client.GameObjects;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Client.DeltaV.Abilities;

public sealed partial class HideUnderTableAbilitySystem : SharedCrawlUnderObjectsSystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CrawlUnderObjectsComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(EntityUid uid,
        CrawlUnderObjectsComponent component,
        AppearanceChangeEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        _appearance.TryGetData(uid, SneakMode.Enabled, out bool enabled);
        if (enabled)
        {
            if (component.OriginalDrawDepth != null)
                return;

            component.OriginalDrawDepth = sprite.DrawDepth;
            sprite.DrawDepth = (int) DrawDepth.SmallMobs;
        }
        else
        {
            if (component.OriginalDrawDepth == null)
                return;

            sprite.DrawDepth = (int) component.OriginalDrawDepth;
            component.OriginalDrawDepth = null;
        }
    }
}
