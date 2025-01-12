using Content.Shared.GameTicking.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Server.GameTicking;

// Goobstation - this file is heavily modified to add credits for lobby backgrounds
public sealed partial class GameTicker
{
    [ViewVariables]
    public LobbyBackgroundPrototype? LobbyBackground { get; private set; }

    [ViewVariables]
    private List<LobbyBackgroundPrototype> _lobbyBackgrounds = [];

    private static readonly string[] WhitelistedBackgroundExtensions = new string[] {"png", "jpg", "jpeg", "webp"};

    private void InitializeLobbyBackground()
    {
        foreach (var prototype in _prototypeManager.EnumeratePrototypes<LobbyBackgroundPrototype>())
        {
            if (!WhitelistedBackgroundExtensions.Contains(prototype.Background.Extension))
            {
                _sawmill.Warning($"Lobby background '{prototype.ID}' has an invalid extension '{prototype.Background.Extension}' and will be ignored.");
                continue;
            }

            _lobbyBackgrounds.Add(prototype);
        }

        RandomizeLobbyBackground();
    }

    private void RandomizeLobbyBackground() {
        LobbyBackground = _lobbyBackgrounds!.Any() ? _robustRandom.Pick(_lobbyBackgrounds!) : null;
    }
}
