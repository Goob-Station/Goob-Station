using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Client._Lavaland.Megafauna.Mercury;

public sealed class ClientPhaseConversionSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PhaseConversionComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(Entity<PhaseConversionComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!_appearance.TryGetData<bool>(ent.Owner, PhaseConversionVisuals.IsRanged, out var isRanged, args.Component))
            return;

        var sprite = ent.Comp.MeleeSprite;
        if (isRanged)
        {
            sprite = ent.Comp.RangedSprite;
        }

        if (sprite is SpriteSpecifier.Rsi rsi)
        {
            _spriteSystem.LayerSetRsiState((ent.Owner, args.Sprite), 0, rsi.RsiState);
        }
    }
}
