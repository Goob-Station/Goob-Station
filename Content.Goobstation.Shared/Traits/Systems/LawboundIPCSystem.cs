using Content.Shared.Silicons.Laws.Components;

namespace Content.Server.Silicons;

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

        var ent = new Entity<UserInterfaceComponent?>(uid, CompOrNull<UserInterfaceComponent>(uid));
        _ui.SetUi(ent, SiliconLawsUiKey.Key, new InterfaceData("SiliconLawBoundUserInterface"));
    }

}
