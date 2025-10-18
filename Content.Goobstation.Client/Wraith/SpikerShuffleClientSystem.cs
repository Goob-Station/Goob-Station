using Content.Goobstation.Shared.Wraith.Minions.Harbinger;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Wraith;

/// <summary>
/// This handles appearance for shuffling
/// </summary>
public sealed class SpikerShuffleClientSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpikerShuffleComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(Entity<SpikerShuffleComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (_appearance.TryGetData<bool>(ent.Owner, ShuffleVisuals.Shuffling, out var shuffling, args.Component))
        {
            if (shuffling)
                _spriteSystem.LayerSetRsiState((ent.Owner, args.Sprite), 0, ent.Comp.Shuffling);
            else
                _spriteSystem.LayerSetRsiState((ent.Owner, args.Sprite), 0, ent.Comp.Normal);
        }
    }
}
