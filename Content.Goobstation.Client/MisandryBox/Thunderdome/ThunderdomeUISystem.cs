using Content.Goobstation.Shared.MisandryBox.Thunderdome;

namespace Content.Goobstation.Client.MisandryBox.Thunderdome;

public sealed class ThunderdomeUISystem : EntitySystem
{
    private ThunderdomeRevivalWindow? _revivalWindow;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<ThunderdomeRevivalOfferEvent>(OnRevivalOffer);
    }

    private void OnRevivalOffer(ThunderdomeRevivalOfferEvent ev)
    {
        _revivalWindow?.Close();
        _revivalWindow = new ThunderdomeRevivalWindow();
        _revivalWindow.OnAccepted += () =>
        {
            RaiseNetworkEvent(new ThunderdomeRevivalAcceptEvent());
        };
        _revivalWindow.OpenCentered();
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _revivalWindow?.Close();
        _revivalWindow = null;
    }
}
