using Content.Goobstation.Shared.Ranching.Food;
using Content.Shared.Chemistry;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Ranching.Food;

public sealed class FoodProducerClientSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FeedSackComponent, AppearanceChangeEvent>(OnSackAppearanceChanged);
    }

    private void OnSackAppearanceChanged(Entity<FeedSackComponent> feedSack, ref AppearanceChangeEvent args)
    {

    }
}
