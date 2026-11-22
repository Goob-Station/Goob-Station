using Content.Goobstation.Shared.RoundIntro;

namespace Content.Client.GameTicking.Managers;

public sealed partial class ClientGameTicker
{
    private bool GoobJoinGameAttempt()
    {
        var ev = new ClientJoinGameAttemptEvent();
        RaiseLocalEvent(ref ev);
        return ev.Handled;
    }
}
