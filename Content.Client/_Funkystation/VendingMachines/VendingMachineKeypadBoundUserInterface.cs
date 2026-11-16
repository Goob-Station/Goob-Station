using Content.Client._Funkystation.VendingMachines.UI;
using Content.Client.VendingMachines;
using Content.Shared.Access.Systems;
using Content.Shared.VendingMachines;
using JetBrains.Annotations;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using System.Linq;
using Content.Shared._Funkystation.VendingMachines;

namespace Content.Client._Funkystation.VendingMachines;

[UsedImplicitly]
public sealed class VendingMachineKeypadBoundUserInterface(EntityUid owner, Enum uiKey)
    : BoundUserInterface(owner, uiKey), IVendingMachineBoundUi
{
    [ViewVariables]
    private VendingMachineKeypadMenu? _menu;

    [ViewVariables]
    private List<VendingMachineInventoryEntry> _cachedInventory = new();

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindowCenteredLeft<VendingMachineKeypadMenu>();
        _menu.VendingMachineOwner = Owner;
        _menu.User = IoCManager.Resolve<IPlayerManager>().LocalSession?.AttachedEntity;
        _menu.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
        _menu.OnCodeEntered += OnCodeEntered;
        _menu.OnAudioPlayed += OnAudioPlayed;
        Refresh();
    }

    public void Refresh()
    {
        var enabled = EntMan.TryGetComponent(Owner, out VendingMachineComponent? bendy) && !bendy.Ejecting;

        var system = EntMan.System<VendingMachineSystem>();
        _cachedInventory = system.GetAllInventory(Owner);

        _menu?.Populate(_cachedInventory, enabled);
    }

    public void UpdateAmounts()
    {
        var enabled = EntMan.TryGetComponent(Owner, out VendingMachineComponent? bendy) && !bendy.Ejecting;

        var system = EntMan.System<VendingMachineSystem>();
        _cachedInventory = system.GetAllInventory(Owner);
        _menu?.UpdateAmounts(_cachedInventory, enabled);
    }

    private void OnAudioPlayed(VendingMachineKeypadSound type, float pitch)
    {
        SendMessage(new VendingMachineKeypadAudioMessage(type, pitch));
    }

    private bool OnCodeEntered(int slotIndex)
    {
        var selectedItem = _cachedInventory.ElementAtOrDefault(slotIndex);

        if (selectedItem == null)
            return false;

        // check access
        var playerManager = IoCManager.Resolve<IPlayerManager>();
        if (playerManager.LocalSession?.AttachedEntity is { } player)
        {
            var accessSystem = EntMan.System<AccessReaderSystem>();
            if (!accessSystem.IsAllowed(player, Owner))
            {
                return false;
            }
        }

        // optimistic
        _menu?.PlayVendAnimation(slotIndex);

        SendPredictedMessage(new VendingMachineEjectMessage(selectedItem.Type, selectedItem.ID));
        return true;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        if (_menu == null)
            return;

        _menu.OnCodeEntered -= OnCodeEntered;
        _menu.OnAudioPlayed -= OnAudioPlayed;
        _menu.OnClose -= Close;
        _menu.Close();
    }
}
