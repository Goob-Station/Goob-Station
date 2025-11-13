using System.Linq;
using Content.Shared._Moffstation.BladeServer;
using Content.Shared.Input;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Input;

namespace Content.Client._Moffstation.BladeServer.UI;

[UsedImplicitly]
public sealed class BladeServerRackBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private readonly IEntityManager _entityMan = default!;

    private BladeServerRackWindow? _window;

    protected override void Open()
    {
        if (!EntMan.TryGetComponent<BladeServerRackComponent>(Owner, out var comp))
            return;

        base.Open();

        _window = this.CreateWindow<BladeServerRackWindow>();
        _window.OnBladeServerPressed += OnServerBladePressed;
        _window.OnEjectPressed += idx => SendPredictedMessage(new BladeServerRackEjectPressedMessage(idx));
        _window.OnInsertPressed += idx => SendPredictedMessage(new BladeServerRackInsertPressedMessage(idx));
        _window.OnPowerPressed += OnPowerPressed;

        _window.PopulateSlots(comp.BladeSlots.Select(it => (it.Item, IsPowered: it.IsPowerEnabled, it.Slot.Locked)));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window == null ||
            state is not BladeServerRackBoundUserInterfaceState cast)
            return;

        _window.PopulateSlots(cast.Slots.Select(it => (_entityMan.GetEntity(it.Entity), it.Powered, it.Locked)));
    }

    private void OnServerBladePressed(int slotIndex, GUIBoundKeyEventArgs args)
    {
        var bladeServer = EntMan.System<BladeServerSystem>();

        if (args.Function == EngineKeyFunctions.Use)
        {
            SendPredictedMessage(new BladeServerRackUseMessage(slotIndex));
            args.Handle();
        }
        else if (args.Function == ContentKeyFunctions.ActivateItemInWorld)
        {
            SendPredictedMessage(new BladeServerRackActivateInWorldMessage(slotIndex, alternate: false));
            args.Handle();
        }
        else if (args.Function == ContentKeyFunctions.AltActivateItemInWorld)
        {
            SendPredictedMessage(new BladeServerRackActivateInWorldMessage(slotIndex, alternate: true));
            args.Handle();
        }
        else if (args.Function == ContentKeyFunctions.ExamineEntity &&
                 bladeServer.TryExamine(Owner, slotIndex) ||
                 args.Function == EngineKeyFunctions.UseSecondary &&
                 bladeServer.TryUseSecondary(Owner, slotIndex))
        {
            args.Handle();
        }
    }

    private void OnPowerPressed(int slotIndex)
    {
        var bladeServer = EntMan.System<BladeServerSystem>();
        if (bladeServer.IsSlotPowerEnabled(Owner, slotIndex) is not { } powered)
            return;

        SendPredictedMessage(new BladeServerRackPowerPressedMessage(slotIndex, !powered));
    }
}
