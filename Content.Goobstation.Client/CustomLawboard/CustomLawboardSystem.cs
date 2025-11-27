using Content.Goobstation.Shared.CustomLawboard;

namespace Content.Goobstation.Client.CustomLawboard;

public sealed class CustomLawboardSystem : SharedCustomLawboardSystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void DirtyUI(EntityUid uid, CustomLawboardComponent? thermoMachine, UserInterfaceComponent? ui = null)
    {
        if (_ui.TryGetOpenUi<CustomLawboardBoundInterface>(uid, CustomLawboardUiKey.Key, out var bui))
        {
            bui.Update();
        }
    }
}
