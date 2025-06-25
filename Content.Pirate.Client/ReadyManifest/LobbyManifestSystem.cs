using Content.Client.Lobby;
using Content.Client.Lobby.UI;
using Robust.Client.UserInterface.Controls;

namespace Content.Pirate.Client.ReadyManifest;

public sealed class LobbyManifestSystem : EntitySystem
{
    [Dependency] private readonly ReadyManifestSystem _readyManifestSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        LobbyState.OnLobbyGuiReady += OnLobbyGuiReady;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        LobbyState.OnLobbyGuiReady -= OnLobbyGuiReady;
    }

    private void OnLobbyGuiReady(LobbyGui gui)
    {
        gui.ManifestButton.OnPressed += OnManifestPressed;
        gui.ManifestButton.Disabled = false;
    }

    private void OnManifestPressed(BaseButton.ButtonEventArgs args)
    {
        _readyManifestSystem.RequestReadyManifest();
    }
}
