using Content.Goobstation.Shared.Gangwars.Components;
using Content.Goobstation.Shared.Gangwars.Events;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Gangwars;

/// <summary>
/// Client-only visual handling for the gang spray can.
/// </summary>
public sealed partial class ClientGangSprayCanSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GangSprayCanComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(Entity<GangSprayCanComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!_appearance.TryGetData<bool>(ent, GangSprayCanVisuals.Empty, out var empty, args.Component) || !empty)
            return;

        _sprite.LayerSetRsiState((ent, args.Sprite), 0, "crushed");
    }
}
