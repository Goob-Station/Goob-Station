using Content.Goobstation.Shared.Virology;
using Content.Server.DeviceNetwork;
using Content.Server.DeviceNetwork.Systems;
using Robust.Server.GameObjects;

namespace Content.Goobstation.Server.Virology;

public sealed class VirologyConsoleSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VirologyConsoleComponent, DeviceNetworkPacketEvent>(OnPacketReceived);
        SubscribeLocalEvent<VirologyConsoleComponent, BoundUIOpenedEvent>(OnUIOpened);
    }

    private void OnPacketReceived(EntityUid uid, VirologyConsoleComponent component, DeviceNetworkPacketEvent args)
    {
        var payload = args.Data;

        // Check command
        if (!payload.TryGetValue(DeviceNetworkConstants.Command, out string? command))
            return;

        if (command != DeviceNetworkConstants.CmdUpdatedState)
            return;

        if (!payload.TryGetValue(VirologyConsoleConstants.NET_STATUS_COLLECTION, out List<DiseaseInformation>? diseaseInfo))
            return;

        component.DiseaseInfo = diseaseInfo;
        UpdateUserInterface(uid, component);
    }

    private void OnUIOpened(EntityUid uid, VirologyConsoleComponent component, BoundUIOpenedEvent args)
    {
        UpdateUserInterface(uid, component);
    }

    private void UpdateUserInterface(EntityUid uid, VirologyConsoleComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!_uiSystem.IsUiOpen(uid, VirologyConsoleUIKey.Key))
            return;

        // update all disease info
        _uiSystem.SetUiState(uid, VirologyConsoleUIKey.Key, new VirologyConsoleState(component.DiseaseInfo));
    }
}
