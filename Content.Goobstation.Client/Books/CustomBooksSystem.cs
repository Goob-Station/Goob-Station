using Content.Goobstation.Client.Books.Ui;
using Content.Goobstation.Shared.Books;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Books;

public sealed partial class CustomBooksSystem : SharedCustomBooksSystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IUserInterfaceManager _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CustomBookComponent, AppearanceChangeEvent>(OnBookAppearanceChange);

        SubscribeNetworkEvent<OpenPendingBooksListMessage>(OnOpenPending);
        SubscribeNetworkEvent<PopulatePendingBooksMenuMessage>(OnPopulatePending);
    }

    private void OnBookAppearanceChange(Entity<CustomBookComponent> ent, ref AppearanceChangeEvent args)
    {
        if (!_appearance.TryGetData<Dictionary<string, (ResPath Path, string State)>>(ent.Owner, CustomBookVisuals.Visuals, out var layers))
            return;

        foreach (var (map, newLayer) in layers)
        {
            if (!_sprite.LayerMapTryGet(ent.Owner, map, out var layer, false))
                continue;

            _sprite.LayerSetRsi(ent.Owner, layer, newLayer.Path, newLayer.State);
        }
    }

    private void OnOpenPending(OpenPendingBooksListMessage args)
    {
        var controller = _ui.GetUIController<AdminBookVerificationUiController>();
        controller.ToggleMenu();
        controller.Populate(args.Books);
    }

    private void OnPopulatePending(PopulatePendingBooksMenuMessage args)
    {
        var controller = _ui.GetUIController<AdminBookVerificationUiController>();
        controller.Populate(args.Books);
    }
}
