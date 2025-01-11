using Robust.Shared.Random;
using System.Linq;
using Content.Shared._OS;

namespace Content.Server.GameTicking;

public sealed partial class GameTicker
{
    [ViewVariables]
    public string? LobbyBackground { get; private set; }

    [ViewVariables]
    private List<string>? _lobbyBackgrounds; // OpenSpace edit

    private void InitializeLobbyBackground()
    {
        _lobbyBackgrounds = _prototypeManager.EnumeratePrototypes<AnimatedLobbyScreenPrototype>() // OpenSpace edit
            .Select(x => x.Path) // OpenSpace edit
            .ToList();

        RandomizeLobbyBackground();
    }

    private void RandomizeLobbyBackground()
    {
        LobbyBackground = _lobbyBackgrounds!.Any() ? _robustRandom.Pick(_lobbyBackgrounds!) : null; // OpenSpace edit
    }
}
