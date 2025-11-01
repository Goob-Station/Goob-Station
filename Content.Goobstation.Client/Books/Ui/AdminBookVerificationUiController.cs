using Content.Goobstation.Shared.Books;
using Robust.Client.UserInterface.Controllers;

namespace Content.Goobstation.Client.Books.Ui;

public sealed class AdminBookVerificationUiController : UIController
{
    private AdminBookVerificationMenu? _menu;

    public void ToggleMenu()
    {
        if (_menu != null)
        {
            _menu.Close();
            return;
        }

        _menu = new();
        _menu.OpenCentered();
        _menu.OnClose += () => _menu = null;
        _menu.ApproveBook += () =>
        {
            var ev = new ApproveBookMessage(_menu.Selected);
            EntityManager.RaisePredictiveEvent(ev);
        };
        _menu.DeclineBook += () =>
        {
            var ev = new DeclineBookMessage(_menu.Selected);
            EntityManager.RaisePredictiveEvent(ev);
        };
    }

    public void Populate(Dictionary<int, BookData> books)
    {
        _menu?.Populate(books);
    }
}
