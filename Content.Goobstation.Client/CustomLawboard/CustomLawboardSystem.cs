using Content.Goobstation.Shared.CustomLawboard;

namespace Content.Goobstation.Client.CustomLawboard;

public sealed class CustomLawboardSystem : SharedCustomLawboardSystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    protected override void DirtyUI(EntityUid uid, CustomLawboardComponent? customLawboard, UserInterfaceComponent? ui = null)
    {
        if (_ui.TryGetOpenUi<CustomLawboardBoundInterface>(uid, CustomLawboardUiKey.Key, out var bui))
        {
            bui.Update();
        }
    }
}
