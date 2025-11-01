using Content.Goobstation.Shared.Ranching.Food;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Ranching.Food;

public sealed class FeedSackSystem : SharedFeedSackSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    protected override void ChangeFeedColour(Color color, EntityUid feedUid)
    {
        base.ChangeFeedColour(color, feedUid);

        var map = _sprite.LayerMapGet(feedUid, SeedColor.Color);
        _sprite.LayerSetColor(feedUid, map, color);
    }
}
