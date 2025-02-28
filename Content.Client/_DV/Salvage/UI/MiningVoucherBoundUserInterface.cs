using Content.Shared._DV.Salvage;
using Robust.Client.UserInterface;

namespace Content.Client._DV.Salvage.UI;

public sealed class MiningVoucherBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private MiningVoucherMenu? _menu;

    public MiningVoucherBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<MiningVoucherMenu>();
        _menu.SetEntity(Owner);
        _menu.OnSelected += i =>
        {
            SendMessage(new MiningVoucherSelectMessage(i));
            Close();
        };
    }
}
