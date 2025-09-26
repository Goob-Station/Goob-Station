using Content.Shared.CCVar;
using Content.Shared.Chat.TypingIndicator;
using Robust.Shared.Prototypes;

namespace Content.Client.Chat.TypingIndicator;

public sealed partial class TypingIndicatorSystem
{
    private bool _shouldShowTyping;

    private void InitializeAlternateTyping()
    {
        Subs.CVar(_cfg, CCVars.ChatShowTypingIndicator, OnShowTypingChangedAlternate);
    }

    private void OnShowTypingChangedAlternate(bool showTyping)
    {
        _shouldShowTyping = showTyping;
    }

    /// <summary>
    /// DeltaV: Client can type with alternate indicators
    /// </summary>
    /// <param name="protoId">The TypingIndicator to show in place of the normal TypingIndicator</param>
    public void ClientAlternateTyping(ProtoId<TypingIndicatorPrototype> protoId)
    {
        if (_shouldShowTyping)
            return;

        _isClientTyping = true;
        _lastTextChange = _time.CurTime;
        RaisePredictiveEvent(new TypingChangedEvent(TypingIndicatorState.Typing, protoId));
    }
}
