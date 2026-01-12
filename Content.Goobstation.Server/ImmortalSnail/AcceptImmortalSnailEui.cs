using Content.Goobstation.Shared.ImmortalSnail;
using Content.Server.EUI;
using Content.Shared.Eui;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.ImmortalSnail;

public sealed class AcceptImmortalSnailEui : BaseEui
{
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly EntityUid _ruleEntity;
    private readonly ImmortalSnailRuleSystem _snailSystem;
    private readonly TimeSpan _endTime;
    private const float OfferDuration = 30f;

    public AcceptImmortalSnailEui(EntityUid ruleEntity, ICommonSession session, ImmortalSnailRuleSystem snailSystem)
    {
        IoCManager.InjectDependencies(this);
        _ruleEntity = ruleEntity;
        _snailSystem = snailSystem;
        _endTime = _timing.CurTime + TimeSpan.FromSeconds(OfferDuration);
    }

    public override void Opened()
    {
        base.Opened();
        StateDirty();
    }

    public override EuiStateBase GetNewState()
    {
        return new AcceptImmortalSnailEuiState(_endTime);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not AcceptImmortalSnailChoiceMessage choice)
            return;

        if (choice.Button == AcceptImmortalSnailUiButton.Accept)
            _snailSystem.OnPlayerAcceptOffer(_ruleEntity, Player);
        else
            _snailSystem.OnPlayerDeclineOffer(_ruleEntity);

        Close();
    }

    public bool HasTimedOut()
    {
        return _timing.CurTime >= _endTime;
    }
}
