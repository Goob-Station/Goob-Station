using Content.Goobstation.Shared.Books;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Books.Ui;

[UsedImplicitly]
public sealed class BookBinderBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    [ViewVariables]
    private BookBinderWindow? _menu;

    [ViewVariables]
    private BookBindingCustomizationMenu? _customMenu;

    public BookBinderBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<BookBinderWindow>();
        _menu.OnClose += () => _customMenu?.Close();

        _menu.CreatePressed += (title, genre, author, desc, pages, binding) =>
        {
            SendMessage(new CreateBookMessage(title, genre, author, pages, desc, binding));
        };
        _menu.CustomizeBookPressed += args =>
        {
            if (_customMenu != null)
                return;

            _customMenu = new();
            _customMenu.InitSelectors(args);

            _customMenu.OnClose += () => _customMenu = null;
            _customMenu.CreatePressed += args =>
            {
                _menu.Layers = args;
                _customMenu.Close();
            };

            _customMenu.OpenCenteredRight();
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not BookBinderUiState cast)
            return;

        _menu?.Populate(cast.Pages);
    }
}
