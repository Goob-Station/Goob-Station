using Content.Goobstation.Shared.Phones.Components;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.Phones.UI;

public sealed class PhoneNameChangeUI : BoundUserInterface
{
    private ChangePhoneName? _menu;

    public PhoneNameChangeUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<ChangePhoneName>();

        _menu.OnTextChanged += i =>
        {
            SendMessage(new PhoneNameChangedMessage(i));
        };
        _menu.OnCategoryChanged += i =>
        {
            SendMessage(new PhoneCategoryChangedMessage(i));
        };
    }
}
