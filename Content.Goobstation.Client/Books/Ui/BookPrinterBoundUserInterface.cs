using Content.Goobstation.Shared.Books;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Books.Ui;

[UsedImplicitly]
public sealed class BookPrinterBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    [ViewVariables]
    private BookPrinterMenu? _menu;

    public BookPrinterBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<BookPrinterMenu>();

        _menu.PrintBook += () =>
        {
            if (_menu.SelectedBook != null)
                SendMessage(new PrintBookMessage(_menu.SelectedBook));
        };

        _menu.DeleteBook += () =>
        {
            if (_menu.Selected != -1)
                SendMessage(new DeleteBookMessage(_menu.Selected));
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (_menu == null)
            return;

        if (state is not BookPrinterUiState cast)
            return;

        _menu.Populate(cast.Books);
        _menu.AllowDeleting = cast.AllowDeleting;
        _menu.Cooldown = cast.Cooldown;
    }
}
