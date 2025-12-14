using Content.Goobstation.Shared.Hastur.Components;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Hastur;

public sealed class HasturDevouringSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        // FIX: Subscribing to HasturDevourComponent instead of the old Devouring one
        SubscribeLocalEvent<HasturDevourComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(Entity<HasturDevourComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (_appearance.TryGetData<bool>(ent.Owner, DevourVisuals.Devouring, out var devouring, args.Component))
        {
            if (devouring)
                _spriteSystem.LayerSetRsiState((ent.Owner, args.Sprite), 0, ent.Comp.Devouring);
            else
                _spriteSystem.LayerSetRsiState((ent.Owner, args.Sprite), 0, ent.Comp.Normal);
        }
    }
}
