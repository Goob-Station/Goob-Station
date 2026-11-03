using Content.Goobstation.Shared.CustomLawboard;

namespace Content.Goobstation.Client.CustomLawboard;

public sealed partial class CustomLawboardSystem : SharedCustomLawboardSystem
{
    [Dependency] private SharedUserInterfaceSystem _ui = default!;

    protected override void DirtyUI(EntityUid uid, CustomLawboardComponent? customLawboard, UserInterfaceComponent? ui = null)
    {
        if (_ui.TryGetOpenUi<CustomLawboardBoundInterface>(uid, CustomLawboardUiKey.Key, out var bui))
        {
            bui.Update();
        }
    }
}
