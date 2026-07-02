using Content.Goobstation.Client.Teleport.Ui;
using Content.Goobstation.Shared.Teleportation.Components;
using Content.Shared.Xenoarchaeology.Equipment.Components;

namespace Content.Goobstation.Client.Teleport;

public sealed class TelesciClientSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TelesciComputerComponent, AfterAutoHandleStateEvent>(OnComputerAfterAutoHandleState);
        SubscribeLocalEvent<TelesciTeleporterComponent, AfterAutoHandleStateEvent>(OnTeleportAfterAutoHandleState);
    }

    private void OnComputerAfterAutoHandleState(Entity<TelesciComputerComponent> ent, ref  AfterAutoHandleStateEvent arg)
    {
        if (_ui.TryGetOpenUi<TelesciConsoleBoundUserInterface>(ent.Owner, TelesciUiKey.Key, out var bui))
            bui.Update(ent);
    }

    private void OnTeleportAfterAutoHandleState(Entity<TelesciTeleporterComponent> ent, ref  AfterAutoHandleStateEvent arg)
    {
        if (ent.Comp.Computer == null)
            return;

        if (!TryComp<TelesciComputerComponent>(ent.Comp.Computer.Value, out var computer))
            return;

        if (_ui.TryGetOpenUi<TelesciConsoleBoundUserInterface>(ent.Comp.Computer.Value, TelesciUiKey.Key, out var bui))
            bui.Update((ent.Comp.Computer.Value, computer));
    }
}
