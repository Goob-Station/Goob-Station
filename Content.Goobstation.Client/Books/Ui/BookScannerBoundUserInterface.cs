using Content.Goobstation.Shared.Books;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Books.Ui;

[UsedImplicitly]
public sealed class BookScannerBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private BookScannerWindow? _menu;

    public BookScannerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<BookScannerWindow>();

        _menu.StartPressed += () =>
        {
            SendMessage(new StartBookScanMessage());
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not BookScannerUiState cast)
            return;

        _menu?.Populate(cast.Author, cast.Genre, cast.Title, cast.Desc, cast.Cooldown);
    }
}
