using Content.Shared._Funkystation.WallStains;
using Content.Shared._Funkystation.WallStains.Components;
using Robust.Client.GameObjects;

namespace Content.Client._Funkystation.WallStains.Systems;

public sealed partial class WallStainVisualsSystem : EntitySystem
{
    [Dependency] private SharedAppearanceSystem _appearance = null!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WallStainComponent, AppearanceChangeEvent>(OnAppearanceChanged);
    }

    private void OnAppearanceChanged(EntityUid uid, WallStainComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (_appearance.TryGetData<Color>(uid, WallStainVisuals.Color, out var color, args.Component))
        {
            args.Sprite.Color = color;
        }

        if (_appearance.TryGetData<string>(uid, WallStainVisuals.State, out var state, args.Component))
        {
            args.Sprite.LayerSetState(0, state);
        }
    }
}
