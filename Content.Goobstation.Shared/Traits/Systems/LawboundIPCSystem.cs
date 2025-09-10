using Content.Shared.Silicons.Laws.Components;

namespace Content.Goobstation.Shared.Traits.Systems;

public sealed class LawboundIPCSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SiliconLawBoundComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, SiliconLawBoundComponent comp, ref ComponentStartup args)
    {
        if (_ui.HasUi(uid, SiliconLawsUiKey.Key))
            return;

        _ui.SetUi(uid, SiliconLawsUiKey.Key, new InterfaceData("SiliconLawBoundUserInterface"));
    }

}
