using Content.Client.Items.UI;
using Content.Client.Message;
using Content.Client.Stylesheets;
using Content.Shared._White.Blink;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._White.Blink;

public sealed class BlinkStatusControl : PollingItemStatusControl<BlinkStatusControl.Data>
{
    private readonly Entity<BlinkComponent> _parent;
    private readonly RichTextLabel _label;

    public BlinkStatusControl(Entity<BlinkComponent> parent)
    {
        _parent = parent;
        _label = new RichTextLabel { StyleClasses = { StyleNano.StyleClassItemStatus } };
        AddChild(_label);

        UpdateDraw();
    }

    public record struct Data(bool IsActive);

    protected override Data PollData()
    {
        return new Data(_parent.Comp.IsActive);
    }

    protected override void Update(in Data data)
    {
        var message = data.IsActive ? "blink-component-control-active" : "blink-component-control-inactive";
        _label.SetMarkup(Loc.GetString(message));
    }
}
