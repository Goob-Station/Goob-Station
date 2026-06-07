using Content.Goobstation.UIKit.UserInterface;

namespace Content.Goobstation.Client.UI;

public sealed class UserInterfaceEventsSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ButtonTagPressedEvent>(OnPressed);
    }

    private void OnPressed(ref ButtonTagPressedEvent ev)
    {
        RaiseNetworkEvent(new Common.Heretic.ButtonTagPressedEvent(ev.Id, ev.User, ev.Coords));
    }
}
