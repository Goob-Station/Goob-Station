using Content.Client.UserInterface.Controls;
using Content.Goobstation.Shared.Cult.Events;
using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.Client.Cult;

public sealed class NameSelectorBUI(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private readonly FancyWindow _window = new();

    protected override void Open()
    {
        base.Open();

        FormWindow();
        _window.OpenCentered();
        _window.OnClose += Close;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _window.Close();
    }

    private void FormWindow()
    {
        var container = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical
        };

        var label = new Label
        {
            Text = Loc.GetString("name-selector-title")
        };

        var lineEdit = new LineEdit
        {
            HorizontalExpand = true
        };

        var button = new Button
        {
            Text = Loc.GetString("name-selector-accept-button")
        };

        button.OnButtonUp += _ =>
        {
            var msg = new NameSelectedMessage(lineEdit.Text);
            SendMessage(msg);
            Close();
        };

        container.AddChild(label);
        container.AddChild(lineEdit);
        container.AddChild(button);

        _window.AddChild(container);
    }
}
